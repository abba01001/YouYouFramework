using System;
using Cysharp.Threading.Tasks;
using GameScripts;
using Main;
using OctoberStudio.Abilities;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using OctoberStudio.Timeline.Bossfight;
using OctoberStudio.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

namespace OctoberStudio
{
    public class StageController : MonoBehaviour
    {
        private static StageController instance;

        [SerializeField] StagesDatabase database;
        [SerializeField] PlayableDirector director;
        [SerializeField] EnemiesSpawner spawner;
        [SerializeField] StageFieldManager fieldManager;
        [SerializeField] ExperienceManager experienceManager;
        [SerializeField] DropManager dropManager;
        [SerializeField] AbilityManager abilityManager;
        [SerializeField] PoolsManager poolsManager;
        [SerializeField] CameraManager cameraManager;

        public static EnemiesSpawner EnemiesSpawner => instance.spawner;
        public static ExperienceManager ExperienceManager => instance.experienceManager;
        public static AbilityManager AbilityManager => instance.abilityManager;
        public static StageFieldManager FieldManager => instance.fieldManager;
        public static PoolsManager PoolsManager => instance.poolsManager;
        public static WorldSpaceTextManager WorldSpaceTextManager
        {
            get
            {
                FormGame formGame = GameEntry.UI.GetUIForm<FormGame>();
                return formGame.WorldSpaceTextManager;
            }
        }

        public static CameraManager CameraController => instance.cameraManager;
        public static DropManager DropManager => instance.dropManager;

        [Header("Testing")]
        [SerializeField] PresetData testingPreset;

        public static StageData Stage { get; private set; }

        private void Awake()
        {
            instance = this;
            GameEntry.Event.AddEventListener(Constants.EventName.BattleSceneInitFinish,OnLoadingSceneComplete);
        }

        public static void PauseDirector(bool bo)
        {
            if (bo)
            {
                instance.director.Pause();
            }
            else
            {
                instance.director.Play();
            }
        }
        
        private void TimelineStopped(PlayableDirector director)
        {
            if (gameObject.activeSelf)
            {
                // ==============================================
                // 🔴 核心：无尽关卡 → 无限循环，不触发通关
                // ==============================================
                if (Stage.StageMode == StageMode.LoopMode)
                {
                    // 无尽模式：时间轴播放完 → 直接从头重播
                    director.time = 0;
                    director.Play();
                    // 不弹完成界面、不暂停、不保存关卡进度
                    return;
                }

                var maxReachedStageId = GameEntry.Data.StageSaveData.maxReachedStageId;
                var selectedStageId = GameEntry.Data.StageSaveData.selectedStageId;
                if (maxReachedStageId < selectedStageId + 1 && selectedStageId + 1 < database.StagesCount)
                {
                    GameEntry.Data.StageSaveData.SetMaxReachedStageId(selectedStageId + 1);
                }
                GameEntry.Data.StageSaveData.SetStageIsPlaying(true);
                GameEntry.UI.CloseUIForm<FormGame>();
                ShowFormResult(true);
                Time.timeScale = 0;
            }
        }

        public static double GetDirectorTime()
        {
            return instance.director.time;
        }
        
        private void OnGameFailed()
        {
            Time.timeScale = 0;

            GameEntry.Data.StageSaveData.SetStageIsPlaying(false);
            GameEntry.UI.CloseUIForm<FormGame>();
            _ = ShowFormResult(false);
        }

        public async UniTask ShowFormResult(bool win)
        {
            FormResult formResult = await GameEntry.UI.OpenUIForm<FormResult>();
            formResult.ShowResult(win);
            
        }
        
        public static void ResurrectPlayer()
        {
            EnemiesSpawner.DealDamageToAllEnemies(PlayerBehavior.Player.GetDamageValue() * 1000);

            GameEntry.UI.OpenUIForm<FormGame>();
            PlayerBehavior.Player.Revive();
            Time.timeScale = 1;
        }

        public static void ReturnToMainMenu()
        {
            GameController.ExitGame();
        }

        public static FormGame FormGameScreen;
        private void OnLoadingSceneComplete(object userdata)
        {
            HandleInit().Forget();
        }

        private async UniTask HandleInit()
        {
            Debugger.Log("Stage Controller HandleInit");
            Debugger.Log("Stage Controller Start");
            Stage = database.GetStage(GameEntry.Data.StageSaveData.selectedStageId);
            director.playableAsset = Stage.Timeline;

            spawner.Init(director);
            experienceManager.Init(testingPreset);
            dropManager.Init();
            fieldManager.Init(Stage, director);
            cameraManager.Init(Stage);

            PlayerBehavior.Player.onPlayerDied += OnGameFailed;

            director.stopped += TimelineStopped;
            if (testingPreset != null) 
            {
                director.time = testingPreset.StartTime; 
            }
            else
            {
                var time = GameEntry.Data.StageSaveData.time;

                var bossClips = director.GetClips<BossTrack, Boss>();

                for(int i = 0; i < bossClips.Count; i++)
                {
                    var bossClip = bossClips[i];

                    if(time >= bossClip.start && time <= bossClip.end)
                    {
                        time = (float) bossClip.start;
                        break;
                    }
                }

                director.time = time;
            }

            director.Play();
            if (Stage.UseCustomMusic) GameController.ChangeMusic(Stage.MusicName);
            FormGameScreen = await GameEntry.UI.OpenUIForm<FormGame>();
            abilityManager.Init(testingPreset, PlayerBehavior.Player.Data);
        }
        
        private void OnDisable()
        {
            director.stopped -= TimelineStopped;
            GameEntry.Event.RemoveEventListener(Constants.EventName.BattleSceneInitFinish,OnLoadingSceneComplete);

        }
        
    }
}
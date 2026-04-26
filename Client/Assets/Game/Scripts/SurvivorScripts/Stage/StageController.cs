using System;
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
        [SerializeField] WorldSpaceTextManager worldSpaceTextManager;
        [SerializeField] CameraManager cameraManager;

        public static EnemiesSpawner EnemiesSpawner => instance.spawner;
        public static ExperienceManager ExperienceManager => instance.experienceManager;
        public static AbilityManager AbilityManager => instance.abilityManager;
        public static StageFieldManager FieldManager => instance.fieldManager;
        public static PoolsManager PoolsManager => instance.poolsManager;
        public static WorldSpaceTextManager WorldSpaceTextManager => instance.worldSpaceTextManager;
        public static CameraManager CameraController => instance.cameraManager;
        public static DropManager DropManager => instance.dropManager;

        [Header("UI")]
        [SerializeField] GameScreenBehavior gameScreen;
        [SerializeField] StageFailedScreen stageFailedScreen;
        [SerializeField] StageCompleteScreen stageCompletedScreen;

        [Header("Testing")]
        [SerializeField] PresetData testingPreset;

        public static GameScreenBehavior GameScreen => instance.gameScreen;

        public static StageData Stage { get; private set; }

        private StageSave stageSave;
        // 🔴 新增：标记当前是否为【无尽关卡】
        private void Awake()
        {
            instance = this;

            stageSave = GameController.SaveManager.StageData;
        }

        private void Start()
        {
            Stage = database.GetStage(stageSave.SelectedStageId);

            director.playableAsset = Stage.Timeline;

            spawner.Init(director);
            experienceManager.Init(testingPreset);
            dropManager.Init();
            fieldManager.Init(Stage, director);
            abilityManager.Init(testingPreset, PlayerBehavior.Player.Data);
            cameraManager.Init(Stage);

            PlayerBehavior.Player.onPlayerDied += OnGameFailed;

            director.stopped += TimelineStopped;
            if (testingPreset != null) {
                director.time = testingPreset.StartTime; 
            } else
            {
                var time = stageSave.Time;

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
            if (Stage.UseCustomMusic)
            {
                GameController.ChangeMusic(Stage.MusicName);
            }
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

                if (stageSave.MaxReachedStageId < stageSave.SelectedStageId + 1 && stageSave.SelectedStageId + 1 < database.StagesCount)
                {
                    stageSave.SetMaxReachedStageId(stageSave.SelectedStageId + 1);
                }

                stageSave.IsPlaying = false;
                GameController.SaveManager.Save(true);

                gameScreen.Hide();
                stageCompletedScreen.Show();
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

            stageSave.IsPlaying = false;
            GameController.SaveManager.Save(true);

            gameScreen.Hide();
            stageFailedScreen.Show();
        }

        public static void ResurrectPlayer()
        {
            EnemiesSpawner.DealDamageToAllEnemies(PlayerBehavior.Player.GetDamageValue() * 1000);

            GameScreen.Show();
            PlayerBehavior.Player.Revive();
            Time.timeScale = 1;
        }

        public static void ReturnToMainMenu()
        {
            GameController.LoadMainMenu();
        }

        private void OnDisable()
        {
            director.stopped -= TimelineStopped;
        }
    }
}
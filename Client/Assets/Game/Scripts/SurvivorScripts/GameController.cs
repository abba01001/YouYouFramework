using UnityEngine;
using Cysharp.Threading.Tasks;
using GameScripts;

namespace OctoberStudio
{
    using OctoberStudio.Audio;
    
    using OctoberStudio.Input;
    using Upgrades;
    using Vibration;

    public class GameController : MonoBehaviour
    {
        protected static readonly string MAIN_MENU_MUSIC_NAME = "Main Menu Music";

        private static GameController instance;

        protected bool isBattleing;
        public static bool IsBattleing => instance.isBattleing;

        [SerializeField] protected UpgradesManager upgradesManager;
        public static UpgradesManager UpgradesManager => instance.upgradesManager;
        public static IAudioManager AudioManager { get; private set; }
        public static IVibrationManager VibrationManager { get; private set; }
        public static IInputManager InputManager { get; private set; }

        public static AudioSource Music { get; private set; }
        protected virtual void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 120;
        }

        protected virtual void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            InputManager.InputAsset.UI.Click.performed += MusicStartWebGL;
#else

            EasingManager.DoAfter(0.2f, () => Music = AudioManager.PlayMusic(MAIN_MENU_MUSIC_NAME.GetHashCode()));
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        protected virtual void MusicStartWebGL(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            InputManager.InputAsset.UI.Click.performed -= MusicStartWebGL;

            Music = AudioManager.PlayMusic(MAIN_MENU_MUSIC_NAME.GetHashCode());
        }
#endif
        public static void ChangeMusic(string musicName)
        {
            if (Music != null)
            {
                var oldMusic = Music;
                oldMusic.DoVolume(0, 0.3f).SetOnFinish(() => oldMusic.Stop());
            }

            Music = AudioManager.PlayMusic(musicName.GetHashCode());

            if(Music != null)
            {
                var volume = Music.volume;
                Music.volume = 0;
                Music.DoVolume(volume, 0.3f);
            }
        }

        public static void RegisterInputManager(IInputManager inputManager)
        {
            InputManager = inputManager;
        }

        public static void RegisterVibrationManager(IVibrationManager vibrationManager)
        {
            VibrationManager = vibrationManager;
        }

        public static void RegisterAudioManager(IAudioManager audioManager)
        {
            AudioManager = audioManager;
        }

        public static void SetBattleingFlag(bool bo)
        {
            if (bo)
            {
                if (instance != null) instance.isBattleing = true;
            }
            else
            {
                if (instance != null) instance.isBattleing = false;
                if (StageController.Stage.UseCustomMusic)
                {
                    ChangeMusic(MAIN_MENU_MUSIC_NAME);
                }
            }
        }

        protected virtual void OnApplicationFocus(bool focus)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (focus) { 
                EasingManager.DoAfter(0.1f, () => { 
                    if (!Music.isPlaying)
                    {
                        Music = AudioManager.AudioDatabase.Music.Play(true);
                    }
                });
            } 
#endif
        }
        
        
        public static void ExitGame()
        {
            int addCount = GameEntry.Data.PlayerRoleData.GetProps((int)PropEnum.BattleCoin);
            GameEntry.Data.PlayerRoleData.AddProp((int)PropEnum.Coin,addCount);
            GameEntry.Data.PlayerRoleData.DelPropAll((int)PropEnum.BattleCoin);
            GameEntry.Procedure.ChangeState(ProcedureState.Main);
        }

        public static void StartGame(bool newStart)
        {
            if (newStart)
            {
                GameEntry.Data.StageSaveData.SetStageIsPlaying(true);
                GameEntry.Data.StageSaveData.SetResetAbilities(true);
                GameEntry.Data.StageSaveData.SetTime(0f);
                GameEntry.Data.StageSaveData.SetXp(0f);
                GameEntry.Data.StageSaveData.SetXpLevel(0);
            }
            if (GameEntry.Data.StageSaveData.resetAbilities) GameEntry.Data.PlayerRoleData.DelPropAll((int)PropEnum.BattleCoin);
            AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            GameEntry.Procedure.ChangeState(ProcedureState.Battle);
        }
    }
}
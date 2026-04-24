using OctoberStudio.Currency;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameScripts;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OctoberStudio
{
    using OctoberStudio.Audio;
    using OctoberStudio.Easing;
    using OctoberStudio.Input;
    using Save;
    using Upgrades;
    using Vibration;

    public class GameController : MonoBehaviour
    {
        protected static readonly string MAIN_MENU_MUSIC_NAME = "Main Menu Music";

        private static GameController instance;

        [SerializeField] protected CurrenciesManager currenciesManager;
        public static CurrenciesManager CurrenciesManager => instance.currenciesManager;

        [SerializeField] protected UpgradesManager upgradesManager;
        public static UpgradesManager UpgradesManager => instance.upgradesManager;

        [SerializeField] protected ProjectSettings projectSettings;
        public static ProjectSettings ProjectSettings => instance.projectSettings;

        public static ISaveManager SaveManager { get; private set; }
        public static IAudioManager AudioManager { get; private set; }
        public static IVibrationManager VibrationManager { get; private set; }
        public static IInputManager InputManager { get; private set; }

        public static CurrencySave Gold { get; private set; }
        public static CurrencySave TempGold { get; private set; }

        public static AudioSource Music { get; private set; }

        private static StageSave stageSave;

        // Indicates that the main menu is just loaded, and not exited from the game scene
        public static bool FirstTimeLoaded { get; private set; }

        protected virtual void Awake()
        {
            if (instance != null)
            {
                Destroy(this);

                FirstTimeLoaded = false;

                return;
            }

            instance = this;

            FirstTimeLoaded = true;

            currenciesManager.Init();

            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = 120;
        }

        protected virtual void Start()
        {
            Gold = SaveManager.GetSave<CurrencySave>("gold");
            TempGold = SaveManager.GetSave<CurrencySave>("temp_gold");

            stageSave = SaveManager.GetSave<StageSave>("Stage");

            if (!stageSave.loadedBefore)
            {
                stageSave.loadedBefore = true;
            }
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

        public static void RegisterSaveManager(ISaveManager saveManager)
        {
            SaveManager = saveManager;
        }

        public static void RegisterVibrationManager(IVibrationManager vibrationManager)
        {
            VibrationManager = vibrationManager;
        }

        public static void RegisterAudioManager(IAudioManager audioManager)
        {
            AudioManager = audioManager;
        }

        public static void LoadStage()
        {
            if (stageSave.ResetStageData) TempGold.Withdraw(TempGold.Amount);
            if (instance != null) _ = instance.StageLoadingCoroutine();
            SaveManager.Save(false);
        }

        public static void LoadMainMenu()
        {
            Gold.Deposit(TempGold.Amount);
            TempGold.Withdraw(TempGold.Amount);

            if (instance != null) _ = instance.MainMenuLoadingCoroutine();

            SaveManager.Save(false);
        }

        protected async UniTask StageLoadingCoroutine()
        {
            await GameEntry.Scene.LoadSceneAsync(SceneGroupName.Game,1);
        }

        protected async UniTask MainMenuLoadingCoroutine()
        {
            await GameEntry.Scene.LoadSceneAsync(SceneGroupName.MainMenu,1);
            if (StageController.Stage.UseCustomMusic)
            {
                ChangeMusic(MAIN_MENU_MUSIC_NAME);
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
    }
}
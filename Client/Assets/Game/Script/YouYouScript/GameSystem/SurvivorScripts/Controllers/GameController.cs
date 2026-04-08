using System;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace Watermelon
{
    // [DefaultExecutionOrder(-10)]
    public class GameController : MonoBehaviour
    {
        private static GameController Instance;
        public static GameData Data;

        private void Awake()
        {
            Instance = this;
        }

        public async UniTask Init()
        {
            Data = await GameEntry.Loader.LoadMainAssetAsync<GameData>("Assets/Game/Download/ProjectFiles/Data/Game Data.asset", GameEntry.Instance.gameObject);
            Debugger.LogError("Data Init222====>");
            await Data.Init();
            Debugger.LogError("Data Init333====>");

            await DefaultMusicController.Instance.Initialise(transform);
            Debugger.LogError("DefaultMusicController Init====>");

            await CameraController.Instance.Initialise();
            Debugger.LogError("CameraController Init====>");

            await SkinController.Instance.Initialise();
            Debugger.LogError("SkinController Init====>");

            await UnlockableToolsController.Instance.Initialise();
            Debugger.LogError("UnlockableToolsController Init====>");

            await EnergyController.Instance.Initialise();
            Debugger.LogError("EnergyController Init====>");

            await EnvironmentController.Instance.Initialise();
            Debugger.LogError("EnvironmentController Init====>");

            await FishingController.Instance.Initialise();
            Debugger.LogError("FishingController Init====>");
            
            await DiggingController.Instance.Initialise();
            Debugger.LogError("DiggingController Init====>");
            
            await GlobalUpgradesController.Instance.Initialise();
            Debugger.LogError("GlobalUpgradesController Init====>");
            
            await FloatingTextController.Instance.Initialise();
            Debugger.LogError("FloatingTextController Init====>");

            await ParticlesController.Instance.Initialise();
            Debugger.LogError("ParticlesController Init====>");

            await NavigationHelper.Instance.Initialise();
            Debugger.LogError("NavigationHelper Init====>");

            await WorldController.Instance.Initialise();
            Debugger.LogError("WorldController Init====>");

            GameEntry.Event.Dispatch(Constants.EventName.EnergyChangedEvent);
            GameLoading.MarkAsReadyToHide();
            await UniTask.NextFrame();
            
            FloatingCloud.Instance.Init();
            
            CheckPlayFirstEntryAnim();
        }

        private void CheckPlayFirstEntryAnim()
        {
            SimpleBoolSave alreadyPlayedAnimationSave = SaveController.GetSaveObject<SimpleBoolSave>("FirstAnimation" + "8a95ba6b-040a-478e-8cb6-a75f8562e553");

            if (!alreadyPlayedAnimationSave.Value)
            {
                Control.DisableMovementControl();
                PlayerBehavior.GetBehavior().PlayerGraphics.RunWakeUpAnimation();

                alreadyPlayedAnimationSave.Value = true;
                SaveController.MarkAsSaveIsRequired();

                Tween.DelayedCall(3f, () =>
                {
                    Control.EnableMovementControl();
                });
            }
        }

        private void LateUpdate()
        {
            CameraController.Instance.LateUpdate();
        }

        private void OnDestroy()
        {
            Data.Unload(); 
            
            NavigationHelper.Instance.Unload();

            DistanceToggle.Unload();

            FishingController.Instance.Unload();

            DiggingController.Instance.Disable();
            DiggingController.Instance.Unload();

            NavMeshController.Reset();

            Tween.RemoveAll();
        }

        public static void LoadCurrentWorld()
        {
            WorldController.Instance.LoadCurrentWorld();
        }

        public static void OpenMainMenu()
        {            
            // Show fullscreen black overlay
            Overlay.Show(0.3f, () =>
            {
                // Save the current state of the game
                SaveController.Save(true);

                // Show main menu
                UIController.ShowPage<UIMainMenu>();

                // Unload the current world and all the dependencies
                GameController.UnloadWorld(() =>
                {
                    DefaultMusicController.Instance.ActivateMusic();

                    UIGamepadButton.DisableAllTags();
                    UIGamepadButton.EnableTag(UIGamepadButtonTag.MainMenu);

                    // Disable fullscreen black overlay
                    Overlay.Hide(0.3f);
                });
            }, true);
        }

        public static void OnWorldLoaded(WorldBehavior worldBehavior)
        {
            // UIController.ShowPage<FormGame>();

            FishingController.Instance.SpawnFishingPlaces();

            MissionsController.Instance.Initialise(worldBehavior.MissionsHolder?.Missions);

            DiggingController.Instance.Activate(worldBehavior.DiggingSpawnSettings);
        }

        public static void UnloadWorld(SimpleCallback onUnloaded)
        {
            Tween.RemoveAll();

            NavigationHelper.Instance.Unload();

            DistanceToggle.Unload();

            FishingController.Instance.Unload();

            DiggingController.Instance.Disable();
            DiggingController.Instance.Unload();

            WorldController.Instance.UnloadWorld(onUnloaded);
        }

        public static void LoadWorld(string worldID, SimpleCallback onWorldUnloaded = null, SimpleCallback onNewWorldLoaded = null)
        {
            // UIController.HidePage<FormGame>();

            // Show fullscreen black overlay
            Overlay.Show(0.3f, () =>
            {
                // Save the current state of the game
                SaveController.Save(true);

                // Unload the current world and all the dependencies
                GameController.UnloadWorld(() =>
                {
                    onWorldUnloaded?.Invoke();

                    // Load next world
                    WorldController.Instance.LoadWorld(worldID);

                    // Disable fullscreen black overlay
                    Overlay.Hide(0.3f, () =>
                    {
                        // UIController.ShowPage<FormGame>();

                        onNewWorldLoaded?.Invoke();
                    });
                });
            }, true);
        }

        #region Extensions
        public bool CacheComponent<T>(out T component) where T : Component
        {
            Component unboxedComponent = gameObject.GetComponent(typeof(T));

            if (unboxedComponent != null)
            {
                component = (T)unboxedComponent;

                return true;
            }

            Debug.LogError(string.Format("Scripts Holder doesn't have {0} script added to it", typeof(T)));

            component = null;

            return false;
        }
        #endregion
    }
}
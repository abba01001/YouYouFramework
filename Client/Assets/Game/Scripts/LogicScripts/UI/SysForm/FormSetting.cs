using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OctoberStudio;
using OctoberStudio.Abilities.UI;

using OctoberStudio.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameScripts
{
    public class FormSetting : UIFormBase
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider soundSlider;

        [SerializeField] Button backButton;
        [SerializeField] Button continueButton;
        [SerializeField] Button exitButton;
        [SerializeField] Button unlockButton;
        [SerializeField] ToggleComponent toggleComponent;

        [SerializeField] List<AbilitiesIndicatorsListBehavior> pauseAbilitiesLists;
        protected override async UniTask Awake()
        {
            await base.Awake();
            toggleComponent.TriggerOnEvent += () =>
            {
                GameController.VibrationManager.IsVibrationEnabled = true;
            };
            toggleComponent.TriggerOffEvent += () =>
            {
                GameController.VibrationManager.IsVibrationEnabled = false;
            };
            toggleComponent.SetState(GameController.VibrationManager.IsVibrationEnabled);
            
            musicSlider.onValueChanged.AddListener((float value) =>
            {
                GameController.AudioManager.MusicVolume = value;
            });
            soundSlider.onValueChanged.AddListener((float value) =>
            {
                GameController.AudioManager.SoundVolume = value;
            });
            backButton.onClick.AddListener(Close);
            exitButton.onClick.AddListener(OnExitButtonClicked);
            continueButton.SetButtonClick(Close);
            unlockButton.SetButtonClick(() => OpenAllStages());
        }
        
        public override void Close()
        {
            base.Close();
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            GameEntry.Event.Dispatch(Constants.EventName.CheckEnableJoystickEvent,true);
        }

        private static async UniTask OpenAllStages()
        {
            var database = await GameEntry.Loader.LoadMainAssetAsync<StagesDatabase>(
                "Assets/Game/Download/SurvivorAsset/Scriptables/Stages/Stages Database.asset");
            if (database != null)
            {
                if(database != null)
                {
                    GameEntry.Data.StageSaveData.SetMaxReachedStageId(database.StagesCount - 1);
                }
            }
        }

        private void RefreshAbilities()
        {
            for (int i = 0; i < pauseAbilitiesLists.Count; i++)
            {
                var abilityList = pauseAbilitiesLists[i];

                abilityList.Show();
                abilityList.Refresh();
            }
        }

        protected override void OnShow()
        {
            base.OnShow();
            Time.timeScale = 0f;
            GameEntry.Event.Dispatch(Constants.EventName.CheckEnableJoystickEvent,false);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Time.timeScale = 1f;
        }

        private void OnExitButtonClicked()
        {
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            Time.timeScale = 1f;
            GameEntry.Data.StageSaveData.SetStageIsPlaying(false);
            gameObject.SetActive(false);
            Close();
            StageController.ReturnToMainMenu();
        }
    }
}
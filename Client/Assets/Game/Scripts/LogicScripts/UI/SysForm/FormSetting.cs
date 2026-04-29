using Cysharp.Threading.Tasks;
using OctoberStudio;
using OctoberStudio.Easing;
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
        // [SerializeField] private Toggle vibrationToggle;


        [SerializeField] Button backButton;
        [SerializeField] Button exitButton;
        [SerializeField] ToggleComponent toggleComponent;

        protected override async UniTask Awake()
        {
            await base.Awake();
            InitToggles();
        }

        protected override void OnShow()
        {
            base.OnShow();
        }

        private void InitToggles()
        {
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
            backButton.onClick.AddListener(() => GameEntry.UI.CloseUIForm<FormSetting>());
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        public void Init(UnityAction onBackButtonClicked)
        {

        }

        private void OnExitButtonClicked()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
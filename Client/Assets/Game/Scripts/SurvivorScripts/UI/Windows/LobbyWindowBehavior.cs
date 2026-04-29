using System.Collections.Generic;
using GameScripts;
using OctoberStudio.Easing;
using OctoberStudio.Input;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using AudioManager = OctoberStudio.Audio.AudioManager;

namespace OctoberStudio.UI
{
    public class LobbyWindowBehavior : MonoBehaviour
    {
        [SerializeField] StagesDatabase stagesDatabase;

        [Space]
        [SerializeField] Button playButton;
        [SerializeField] Button upgradesButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button charactersButton;

        [Space]
        [SerializeField] Sprite playButtonEnabledSprite;
        [SerializeField] Sprite playButtonDisabledSprite;

        [Space]
        [SerializeField] Image continueBackgroundImage;
        [SerializeField] RectTransform contituePopupRect;
        [SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;
        [SerializeField] Button testBtn;
        [SerializeField] Button test1Btn;

        private void Awake()
        {
            InitBottomBtn();
            
            playButton.onClick.AddListener(OnPlayButtonClicked);
            confirmButton.onClick.AddListener(ConfirmButtonClicked);
            cancelButton.SetButtonClick(CancelButtonClicked);
            testBtn.SetButtonClick(async () =>
            {
                await GameEntry.Scene.LoadSceneAsync(SceneGroupName.Demo_Casual, 1);
                GameEntry.UI.CloseUIForm<FormMain>();
            });
            test1Btn.SetButtonClick(async () =>
            {
                await GameEntry.Scene.LoadSceneAsync(SceneGroupName.DemoScene, 1);
                GameEntry.UI.CloseUIForm<FormMain>();
            });
        }

        private void Start()
        {
            GameController.SaveManager.GoldData.onGoldAmountChanged += SetAmount;
            SetAmount(GameController.SaveManager.GoldData.Amount);
            
            if (GameController.SaveManager.StageData.IsPlaying && GameController.FirstTimeLoaded)
            {
                continueBackgroundImage.gameObject.SetActive(true);

                contituePopupRect.gameObject.SetActive(true);

                EventSystem.current.SetSelectedGameObject(confirmButton.gameObject);
            } else
            {
                EventSystem.current.SetSelectedGameObject(playButton.gameObject);
                GameController.SaveManager.StageData.SetSelectedStageId(GameController.SaveManager.StageData.MaxReachedStageId);
            }

            GameController.InputManager.onInputChanged += OnInputChanged;
            GameController.InputManager.InputAsset.UI.Settings.performed += OnSettingsInputClicked;
        }

        public void Init(UnityAction onUpgradesButtonClicked, UnityAction onSettingsButtonClicked, UnityAction onCharactersButtonClicked)
        {
            upgradesButton.onClick.AddListener(onUpgradesButtonClicked);
            settingsButton.onClick.AddListener(onSettingsButtonClicked);
            charactersButton.onClick.AddListener(onCharactersButtonClicked);
        }

        public void Open()
        {
            gameObject.SetActive(true);
            EasingManager.DoNextFrame(() => EventSystem.current.SetSelectedGameObject(playButton.gameObject));

            GameController.InputManager.onInputChanged += OnInputChanged;
            GameController.InputManager.InputAsset.UI.Settings.performed += OnSettingsInputClicked;
        }

        public void Close()
        {
            gameObject.SetActive(false);

            GameController.InputManager.onInputChanged -= OnInputChanged;
            GameController.InputManager.InputAsset.UI.Settings.performed -= OnSettingsInputClicked;
        }

        public void OnPlayButtonClicked()
        {
            GameEntry.UI.OpenUIForm<FormStage>();
        }

        private void OnDestroy()
        {
            GameController.SaveManager.GoldData.onGoldAmountChanged -= SetAmount;
            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        private void OnSettingsInputClicked(InputAction.CallbackContext context)
        {
            settingsButton.onClick?.Invoke();
        }

        private void ConfirmButtonClicked()
        {
            GameController.SaveManager.StageData.ResetStageData = false;

            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            GameController.LoadStage();
        }

        private void CancelButtonClicked()
        {
            // GameController.StageData.IsPlaying = false;
            continueBackgroundImage.DoAlpha(0, 0.3f).SetOnFinish(() => continueBackgroundImage.gameObject.SetActive(false));
            contituePopupRect.DoAnchorPosition(Vector2.down * 2500, 0.3f).SetEasing(EasingType.SineIn).SetOnFinish(() => contituePopupRect.gameObject.SetActive(false));
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }

        private void OnInputChanged(InputType prevInputType, InputType inputType)
        {
            if(prevInputType == InputType.UIJoystick)
            {
                if (continueBackgroundImage.gameObject.activeSelf)
                {
                    EventSystem.current.SetSelectedGameObject(confirmButton.gameObject);
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(playButton.gameObject);
                }
            }
        }


        [SerializeField] private List<Button> buttons = new List<Button>();
        [SerializeField] private Button lightBtn = null;
        private int disapearBtnIndex = -1;
        private void InitBottomBtn()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                int index = i;
                buttons[index].SetButtonClick(() => ChangeBtn(index));
            }

            ChangeBtn(2);
        }
        private void ChangeBtn(int index)
        {
            if (disapearBtnIndex != -1)
            {
                buttons[disapearBtnIndex].gameObject.MSetActive(true);
            }
            var selectBtn = buttons[index];
            var image = selectBtn.transform.Find("Icon").GetComponent<Image>();
            var lightIcon = lightBtn.transform.Find("Icon").GetComponent<Image>();
            lightIcon.sprite = image.sprite;
            lightIcon.SetNativeSize();
            lightBtn.transform.SetSiblingIndex(index);
            selectBtn.gameObject.MSetActive(false);
            disapearBtnIndex = index;
        }
        
        [SerializeField] private TextMeshProUGUI goldText;
        private void SetAmount(int amount)
        {
            goldText.text = amount.ToString();
        }
    }
}
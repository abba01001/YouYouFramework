using GameScripts;
using Main;
using OctoberStudio.Easing;
using OctoberStudio.Input;
using OctoberStudio.Save;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AudioManager = OctoberStudio.Audio.AudioManager;

namespace OctoberStudio.UI
{
    public class LobbyWindowBehavior : MonoBehaviour
    {
        [SerializeField] StagesDatabase stagesDatabase;

        [Space]
        [SerializeField] Image stageIcon;
        [SerializeField] Image lockImage;
        [SerializeField] TMP_Text stageLabel;
        [SerializeField] TMP_Text stageNumberLabel;

        [Space]
        [SerializeField] Button playButton;
        [SerializeField] Button upgradesButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button charactersButton;
        [SerializeField] Button leftButton;
        [SerializeField] Button rightButton;

        [Space]
        [SerializeField] Sprite playButtonEnabledSprite;
        [SerializeField] Sprite playButtonDisabledSprite;

        [Space]
        [SerializeField] Image continueBackgroundImage;
        [SerializeField] RectTransform contituePopupRect;
        [SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;
        [SerializeField] Button testBtn;

        private void Awake()
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
            leftButton.onClick.AddListener(DecrementSelectedStageId);
            rightButton.onClick.AddListener(IncremenSelectedStageId);

            confirmButton.onClick.AddListener(ConfirmButtonClicked);
            cancelButton.SetButtonClick(CancelButtonClicked);
            testBtn.SetButtonClick(async () =>
            {
                await GameEntry.Scene.LoadSceneAsync(SceneGroupName.Demo_Casual, 1);
            });
        }

        private void Start()
        {
            GameController.SaveManager.StageData.onSelectedStageChanged += InitStage;

            if (GameController.SaveManager.StageData.IsPlaying && GameController.FirstTimeLoaded)
            {
                continueBackgroundImage.gameObject.SetActive(true);

                contituePopupRect.gameObject.SetActive(true);

                EventSystem.current.SetSelectedGameObject(confirmButton.gameObject);

                InitStage(GameController.SaveManager.StageData.SelectedStageId);
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

        public void InitStage(int stageId)
        {
            var stage = stagesDatabase.GetStage(stageId);

            stageLabel.text = stage.DisplayName;
            stageNumberLabel.text = $"Stage {stageId + 1}";
            stageIcon.sprite = stage.Icon;

            if(GameController.SaveManager.StageData.SelectedStageId > GameController.SaveManager.StageData.MaxReachedStageId)
            {
                lockImage.gameObject.SetActive(true);
                playButton.interactable = false;
                playButton.image.sprite = playButtonDisabledSprite;
            } else
            {
                lockImage.gameObject.SetActive(false);
                playButton.interactable = true;
                playButton.image.sprite = playButtonEnabledSprite;
            }

            leftButton.gameObject.SetActive(!GameController.SaveManager.StageData.IsFirstStageSelected);
            rightButton.gameObject.SetActive(GameController.SaveManager.StageData.SelectedStageId != stagesDatabase.StagesCount - 1);
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
            GameController.SaveManager.StageData.IsPlaying = true;
            GameController.SaveManager.StageData.ResetStageData = true;
            GameController.SaveManager.StageData.Time = 0f;
            GameController.SaveManager.StageData.XP = 0f;
            GameController.SaveManager.StageData.XPLEVEL = 0;

            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            GameController.LoadStage();
        }

        private void IncremenSelectedStageId()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            GameController.SaveManager.StageData.SetSelectedStageId(GameController.SaveManager.StageData.SelectedStageId + 1);

            if (!rightButton.gameObject.activeSelf)
            {
                if (leftButton.gameObject.activeSelf)
                {
                    EventSystem.current.SetSelectedGameObject(leftButton.gameObject);
                } else
                {
                    EventSystem.current.SetSelectedGameObject(playButton.gameObject);
                }
            }
        }

        private void DecrementSelectedStageId()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            GameController.SaveManager.StageData.SetSelectedStageId(GameController.SaveManager.StageData.SelectedStageId - 1);

            if (!leftButton.gameObject.activeSelf)
            {
                if (rightButton.gameObject.activeSelf)
                {
                    EventSystem.current.SetSelectedGameObject(rightButton.gameObject);
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(playButton.gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            GameController.SaveManager.StageData.onSelectedStageChanged -= InitStage;
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
    }
}
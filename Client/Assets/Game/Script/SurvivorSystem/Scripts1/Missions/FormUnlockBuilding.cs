using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class FormUnlockBuilding : UIFormBase
    {
        UIFadeAnimation fadeAnimation;
        UIScaleAnimation panelBackScaleAnimation;

        [Space]
        [SerializeField] TMP_Text headerText;
        [SerializeField] Image previewIcon;
        [SerializeField] TMP_Text mainText;
        [SerializeField] TMP_Text buttonText;

        [SerializeField] Button continueButton;

        protected override async UniTask Awake()
        {
            await base.Awake();
            fadeAnimation = new UIFadeAnimation(fadeImage.gameObject);
            panelBackScaleAnimation = new UIScaleAnimation(panelRectTransform);

            continueButton.onClick.AddListener(() => ClosePanelButton());
        }

        public void Show(RewardInfo data)
        {
            fadeAnimation.Show();
            panelBackScaleAnimation.Show();

            headerText.text = data.HeaderText;
            previewIcon.sprite = data.PreviewIcon;
            mainText.text = data.MainText;
            buttonText.text = data.ButtonText;

            UIGamepadButton.DisableAllTags();
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Popup);
        }

        public async UniTask Hide()
        {
            if (!gameObject.activeSelf)
                return;

            fadeAnimation.Hide();
            panelBackScaleAnimation.Hide(onCompleted: () =>
            {
                gameObject.SetActive(false);
            });

            UIGamepadButton.DisableAllTags();
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Game);
            await UniTask.Delay(500);
        }

        private async UniTask ClosePanelButton()
        {
            await Hide();
            MissionsController.Instance.CompleteMission();
        }

        [System.Serializable]
        public class RewardInfo
        {
            [SerializeField] string headerText = "YOU UNLOCKED";
            public string HeaderText => headerText;

            [SerializeField] Sprite previewIcon;
            public Sprite PreviewIcon => previewIcon;

            [SerializeField] string mainText;
            public string MainText => mainText;

            [SerializeField] string buttonText = "AWESOME";
            public string ButtonText => buttonText;
        }
    }
}
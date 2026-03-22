using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.GlobalUpgrades;

namespace Watermelon
{
    public class FormUpgrade : UIFormBase
    {
        private readonly Vector2 DEFAULT_POSITION = new Vector2(0, 0);
        private readonly Vector2 HIDE_POSITION = new Vector2(0, -2000);

        [SerializeField] VerticalLayoutGroup verticalLayoutGroup;

        [Space]
        [SerializeField] Image fadeImage;
        [SerializeField] RectTransform panelRectTransform;
        [SerializeField] RectTransform viewportRectTransform;
        [SerializeField] RectTransform contentTransform;
        [SerializeField] Button closeButton;

        [Space]
        [SerializeField] Color defaultColor;
        [SerializeField] Color highlightedColor;
        public Color DefaultColor => defaultColor;
        public Color HighlightedColor => highlightedColor;

        [Header("Upgrade")]
        [SerializeField] GameObject upgradeUIPrefab;
        public GameObject UpgradeUIPrefab => upgradeUIPrefab;

        [Header("Scroll Rect")]
        [SerializeField] ScrollRect scrollRect;

        private int SelectedItemId { get; set; }
        private UpgradeUIPanel SelectedItem => UpgradeHelper.UpgradeUIPanels[SelectedItemId];
        public Transform ContentTransform => contentTransform;

        // Upgrades

        public UpgradePanelHelper _upgradeHelper;
        public UpgradePanelHelper UpgradeHelper => _upgradeHelper ??= new UpgradePanelHelper(this);

        private bool showAllAfterUpgrade;
        public bool ShowAllAfterUpgrade { get => showAllAfterUpgrade; set => showAllAfterUpgrade = value; }

        private float panelHeight;
        private float viewportHeight;

        private TweenCase scrollCase;

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        public void Init()
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            panelHeight = panelRectTransform.sizeDelta.y;
            viewportHeight = panelRectTransform.sizeDelta.y + viewportRectTransform.sizeDelta.y;
        }

        private void OnDestroy()
        {
            UpgradeHelper.Unload();

            GlobalUpgradesEventsHandler.OnUpgraded -= UpgradeHelper.OnUpgraded;
        }

        public void RegisterUpgrades(List<IUpgrade> upgrades)
        {
            UpgradeHelper.AddUpgrades(upgrades);
        }

        public void PlayShowAnimation()
        {
            GlobalUpgradesEventsHandler.OnUpgraded += UpgradeHelper.OnUpgraded;

            FormGame.Instance.Joystick.HideVisuals();

            fadeImage.color = fadeImage.color.SetAlpha(0.0f);
            fadeImage.DOFade(0.25f, 0.5f);

            // Reset panel position
            panelRectTransform.anchoredPosition = HIDE_POSITION;
            panelRectTransform.DOAnchoredPosition(DEFAULT_POSITION, 0.5f).SetEasing(Ease.Type.CircOut);

            UpgradeHelper.Show();

            UpgradeHelper.Redraw(true);

            scrollCase.KillActive();

            SelectedItemId = 0;

            UpgradeHelper.UpgradeUIPanels[SelectedItemId].OnSelect();

            contentTransform.anchoredPosition = new Vector2(0, 0);

            var contentSize = (UpgradeHelper.UpgradeUIPanels[0].Height + verticalLayoutGroup.spacing) * UpgradeHelper.UpgradeUIPanels.Count;

            if(contentSize < viewportHeight)
            {
                panelRectTransform.sizeDelta = panelRectTransform.sizeDelta.SetY(panelHeight - viewportHeight + contentSize);
            } 
            else
            {
                panelRectTransform.sizeDelta = panelRectTransform.sizeDelta.SetY(panelHeight);
            }

            contentTransform.sizeDelta = contentTransform.sizeDelta.SetY(contentSize);

            Control.DisableMovementControl();

            UIGamepadButton.DisableAllTags();
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Upgrades);

        }

        public  void PlayHideAnimation()
        {
            GlobalUpgradesEventsHandler.OnUpgraded -= UpgradeHelper.OnUpgraded;
            
            FormGame.Instance.Joystick.ShowVisuals();
            
            fadeImage.DOFade(0, 0.5f);
            panelRectTransform.DOAnchoredPosition(HIDE_POSITION, 0.5f).SetEasing(Ease.Type.CircIn).OnComplete(delegate
            {
                GameEntry.UI.CloseUIForm<FormUpgrade>();
            });

            for(int i = 0; i < UpgradeHelper.UpgradeUIPanels.Count; i++)
            {
                UpgradeHelper.UpgradeUIPanels[i].Disable();
            }

            Control.EnableMovementControl();
        }

        private void Update()
        {
            if (Control.InputType == InputType.Gamepad)
            {
                if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.B))
                {
                    closeButton.ClickButton();

                    UIGamepadButton.DisableTag(UIGamepadButtonTag.Upgrades);
                    UIGamepadButton.EnableTag(UIGamepadButtonTag.Game);
                }

                if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DDown))
                {
                    if (SelectedItemId < UpgradeHelper.UpgradeUIPanels.Count - 1)
                    {
                        UpgradeHelper.UpgradeUIPanels[SelectedItemId].OnDeselect();

                        SelectedItemId++;

                        UpgradeHelper.UpgradeUIPanels[SelectedItemId].OnSelect();

                        scrollCase.KillActive();

                        if (scrollRect.IsTargetLowerThanViewport(SelectedItem.Rect))
                        {
                            scrollCase = scrollRect.DoSnapTargetBottom(SelectedItem.Rect, 0.2f, -5).SetEasing(Ease.Type.SineOut);
                        }
                    }
                }
                else if (GamepadControl.WasButtonPressedThisFrame(GamepadButtonType.DUp))
                {
                    if (SelectedItemId > 0)
                    {
                        UpgradeHelper.UpgradeUIPanels[SelectedItemId].OnDeselect();

                        SelectedItemId--;

                        UpgradeHelper.UpgradeUIPanels[SelectedItemId].OnSelect();

                        scrollCase.KillActive();

                        if (scrollRect.IsTargetHigherThanViewport(SelectedItem.Rect))
                        {
                            scrollCase = scrollRect.DoSnapTargetTop(SelectedItem.Rect, 0.2f).SetEasing(Ease.Type.SineOut);
                        }
                    }
                }
            }
        }

        public void ResetUpgrades()
        {
            UpgradeHelper.Reset();
        }

        #region Buttons
        public void OnCloseButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            PlayHideAnimation();
        }
        #endregion
    }
}

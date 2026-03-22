using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.GlobalUpgrades;
using Watermelon.IAPStore;


namespace Watermelon
{
    public class FormGame : UIFormBase
    {
        public static FormGame Instance = null;
        [SerializeField] RectTransform safeAreaRectTransform;
        [SerializeField] Joystick joystick;
        public Joystick Joystick => joystick;

        [SerializeField] CurrencyUIController currenciesUIController;
        public CurrencyUIController CurrenciesUIController => currenciesUIController;

        [SerializeField] MissionUIPanel missionUI;
        public MissionUIPanel MissionUIPanel => missionUI;

        [SerializeField] EnergyUIPanel hungerUI;
        public EnergyUIPanel HungerUI => hungerUI;

        [SerializeField] UIWorldChangePopUp worldTransitionPopUp;
        public UIWorldChangePopUp WorldTransitionPopUp => worldTransitionPopUp;
        
        [Space]
        [SerializeField] Button upgradesButton;
        [SerializeField] Button iapStoreButton;
        [SerializeField] Button pauseButton;
        [SerializeField] TutorialCanvasController tutorialCanvasController;

        [Space]
        [SerializeField] GamepadIndicatorUI gamepadInteractor;

        [Header("Inventory")]
        [SerializeField] Button inventoryButton;
        [SerializeField] Image inventoryShine;
        [SerializeField] RectTransform inventoryRedDot;
        [SerializeField] TMP_Text inventoryCapacityText;
        [SerializeField] GameObject inventoryTutorial;

        private PlayerInventory playerInventory;
        private TweenCaseCollection caseCollection;
        private Coroutine shineCoroutine;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            GameUtil.LogError("初始化FormGame");
            Init();
            GameUtil.LogError("初始化FormGame完成");
        }

        public void Init()
        {
            joystick.Init(CurrCanvas);

            inventoryButton.onClick.AddListener(OnInventoryButtonClicked);
            upgradesButton.onClick.AddListener(OnUpgradesButonClicked);
            iapStoreButton.onClick.AddListener(OnIAPStoreButtonClicked);
            pauseButton.onClick.AddListener(OnPauseButtonClicked);

            currenciesUIController.Init(CurrencyController.Currencies);

            // if (EnergyController.IsEnergySystemEnabled)
            // {
            //     hungerUI.Initialise();
            // }
            // else
            // {
            //     hungerUI.gameObject.SetActive(false);
            // }

            // NotchSaveArea.RegisterRectTransform(safeAreaRectTransform);

            tutorialCanvasController.Initialise();
            worldTransitionPopUp.Initialise();

            GameEntry.Instance.BackgroundImage.color = Color.white;

            gamepadInteractor.Init();
        }

        #region Show/Hide

        public override async UniTask PlayShowAnimation()
        {
            base.PlayShowAnimation();
            playerInventory = PlayerBehavior.GetBehavior().Inventory;
            playerInventory.CapacityChanged += UpdateInventoryUI;

            UpdateInventoryUI();

            UIGamepadButton.DisableAllTags();
            UIGamepadButton.EnableTag(UIGamepadButtonTag.Game);
        }

        public override async UniTask PlayHideAnimation()
        {
            base.PlayHideAnimation();
            playerInventory.CapacityChanged -= UpdateInventoryUI;

            // UIController.OnPageClosed(this);
        }

        #endregion

        #region Player Inventory

        private void OnInventoryButtonClicked()
        {
            FormInventory form = GameEntry.UI.OpenUIForm<FormInventory>();
            form.ActivateTutorial(inventoryTutorial.activeSelf);

            inventoryTutorial.SetActive(false);

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            
            form.PlayShowAnimation();
        }

        private void UpdateInventoryUI()
        {
            if (!playerInventory.IsFull())
            {
                caseCollection.KillActive();
                caseCollection = Tween.BeginTweenCaseCollection();

                inventoryRedDot.DOScale(0, 0.2f).SetEasing(Ease.Type.SineIn);

                if (shineCoroutine != null)
                {
                    StopCoroutine(shineCoroutine);
                    shineCoroutine = null;

                    inventoryShine.DOFade(0, 0.2f);
                }

                inventoryCapacityText.text = CurrencyHelper.Format(playerInventory.CurrentCapacity) + "/" + CurrencyHelper.Format(playerInventory.MaxCapacity);

                Tween.EndTweenCaseCollection();
            }
            else
            {
                caseCollection.KillActive();
                caseCollection = Tween.BeginTweenCaseCollection();

                inventoryRedDot.DOScale(1, 0.2f).SetEasing(Ease.Type.SineOut);

                if (shineCoroutine == null)
                {
                    shineCoroutine = StartCoroutine(InventoryShineCoroutine());
                }

                inventoryCapacityText.text = "FULL!";

                Tween.EndTweenCaseCollection();
            }
        }

        private IEnumerator InventoryShineCoroutine()
        {
            float speed = 0.5f;

            while (true)
            {
                var alpha = inventoryShine.color.a;

                alpha += speed * Time.deltaTime;

                if (alpha > 1)
                {
                    alpha = 1;
                    speed *= -1;
                }
                else if (alpha <= 0)
                {
                    alpha = 0;
                    speed *= -1;
                }

                inventoryShine.SetAlpha(alpha);

                yield return null;
            }
        }

        public void SetInventoryTutorialState(bool state)
        {
            inventoryTutorial.SetActive(state);
        }
        #endregion

        public void SetBackgroundTexture(Texture texture)
        {
            GameEntry.Instance.BackgroundImage.texture = texture;
        }

        private void OnUpgradesButonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            GlobalUpgradesController.Instance.OpenMainUpgradesPage();

            StopUpgradesButtonHighlight();
        }

        private void OnIAPStoreButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

#if MODULE_MONETIZATION
            UIController.ShowPage<UIStore>();
#endif
        }

        private void OnPauseButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            UIController.ShowPage<UIPause>();
        }

        public void HighlightUpgradesButton()
        {
            TutorialCanvasController.ActivatePointerScreenPos((RectTransform)upgradesButton.transform, TutorialCanvasController.POINTER_DEFAULT);
        }

        public void StopUpgradesButtonHighlight()
        {
            TutorialCanvasController.ResetPointer();
        }
    }
}

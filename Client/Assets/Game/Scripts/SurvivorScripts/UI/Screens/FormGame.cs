using OctoberStudio.Abilities;
using OctoberStudio.Abilities.UI;
using OctoberStudio.Bossfight;

using OctoberStudio.UI;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameScripts;
using Main;
using OctoberStudio;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameScripts
{
    public class FormGame : UIFormBase
    {
        [SerializeField] BackgroundTintUI blackgroundTint;
        [SerializeField] JoystickBehavior joystick;

        [Header("Abilities")] [FormerlySerializedAs("abilitiesPanel")] [SerializeField]
        AbilitiesWindowBehavior abilitiesWindow;

        [SerializeField] ChestWindowBehavior chestWindow;
        [SerializeField] List<AbilitiesIndicatorsListBehavior> abilitiesLists;

        public AbilitiesWindowBehavior AbilitiesWindow => abilitiesWindow;
        public ChestWindowBehavior ChestWindow => chestWindow;

        [Header("Top UI")] [SerializeField] CanvasGroup topUI;

        [Header("Pause")] [SerializeField] Button pauseButton;

        [Header("Bossfight")] [SerializeField] CanvasGroup bossfightWarning;
        [SerializeField] BossfightHealthbarBehavior bossHealthbar;

        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private TextMeshProUGUI netDelayText;

        [SerializeField] private UITimer uiTimer;
        public WorldSpaceTextManager WorldSpaceTextManager;
        
        [SerializeField] private TextMeshProUGUI enemyKilledText;
        [SerializeField] private TextMeshProUGUI goldText;
        
        protected override async UniTask Awake()
        {
            await base.Awake();
            abilitiesWindow.Init();
            abilitiesWindow.onPanelClosed += OnAbilitiesPanelClosed;
            abilitiesWindow.onPanelStartedClosing += OnAbilitiesPanelStartedClosing;
            pauseButton.SetButtonClick(() =>
            {
                GameEntry.UI.OpenUIForm<FormSetting>();
            });
            chestWindow.OnClosed += OnChestWindowClosed;
        }

        public float refreshInterval = 0.1f;
        private float _totalTime; // 累加时间
        private int _frameCount; // 累加帧数

        private void RefreshFPS()
        {
            // 累加时间和帧数
            _totalTime += Time.deltaTime;
            _frameCount++;
            // 达到刷新间隔 → 计算平均FPS
            if (_totalTime >= refreshInterval)
            {
                netDelayText.text = $"MS:{NetManager.Instance.NetDelay}";
                // 平均FPS = 总帧数 ÷ 总时间
                float fps = _frameCount / _totalTime;
                // 显示（保留0位小数）
                fpsText.text = $"FPS: {Mathf.Round(fps)}";
                // 重置计数
                _totalTime = 0;
                _frameCount = 0;
            }
        }
        
        private void Update()
        {
            RefreshFPS();
        }

        protected override void OnShow()
        {
            base.OnShow();
            enemyKilledText.text = "0";
            goldText.text = "0";
            GameController.InputManager.InputAsset.UI.Settings.performed += OnSettingsInputClicked;
            GameEntry.Event.AddEventListener(Constants.EventName.RefreshKilledEnemiesCount,OnRefreshKilledEnemiesCount);
            GameEntry.Event.AddEventListener(Constants.EventName.PropsChangedEvent,HandleCoinAmountChanged);
            GameEntry.Event.AddEventListener(Constants.EventName.CheckEnableJoystickEvent,CheckEnableJoystick);
        }

        private void OnRefreshKilledEnemiesCount(object userdata)
        {
            var count = (int)userdata;
            enemyKilledText.text = count.ToString();
        }
        
        private void HandleCoinAmountChanged(object userdata)
        {
            PropChangeModel model = (PropChangeModel)userdata;
            Debugger.LogError($"{model.PropType}==={model.PropValue}");
            if (model.PropType == PropEnum.Coin)
            {
                goldText.text = model.PropValue.ToString();
            }
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            GameEntry.Event.RemoveEventListener(Constants.EventName.RefreshKilledEnemiesCount,OnRefreshKilledEnemiesCount);
            GameEntry.Event.RemoveEventListener(Constants.EventName.PropsChangedEvent,HandleCoinAmountChanged);
            GameEntry.Event.RemoveEventListener(Constants.EventName.CheckEnableJoystickEvent,CheckEnableJoystick);
        }
        
        private void OnSettingsInputClicked(InputAction.CallbackContext context)
        {
            pauseButton.onClick?.Invoke();
        }

        public void ShowBossfightWarning()
        {
            bossfightWarning.gameObject.SetActive(true);
            bossfightWarning.alpha = 0;
            bossfightWarning.DoAlpha(1f, 0.3f);
        }

        public void HideBossFightWarning()
        {
            bossfightWarning.DoAlpha(0f, 0.3f).SetOnFinish(() => bossfightWarning.gameObject.SetActive(false));
            topUI.DoAlpha(0, 0.3f);
        }

        public void ShowBossHealthBar(BossfightData data)
        {
            bossHealthbar.Init(data);
            bossHealthbar.Show();
        }

        public void HideBossHealthbar()
        {
            bossHealthbar.Hide();
            topUI.DoAlpha(1, 0.3f);
        }

        public void LinkBossToHealthbar(EnemyBehavior enemy)
        {
            bossHealthbar.SetBoss(enemy);
        }

        public void ShowAbilitiesPanel(List<AbilityData> abilities, bool isLevelUp)
        {
            abilitiesWindow.SetData(abilities);

            EasingManager.DoAfter(0.2f, () =>
            {
                for (int i = 0; i < abilitiesLists.Count; i++)
                {
                    var abilityList = abilitiesLists[i];

                    abilityList.Show();
                    abilityList.Refresh();
                }
            }, true);

            blackgroundTint.Show();

            abilitiesWindow.Show(isLevelUp);

            GameController.InputManager.InputAsset.UI.Settings.performed -= OnSettingsInputClicked;
        }

        private void OnAbilitiesPanelStartedClosing()
        {
            for (int i = 0; i < abilitiesLists.Count; i++)
            {
                var abilityList = abilitiesLists[i];

                abilityList.Hide();
            }

            blackgroundTint.Hide();
        }

        private void OnAbilitiesPanelClosed()
        {
            GameController.InputManager.InputAsset.UI.Settings.performed += OnSettingsInputClicked;
        }

        public void ShowChestWindow(int tierId, List<AbilityData> abilities, List<AbilityData> selectedAbilities)
        {
            chestWindow.OpenWindow(tierId, abilities, selectedAbilities);

            GameController.InputManager.InputAsset.UI.Settings.performed -= OnSettingsInputClicked;
        }

        private void OnChestWindowClosed()
        {
            GameController.InputManager.InputAsset.UI.Settings.performed += OnSettingsInputClicked;
        }

        public void CheckEnableJoystick(object userdata)
        {
            bool enable = (bool)userdata;
            if (enable)
            {
                if (GameController.InputManager.ActiveInput == OctoberStudio.Input.InputType.UIJoystick)
                {
                    joystick.Enable();
                }
            }
            else
            {
                joystick.Disable();
            }
        }
    }
}
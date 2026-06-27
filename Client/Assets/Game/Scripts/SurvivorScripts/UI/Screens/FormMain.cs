using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OctoberStudio;
using OctoberStudio.UI;
using OctoberStudio.Upgrades.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts
{
    public enum MainPanelType
    {
        lobbyWindow,
        upgradesWindow,
        charactersWindow,
        stageWindow,
        collectionWindow,
        equipmentWindow,
        shopWindow,
    }
    public class FormMain : UIFormBase
    {
        [SerializeField] LobbyPanel lobbyWindow;
        [SerializeField] UpgradesWindowBehavior upgradesWindow;
        [SerializeField] CharactersWindowBehavior charactersWindow;
        [SerializeField] StagePanel stageWindow;
        [SerializeField] CollectionPanel collectionWindow;
        [SerializeField] BagPanel equipmentWindow;
        [SerializeField] ShopPanel shopWindow;

        
        protected override async UniTask Awake()
        {
            await base.Awake();
            InitBottomBtn();
        }

        protected override void OnShow()
        {
            base.OnShow();
            GameEntry.Event.AddEventListener(Constants.EventName.FormMainChangePanelEvent,OnFormMainChangePanelEvent);
        }

        private void OnFormMainChangePanelEvent(object userdata)
        {
            ShowPanel((MainPanelType)userdata);
        }
        
        public void ShowPanel(MainPanelType panelType)
        {
            upgradesWindow.gameObject.MSetActive(false);
            charactersWindow.gameObject.MSetActive(false);
            lobbyWindow.gameObject.MSetActive(false);
            stageWindow.gameObject.MSetActive(false);
            collectionWindow.gameObject.MSetActive(false);
            equipmentWindow.gameObject.MSetActive(false);
            shopWindow.gameObject.MSetActive(false);
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            
            var lightText = focusBtn.transform.Get<TextMeshProUGUI>("Text");
            switch (panelType)
            {
                case MainPanelType.lobbyWindow:
                    lobbyWindow.gameObject.MSetActive(true);
                    lightText.text = "战斗";
                    break;
                case MainPanelType.upgradesWindow:
                    upgradesWindow.gameObject.MSetActive(true);
                    break;
                case MainPanelType.charactersWindow:
                    charactersWindow.gameObject.MSetActive(true);
                    lightText.text = "角色";
                    break;
                case MainPanelType.stageWindow:
                    stageWindow.gameObject.MSetActive(true);
                    lightText.text = "地图";
                    break;
                case MainPanelType.collectionWindow:
                    collectionWindow.gameObject.MSetActive(true);
                    lightText.text = "容器";
                    break;
                case MainPanelType.equipmentWindow:
                    equipmentWindow.gameObject.MSetActive(true);
                    lightText.text = "装备";
                    break;
                case MainPanelType.shopWindow:
                    shopWindow.gameObject.MSetActive(true);
                    lightText.text = "商城";
                    break;
                default:
                    break;
            }
        }
        

        private void ShowSettings()
        {
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            GameEntry.UI.OpenUIForm<FormSetting>();
        }

        private void OnDestroy()
        {
            charactersWindow.Clear();
            upgradesWindow.Clear();
            GameEntry.Event.RemoveEventListener(Constants.EventName.FormMainChangePanelEvent,OnFormMainChangePanelEvent);
        }
        
               
        

        [SerializeField] private List<Button> buttons = new List<Button>();
        [SerializeField] private Button focusBtn = null;
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
            var lightIcon = focusBtn.transform.Find("Icon").GetComponent<Image>();
            lightIcon.sprite = image.sprite;
            lightIcon.SetNativeSize();
            focusBtn.transform.SetSiblingIndex(index);
            focusBtn.gameObject.MSetActive(true);
            selectBtn.gameObject.MSetActive(false);
            disapearBtnIndex = index;

            if (index == 0)
            {
                ShowPanel(MainPanelType.shopWindow);
            }
            else if (index == 1)
            {
                ShowPanel(MainPanelType.collectionWindow);
            }
            if (index == 2)
            {
                ShowPanel(MainPanelType.lobbyWindow);
            }
            else if (index == 3)
            {
                ShowPanel(MainPanelType.equipmentWindow);
            }
            else if (index == 4)
            {
                ShowPanel(MainPanelType.stageWindow);
            }
            
            // AndroidManager.Instance.ScheduleNotification();

            // ChainManager.Instance.AddTask(new ResourceTaskChain(), priority: 1);
            // ChainManager.Instance.AddTask(new ResourceTaskChain222(), priority: 50);
            // ChainManager.Instance.CheckEnableChains();
            //
            // QueueManager.Instance.AddPopupTask(nameof(FormSetting), () => _ = GameEntry.UI.OpenUIForm<FormSetting>());
            // QueueManager.Instance.AddPopupTask(nameof(FormMailBox), () => _ = GameEntry.UI.OpenUIForm<FormMailBox>());
            // QueueManager.Instance.AddPopupTask(nameof(FormItemInfo), () => _ = GameEntry.UI.OpenUIForm<FormItemInfo>());
            
            // NotificationRequest myReq = new NotificationRequest
            // {
            //     Id = "daily_activity",          // 唯一业务ID，用于防重
            //     Title = "限时活动开启",         // 标题
            //     Content = "精彩活动正在进行中，快来领取奖励！", // 内容
            //     DelaySeconds = 10               // 10秒后触发测试
            // };
            // GameEntry.Notify.Schedule(myReq);

        }
    }
}

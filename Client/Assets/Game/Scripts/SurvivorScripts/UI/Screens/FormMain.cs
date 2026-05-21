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
        public static FormMain Instance { get; private set; }
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
            Instance = this;
        }

        protected override void OnShow()
        {
            base.OnShow();
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
        }
    }
}
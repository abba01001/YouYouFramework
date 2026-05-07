using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using OctoberStudio;
using OctoberStudio.UI;
using OctoberStudio.Upgrades.UI;
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
        collectionWindow
    }
    public class FormMain : UIFormBase
    {
        public static FormMain Instance { get; private set; }
        [SerializeField] LobbyWindowBehavior lobbyWindow;
        [SerializeField] UpgradesWindowBehavior upgradesWindow;
        [SerializeField] CharactersWindowBehavior charactersWindow;
        [SerializeField] StageWindowBehavior stageWindow;
        [SerializeField] CollectionWindowBehavior collectionWindow;

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
            
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            
            switch (panelType)
            {
                case MainPanelType.lobbyWindow:
                    lobbyWindow.gameObject.MSetActive(true);
                    break;
                case MainPanelType.upgradesWindow:
                    upgradesWindow.gameObject.MSetActive(true);
                    break;
                case MainPanelType.charactersWindow:
                    charactersWindow.gameObject.MSetActive(true);
                    break;
                case MainPanelType.stageWindow:
                    stageWindow.gameObject.MSetActive(true);
                    break;
                case MainPanelType.collectionWindow:
                    collectionWindow.gameObject.MSetActive(true);
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

            if (index == 2)
            {
                ShowPanel(MainPanelType.lobbyWindow);
            }
            else if (index == 4)
            {
                ShowPanel(MainPanelType.stageWindow);
            }
            else if (index == 3)
            {
                ShowPanel(MainPanelType.collectionWindow);
            }
        }
    }
}
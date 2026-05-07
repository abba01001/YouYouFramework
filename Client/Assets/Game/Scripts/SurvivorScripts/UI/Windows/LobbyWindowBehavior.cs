using System.Collections.Generic;
using GameScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class LobbyWindowBehavior : PanelBase
    {
        [SerializeField] StagesDatabase stagesDatabase;

        [SerializeField] Button playButton;
        [SerializeField] Button upgradesButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button charactersButton;
        [SerializeField] Sprite playButtonEnabledSprite;
        [SerializeField] Sprite playButtonDisabledSprite;
        [SerializeField] Button testBtn;
        [SerializeField] Button test1Btn;

        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private TextMeshProUGUI goldText;
        
        protected override void OnAwake()
        {
            playButton.SetButtonClick(() =>
            {
                GameEntry.UI.CloseUIForm<FormMain>();
                GameController.StartGame();
            });
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
            upgradesButton.SetButtonClick(() =>
            {
                FormMain.Instance.ShowPanel(MainPanelType.upgradesWindow);
            });
            charactersButton.SetButtonClick(() =>
            {
                FormMain.Instance.ShowPanel(MainPanelType.charactersWindow);
            });
            settingsButton.SetButtonClick(() =>
            {
                GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
                GameEntry.UI.OpenUIForm<FormSetting>();
            });
        }

        private bool checkLoadContinue = false;
        protected override void OnShow()
        {
            base.OnShow();
            GameEntry.Event.AddEventListener(Constants.EventName.PropsChangedEvent,HandleCoinAmountChanged);
            goldText.text = GameEntry.Data.GetProps((int)PropEnum.Coin).ToString();
            energyText.text = GameEntry.Data.GetProps((int)PropEnum.Energy).ToString();
            
            if (GameController.SaveManager.StageData.IsPlaying && !checkLoadContinue)
            {
                GameEntry.UI.OpenUIForm<FormContinue>();
            } 
            else
            {
                GameController.SaveManager.StageData.SetSelectedStageId(GameController.SaveManager.StageData.MaxReachedStageId);
            }
            checkLoadContinue = true;
        }
        
        private void HandleCoinAmountChanged(object userdata)
        {
            PropChangeModel model = (PropChangeModel)userdata;
            switch (model.PropType)
            {
                case PropEnum.Coin:
                    energyText.text = model.PropValue.ToString();
                    break;
                case PropEnum.Energy:
                    goldText.text = model.PropValue.ToString();
                    break;
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
            GameEntry.Event.RemoveEventListener(Constants.EventName.PropsChangedEvent,HandleCoinAmountChanged);
        }
    }
}
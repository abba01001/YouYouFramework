using System;
using System.Collections.Generic;
using DG.Tweening;
using GameScripts;
using Main;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class LobbyPanel : PanelBase
    {
        [SerializeField] StagesDatabase stagesDatabase;

        [SerializeField] Button playButton;
        [SerializeField] Button upgradesButton;
        [SerializeField] Button catalogueButton;
        [SerializeField] Button charactersButton;
        [SerializeField] Button testBtn;
        [SerializeField] Button test1Btn;
        [SerializeField] Button chatBtn;

        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private TextMeshProUGUI goldText;
        
        [SerializeField] private GameObject catalogue;
        [SerializeField] Button settingButton;
        [SerializeField] Button achievementButton;
        [SerializeField] Button dailyGiftButton;
        [SerializeField] Button inboxButton;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            
            Debugger.LogError(ActivityManager.Instance == null);
            RedDotNode rootNode = RedDotManager.Instance.GetModuleRootNode(RedDotId.TestRedDotId);
            RedDotManager.Instance.CreateRedDot(rootNode, playButton.transform, new Vector2(102, 48));

            playButton.SetButtonClick(() =>
            {
                // GameUtil.PlayMultiplyItemBounceAnim(energyText.transform);
                // return;
                GameController.StartGame(true);
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
                GameEntry.Event.Dispatch(Constants.EventName.FormMainChangePanelEvent,MainPanelType.upgradesWindow);
            });
            charactersButton.SetButtonClick(() =>
            {
                GameEntry.Event.Dispatch(Constants.EventName.FormMainChangePanelEvent,MainPanelType.charactersWindow);
            });
            catalogueButton.SetButtonClick(() =>
            {
                GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
                ShowCatalogue(true);
            });
            catalogue.GetComponent<Button>().SetButtonClick(() =>
            {
                ShowCatalogue(false);
            });
            settingButton.SetButtonClick(() =>
            {
                GameEntry.UI.OpenUIForm<FormSetting>();
                ShowCatalogue(false);
            });
            achievementButton.SetButtonClick(() =>
            {

            });
            dailyGiftButton.SetButtonClick(() =>
            {
                
            });
            inboxButton.SetButtonClick(() =>
            {
                GameEntry.UI.OpenUIForm<FormMailBox>();
                ShowCatalogue(false);
            });
            chatBtn.SetButtonClick(() =>
            {
                GameEntry.UI.OpenUIForm<FormChat>();
            });
        }

        private bool isShowingCatalogue = false;
        private void ShowCatalogue(bool bo)
        {
            if (isShowingCatalogue) return;
            catalogue.transform.DOKill();
            Sequence seq = DOTween.Sequence();
            isShowingCatalogue = true;
            var content = catalogue.transform.Find("Content");
            if (bo)
            {
                catalogue.gameObject.MSetActive(true);
                seq.Append(content.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutQuad));
                seq.Append(content.transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack));
                seq.OnComplete(() => isShowingCatalogue = false);
            }
            else
            {
                seq.Append(content.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack));
                seq.AppendCallback(() =>
                {
                    catalogue.gameObject.MSetActive(false);
                });
            }
            seq.OnComplete(() =>
            {
                isShowingCatalogue = false;
            });
        }

        private bool checkLoadContinue = false;
        protected override void OnShow()
        {
            base.OnShow();
            GameEntry.Event.AddEventListener(Constants.EventName.PropsChangedEvent,OnPropsChanged);
            goldText.text = GameEntry.Data.PlayerRoleData.GetProps((int)PropEnum.Coin).ToString();
            energyText.text = GameEntry.Data.PlayerRoleData.GetProps((int)PropEnum.Energy).ToString();
            
            // if (GameController.SaveManager.StageData.IsPlaying && !checkLoadContinue)
            // {
            //     GameEntry.UI.OpenUIForm<FormContinue>();
            // } 
            // else
            // {
            GameEntry.Data.StageSaveData.SetSelectedStageId(GameEntry.Data.StageSaveData.maxReachedStageId);
            // }
            checkLoadContinue = true;
        }
        
        private void OnPropsChanged(object userdata)
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
            GameEntry.Event.RemoveEventListener(Constants.EventName.PropsChangedEvent,OnPropsChanged);
        }
    }
}
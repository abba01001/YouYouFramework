using Cysharp.Threading.Tasks;
using OctoberStudio;
using OctoberStudio.UI;
using OctoberStudio.Upgrades.UI;
using UnityEngine;
using UnityEngine.UI;
using AudioManager = OctoberStudio.Audio.AudioManager;

namespace GameScripts
{
    public class FormContinue : UIFormBase
    {

        [SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;
        [SerializeField] Button closetButton;
        
        protected override async UniTask Awake()
        {
            await base.Awake();
            confirmButton.onClick.AddListener(ConfirmButtonClicked);
            cancelButton.SetButtonClick(CancelButtonClicked);
            closetButton.SetButtonClick(CancelButtonClicked);
        }
        
        private void ConfirmButtonClicked()
        {
            GameController.SaveManager.StageData.ResetStageData = false;
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            GameController.LoadStage();
        }

        private void CancelButtonClicked()
        {
            GameEntry.UI.CloseUIForm<FormContinue>();
        }

        protected override void OnShow()
        {
            base.OnShow();
        }


        private void OnDestroy()
        {

        }
    }
}
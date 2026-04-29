using Cysharp.Threading.Tasks;
using OctoberStudio;
using OctoberStudio.Easing;
using OctoberStudio.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameScripts
{
    public class FormResult : UIFormBase
    {
        [SerializeField] Button reviveButton;
        [SerializeField] Button exitButton;
        [SerializeField] Button continueButton;
        [SerializeField] private GameObject defeatContent;
        [SerializeField] private GameObject victoryContent;
        
        private static readonly int STAGE_COMPLETE_HASH = "Stage Complete".GetHashCode();
        protected override async UniTask Awake()
        {
            await base.Awake();
            continueButton.SetButtonClick(ContinueButtonClick);
            reviveButton.SetButtonClick(ReviveButtonClick);
            exitButton.SetButtonClick(ExitButtonClick);
        }

        protected override void OnShow()
        {
            base.OnShow();
        }

        public void ShowResult(bool win)
        {
            defeatContent.MSetActive(!win);
            victoryContent.MSetActive(win);
            if (win)
            {
                GameController.AudioManager.PlaySound(STAGE_COMPLETE_HASH);
            }
        }
        
        private void ReviveButtonClick()
        {
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            GameEntry.UI.CloseUIForm<FormResult>();
            StageController.ResurrectPlayer();
        }

        private void ExitButtonClick()
        {
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            Time.timeScale = 1;
            GameEntry.UI.CloseUIForm<FormResult>();
            StageController.ReturnToMainMenu();
        }
        
        private void ContinueButtonClick()
        {
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            Time.timeScale = 1;
            GameEntry.UI.CloseUIForm<FormResult>();
            GameController.LoadMainMenu();
        }
    }
}
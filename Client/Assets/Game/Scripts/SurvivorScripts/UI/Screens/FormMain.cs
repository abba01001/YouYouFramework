using Cysharp.Threading.Tasks;
using OctoberStudio;
using OctoberStudio.UI;
using OctoberStudio.Upgrades.UI;
using UnityEngine;

namespace GameScripts
{
    public class FormMain : UIFormBase
    {
        [SerializeField] LobbyWindowBehavior lobbyWindow;
        [SerializeField] UpgradesWindowBehavior upgradesWindow;
        [SerializeField] CharactersWindowBehavior charactersWindow;

        protected override async UniTask Awake()
        {
            await base.Awake();
            lobbyWindow.Init(ShowUpgrades, ShowSettings, ShowCharacters);
            upgradesWindow.Init(HideUpgrades);
            charactersWindow.Init(HideCharacters);
        }

        protected override void OnShow()
        {
            base.OnShow();
        }

        private void ShowUpgrades()
        {
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            upgradesWindow.Open();
        }

        private void HideUpgrades()
        {
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            upgradesWindow.Close();
        }

        private void ShowCharacters()
        {
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            charactersWindow.Open();
        }

        private void HideCharacters()
        {
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            charactersWindow.Close();
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
    }
}
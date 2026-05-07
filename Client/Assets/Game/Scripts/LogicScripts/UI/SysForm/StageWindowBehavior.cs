using Cysharp.Threading.Tasks;
using OctoberStudio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts
{
    public class StageWindowBehavior : PanelBase
    {
        [SerializeField] private Button preBtn;
        [SerializeField] private Button nextBtn;
        [SerializeField] private Button startBtn;
        [SerializeField] private TextMeshProUGUI titleText;
        protected override void OnAwake()
        {
            base.OnAwake();
            preBtn.SetButtonClick(() =>
            {
                DecrementSelectedStageId();
            });
            nextBtn.SetButtonClick(() =>
            {
                IncremenSelectedStageId();
            });
            startBtn.SetButtonClick(() =>
            {
                GameController.StartGame();
                FormMain.Instance.ShowPanel(MainPanelType.lobbyWindow);
                GameEntry.UI.CloseUIForm<FormMain>();
            });
        }

        private StagesDatabase _stagesDatabase = null;
        private void InitStage(int stageId)
        {
            if (_stagesDatabase == null)
            {
                _stagesDatabase = GameEntry.Loader.LoadMainAsset<StagesDatabase>("Assets/Game/Download/SurvivorAsset/Scriptables/Stages/Stages Database.asset");
            }
            var stage = _stagesDatabase.GetStage(stageId);
            titleText.text = $"{stageId}.{stage.DisplayName}";
            preBtn.gameObject.SetActive(GameController.SaveManager.StageData.SelectedStageId != 0);
            nextBtn.gameObject.SetActive(GameController.SaveManager.StageData.SelectedStageId != _stagesDatabase.StagesCount - 1);
        }

        protected override void OnShow()
        {
            base.OnShow();
            GameController.SaveManager.StageData.onSelectedStageChanged += InitStage;
            InitStage(GameController.SaveManager.StageData.SelectedStageId);
        }

        protected override void OnHide()
        {
            base.OnHide();
            GameController.SaveManager.StageData.onSelectedStageChanged -= InitStage;
        }

        private void IncremenSelectedStageId()
        {
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            GameController.SaveManager.StageData.SetSelectedStageId(GameController.SaveManager.StageData.SelectedStageId + 1);
        }

        private void DecrementSelectedStageId()
        {
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            GameController.SaveManager.StageData.SetSelectedStageId(GameController.SaveManager.StageData.SelectedStageId - 1);
        }

    }
}
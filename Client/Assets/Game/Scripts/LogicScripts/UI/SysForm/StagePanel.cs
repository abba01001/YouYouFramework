using Cysharp.Threading.Tasks;
using OctoberStudio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts
{
    public class StagePanel : PanelBase
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
        private void InitStage(object userdata = null)
        {
            if (_stagesDatabase == null)
            {
                _stagesDatabase = GameEntry.Loader.LoadMainAsset<StagesDatabase>("Assets/Game/Download/SurvivorAsset/Scriptables/Stages/Stages Database.asset");
            }

            var stageId = GameEntry.Data.StageSaveData.selectedStageId;
            var stage = _stagesDatabase.GetStage(stageId);
            titleText.text = $"{stageId}.{stage.DisplayName}";
            preBtn.gameObject.SetActive(GameEntry.Data.StageSaveData.selectedStageId != 0);
            nextBtn.gameObject.SetActive(GameEntry.Data.StageSaveData.selectedStageId!= _stagesDatabase.StagesCount - 1);
        }

        protected override void OnShow()
        {
            base.OnShow();
            GameEntry.Event.AddEventListener(Constants.EventName.SelectedStageEvent,InitStage);
            InitStage();
        }

        protected override void OnHide()
        {
            base.OnHide();
            GameEntry.Event.RemoveEventListener(Constants.EventName.SelectedStageEvent,InitStage);
        }

        private void IncremenSelectedStageId()
        {
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            GameEntry.Data.StageSaveData.SetSelectedStageId(GameEntry.Data.StageSaveData.selectedStageId + 1);
        }

        private void DecrementSelectedStageId()
        {
            GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
            GameEntry.Data.StageSaveData.SetSelectedStageId(GameEntry.Data.StageSaveData.selectedStageId - 1);
        }

    }
}
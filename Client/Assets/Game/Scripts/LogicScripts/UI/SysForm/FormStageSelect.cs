using Cysharp.Threading.Tasks;
using OctoberStudio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts
{
    public class FormStageSelect : UIFormBase
    {
        [SerializeField] private Button preBtn;
        [SerializeField] private Button nextBtn;
        [SerializeField] private Button startBtn;
        [SerializeField] private Button backBtn;
        [SerializeField] private TextMeshProUGUI titleText;
        protected override async UniTask Awake()
        {
            await base.Awake();
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
                GameController.SaveManager.StageData.IsPlaying = true;
                GameController.SaveManager.StageData.ResetStageData = true;
                GameController.SaveManager.StageData.Time = 0f;
                GameController.SaveManager.StageData.XP = 0f;
                GameController.SaveManager.StageData.XPLEVEL = 0;
                GameController.AudioManager.PlaySound(OctoberStudio.Audio.AudioManager.BUTTON_CLICK_HASH);
                GameController.LoadStage();
                GameEntry.UI.CloseUIForm<FormMain>();
                GameEntry.UI.CloseUIForm<FormStageSelect>();
            });
            backBtn.SetButtonClick(() =>
            {
                GameEntry.UI.CloseUIForm<FormStageSelect>();
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
            InitStage(GameController.SaveManager.StageData.SelectedStageId);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            GameController.SaveManager.StageData.onSelectedStageChanged += InitStage;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
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
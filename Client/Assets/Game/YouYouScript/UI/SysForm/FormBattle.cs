using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using YouYou;


/// <summary>
/// "战斗"界面
/// </summary>
public class FormBattle : UIFormBase
{
    [SerializeField] private Button exitBtn;
    [SerializeField] private Button testBtn;
    [SerializeField] private Button callBtn;
    [SerializeField] private Button settingBtn;
    [SerializeField] private RectTransform settingRect;
    [SerializeField] private Text timer;
    [SerializeField] private Text round;
    [SerializeField] private Text enermyCount;
    
    private int totalEnemyCount;
    private int curEnemyCount;
    private bool isShowSetting;
    protected override void Awake()
    {
        base.Awake();
        // exitBtn.SetButtonClick(()=>{
        //     GameEntry.Procedure.ChangeState(ProcedureState.Game);
        // });
        // testBtn.SetButtonClick(() =>
        // {
        //     GameEntry.UI.OpenUIForm<FormMap>();
        // });
        callBtn.SetButtonClick(() =>
        {
            BattleCtrl.Instance.GridManager.CallHero();
        });
        settingBtn.SetButtonClick(ShowSetting);
        settingRect.transform.localScale = new Vector3(1, 0, 1);
        BattleCtrl.Instance.GridManager.InitParams(transform.Find("GridLayout"));
    }

    private void ShowSetting()
    {
        if (!isShowSetting)
        {
            isShowSetting = true;
            settingRect.transform.DOScaleY(1f, 0.2f).SetEase(Ease.OutBack);
        }
        else
        {
            isShowSetting = false;
            settingRect.transform.DOScaleY(0f, 0.2f).SetEase(Ease.OutQuint);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEntry.Event.AddEventListener(Constants.EventName.UpdateBattleTimer,UpdateTimer);
        GameEntry.Event.AddEventListener(Constants.EventName.UpdateBattleRound,UpdateRound);
        GameEntry.Event.AddEventListener(Constants.EventName.UpdateEnemyCount,UpdateEnemy);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        GameEntry.Event.RemoveEventListener(Constants.EventName.UpdateBattleTimer,UpdateTimer);
        GameEntry.Event.RemoveEventListener(Constants.EventName.UpdateBattleRound,UpdateRound);
        GameEntry.Event.RemoveEventListener(Constants.EventName.UpdateEnemyCount,UpdateEnemy);
    }

    private void UpdateTimer(object userdata)
    {
        UpdateBattleTimerEvent t = userdata as UpdateBattleTimerEvent;
        string str = GameEntry.Time.ConvertSecondsToTimeFormat(t.Interval);
        timer.text = str;
    }

    private void UpdateEnemy(object userdata)
    {
        UpdateEnemyCountEvent t = userdata as UpdateEnemyCountEvent;
        curEnemyCount = Mathf.Max(curEnemyCount + t.Count, 0);
        enermyCount.text = $"{curEnemyCount}/{totalEnemyCount}";
    }
    
    private void UpdateRound(object userdata)
    {
        UpdateBattleRoundEvent t = userdata as UpdateBattleRoundEvent;
        round.text = $"波次:{t.Stage.stageIndex}/{t.StageCount}";
        totalEnemyCount += t.Stage.enemy.enemyCount;
    }
}

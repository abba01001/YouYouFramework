using System.Collections.Generic;
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
    [SerializeField] private Button stopBtn;
    [SerializeField] private Button callBtn;
    [SerializeField] private Button settingBtn;
    [SerializeField] private RectTransform settingRect;
    [SerializeField] private Text timer;
    [SerializeField] private Text round;
    [SerializeField] private Text enermyCount;
    [SerializeField] private Image enermyImage;
    [SerializeField] private GameObject gridPrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private Transform gridParent;

    private readonly List<GameObject> gridList = new List<GameObject>();
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
        stopBtn.SetButtonClick(() =>
        {
            GameEntry.Time.PauseTime(true);
            BattleCtrl.Instance.HideAllModel(true);
            GameEntry.UI.OpenUIForm<FormBattleStop>();
        });
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
        foreach (var go in gridList)
        {
            Destroy(go);
        }
        gridList.Clear();
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

    protected override void OnShow()
    {
        base.OnShow();
        GenerateGrid();
        curEnemyCount = 0;
        enermyImage.fillAmount = 0;
        enermyCount.text = "";
        round.text = "";
        timer.text = "00:00";
        settingBtn.SetButtonClick(ShowSetting);
        settingRect.transform.localScale = new Vector3(1, 0, 1);
    }
    
    private void GenerateGrid()
    {
        StartCoroutine(BatchObjectCreator.CreateObjectsInBatches(
            totalCount: 18,
            objectFactory: (i) => Instantiate(gridPrefab),
            initializeObject: (obj, i) =>
            {
                obj.transform.SetParent(gridParent);
                obj.SetActive(true);
                obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                obj.transform.localScale = Vector3.one;
                gridList.Add(obj);
                int tempIndex = i;
                //gridList.Add(obj);
            },
            batchSize: 9, // 每次创建20个对象
            onComplete: () =>
            {
                BattleCtrl.Instance.GridManager.InitParams(gridParent,linePrefab.GetComponent<RectTransform>());
                // 完成后执行的操作
            }
        ));
    }
    
    private void UpdateEnemy(object userdata)
    {
        UpdateEnemyCountEvent t = userdata as UpdateEnemyCountEvent;
        curEnemyCount = Mathf.Max(curEnemyCount + t.Count, 0);
        enermyCount.text = $"{curEnemyCount}/{BattleCtrl.Instance.MaxEnemyCount}";

        float value = (float) curEnemyCount / BattleCtrl.Instance.MaxEnemyCount;
        enermyImage.DOFillAmount(value, 0.5f);
    }
    
    private void UpdateRound(object userdata)
    {
        UpdateBattleRoundEvent t = userdata as UpdateBattleRoundEvent;
        round.text = $"波次:{t.Stage.stageIndex}/{t.StageCount}";
    }
    
    
}

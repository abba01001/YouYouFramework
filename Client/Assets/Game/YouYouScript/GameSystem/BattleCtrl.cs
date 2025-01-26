using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Main;
using UnityEngine;
using UnityEngine.Rendering;
using YouYou;

public class BattleCtrl : Singleton<BattleCtrl>
{
    public BattleGridManager GridManager { get; set; }
    public int MaxEnemyCount => 100; //最大敌人数
    private TimeAction TimeAction { get; set; }
    public  LevelData CurLevelData { get; set; }
    private List<Transform> Waypoints { get; set; } = new List<Transform>();
    private List<EnemyBase> EnemyList { get; set; } = new List<EnemyBase>();
    public bool IsInGaming = false;
    
    private int timer = 0;
    private string fullSavePath;
    private LevelStage stageData;
    private int curStageIndex;
    private bool canGenerateEnemy;

    private bool isGeneratingEnemies = false;  // 控制是否生成敌人
    private CancellationTokenSource cancellationTokenSource;  // 用来取消敌人生成的操作
    private TimeAction generateEnemyTimeAction;
    //注册事件
    public void Init()
    {
        IsInGaming = true;
        GameEntry.Event.AddEventListener(Constants.EventName.InitBattleData, InitBattleData);
        GridManager = new BattleGridManager();
    }

    //卸载事件
    public void End()
    {
        IsInGaming = false;
        generateEnemyTimeAction?.Stop();
        StopGeneratingEnemies();
        RecycleEnemy();
        GameEntry.Event.RemoveEventListener(Constants.EventName.InitBattleData, InitBattleData);
    }

    public void Update()
    {
        
    }
    
    public void InitBattleData(object userData)
    {
        timer = 0;
        curStageIndex = 0;
        canGenerateEnemy = false;
        Waypoints.Clear();
        EnemyList.Clear();    
        
        CurLevelData = userData as LevelData; //初始化关卡数据
        
        //初始化路径
        GeneratePath();

        //初始化敌人
        generateEnemyTimeAction =  GameEntry.Time.CreateTimerLoop(this, 1, -1, (int seconds) =>
        {
            timer += 1;
            CheckGenerateEnemy();
        });
        
    }
    
    private async void GeneratePath()
    {
        PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync(CurLevelData.path);
        for (int i = 0; i < obj.gameObject.transform.childCount; i++)
        {
            Waypoints.Add(obj.transform.GetChild(i));
        }
    }

    private void CheckGenerateEnemy()
    {
        if(CurLevelData == null) return;
        if(curStageIndex >= CurLevelData.stages.Count) return;
        stageData = CurLevelData.stages[curStageIndex];
        if (timer >= stageData.startTime)
        {
            canGenerateEnemy = true;
            curStageIndex++;
            GameEntry.Event.Dispatch(Constants.EventName.UpdateBattleRound,new UpdateBattleRoundEvent(stageData,CurLevelData.stages.Count));
        }
        GameEntry.Event.Dispatch(Constants.EventName.UpdateBattleTimer,new UpdateBattleTimerEvent(stageData.startTime - timer));
        if (canGenerateEnemy)
        {
            canGenerateEnemy = false;
            InstantiateEnemy(stageData);
        }
    }
    
    private async void InstantiateEnemy(LevelStage data)
    {
        int count = 0;
        if (isGeneratingEnemies)
            return;
        // 设置取消标志
        isGeneratingEnemies = true;
        cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            while (count < data.enemy.enemyCount)
            {
                // 如果生成被取消，则退出循环
                if (cancellationTokenSource.Token.IsCancellationRequested) break;
                PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync(GameUtil.GetModelPath(data.enemy.modelId));
                EnemyBase enemyBase = obj.GetComponent<EnemyBase>();
                enemyBase.WayPoints = Waypoints;
                enemyBase.StartRun();
                obj.GetComponent<SortingGroup>().sortingOrder -= count;
                count++;
                GameEntry.Event.Dispatch(Constants.EventName.UpdateEnemyCount, new UpdateEnemyCountEvent(1));
                EnemyList.Add(enemyBase);
                // 使用 cancellationToken 来处理延时
                await UniTask.Delay(TimeSpan.FromSeconds(data.enemy.interval), cancellationToken: cancellationTokenSource.Token);
            }
        }
        finally
        {
            // 生成结束后重置标志位
            isGeneratingEnemies = false;
        }
    }

    // 外部调用的中断敌人生成方法
    public void StopGeneratingEnemies()
    {
        // 取消当前的生成操作
        cancellationTokenSource?.Cancel();
    }

    
    private void RecycleEnemy()
    {
        foreach (var enemy in EnemyList)
        {
            GameEntry.Pool.GameObjectPool.Despawn(enemy.transform);
        }
    }
    
    public void HideAllModel(bool bo)
    {
        if (bo)
        {
            GameUtil.BlockSceneLayer(GameEntry.Instance.SceneCamera,18);
        }
        else
        {
            GameUtil.RestoreSceneLayer(GameEntry.Instance.SceneCamera,18);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Main;
using UnityEngine;
using UnityEngine.Rendering;
using YouYou;

//用来操作战斗数据
public class BattleCtrl : Singleton<BattleCtrl>
{
    public BattleGridManager GridManager { get; set; }
    public int MaxEnemyCount => 100; //最大敌人数
    private TimeAction TimeAction { get; set; }
    public  LevelData CurLevelData { get; set; }
    public List<Transform> Waypoints { get; set; } = new List<Transform>();
    private List<EnemyBase> EnemyList { get; set; } = new List<EnemyBase>();
    public bool IsInGaming = false;
    
    private int timer = 0;
    private LevelStage stageData;
    private int CurStageIndex { get; set; }//阶段索引
    private bool CanGenerateEnemyFlag { get; set; }//能否生成敌人标记
    private bool IsGeneratingEnemies { get; set; }//敌人是否正在生成
    private CancellationTokenSource cancellationTokenSource;  // 用来取消敌人生成的操作
    private TimeAction generateEnemyTimeAction;
    //注册事件
    public void Init()
    {
        IsInGaming = true;
        GameEntry.Event.AddEventListener(Constants.EventName.InitBattleData, InitBattleData);
        GridManager = new BattleGridManager();
    }

    public void StartEntryBattle()
    {
        // GameEntry.Data.TempSelectMapLvNum
        GameEntry.Procedure.ChangeState(ProcedureState.Battle);
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
        CurStageIndex = 0;
        CanGenerateEnemyFlag = false;
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
        if(CurStageIndex >= CurLevelData.stages.Count) return;
        stageData = CurLevelData.stages[CurStageIndex];
        if (timer >= stageData.startTime)
        {
            CanGenerateEnemyFlag = true;
            CurStageIndex++;
            GameEntry.Event.Dispatch(Constants.EventName.UpdateBattleRound,new UpdateBattleRoundEvent(stageData,CurLevelData.stages.Count));
        }
        GameEntry.Event.Dispatch(Constants.EventName.UpdateBattleTimer,new UpdateBattleTimerEvent(stageData.startTime - timer));
        if (CanGenerateEnemyFlag)
        {
            CanGenerateEnemyFlag = false;
            InstantiateEnemy(stageData);
        }
    }
    
    private async void InstantiateEnemy(LevelStage data)
    {
        int count = 0;
        if (IsGeneratingEnemies) return;
        // 设置取消标志
        IsGeneratingEnemies = true;
        cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            while (count < data.enemy.enemyCount)
            {
                // 如果生成被取消，则退出循环
                if (cancellationTokenSource.Token.IsCancellationRequested) break;
                PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync(GameUtil.GetModelPath(data.enemy.modelId));
                Sys_ModelEntity entity = GameEntry.DataTable.Sys_ModelDBModel.GetEntity(data.enemy.modelId);
                EnemyBase enemyBase = obj.GetComponent<EnemyBase>();
                await enemyBase.Init(data.enemy,entity,count,obj.GetComponent<SortingGroup>());
                enemyBase.StartRun();
                obj.GetComponent<SortingGroup>().sortingOrder += count;
                enemyBase.priority = 1000 - count;
                count++;
                GameEntry.Event.Dispatch(Constants.EventName.UpdateEnemyCount, new UpdateEnemyCountEvent(1));
                EnemyList.Add(enemyBase);
                await UniTask.Delay(TimeSpan.FromSeconds(data.enemy.interval), cancellationToken: cancellationTokenSource.Token);
            }
        }
        finally
        {
            // 生成结束后重置标志位
            IsGeneratingEnemies = false;
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

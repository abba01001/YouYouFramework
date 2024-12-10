using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Main;
using UnityEngine;
using YouYou;

public class BattleCtrl : Singleton<BattleCtrl>
{
    public BattleGridManager GridManager { get; set; }
    public int TotalRounds { get; set; }  // 总回合数
    private TimeAction TimeAction { get; set; }
    public  LevelData CurLevelData { get; set; }
    private List<Transform> Waypoints { get; set; }

    private int timer = 0;
    private string fullSavePath;
    private LevelStage stageData;
    private EnemyData enemyData;
    private int curStageIndex;
    private bool canGenerateEnemy;
    //注册事件
    public void Init()
    {
        GameEntry.Event.AddEventListener(Constants.EventName.InitBattleData, InitBattleData);
        GridManager = new BattleGridManager();
    }

    //卸载事件
    public void End()
    {
        GameEntry.Event.RemoveEventListener(Constants.EventName.InitBattleData, InitBattleData);
    }

    public void Update()
    {
        
    }
    
    public void InitBattleData(object userData)
    {
        //初始化关卡数据
        CurLevelData = userData as LevelData;
        
        //初始化路径
        GeneratePath();

        //初始化敌人
        GameEntry.Time.CreateTimerLoop(this, 1, -1, (int seconds) =>
        {
            timer += 1;
            CheckGenerateEnemy();
        });
        
    }
    
    private async void GeneratePath()
    {
        Waypoints ??= new List<Transform>();
        Waypoints.Clear();
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
        while (count < data.enemy.enemyCount)
        {
            PoolObj obj = await GameEntry.Pool.GameObjectPool.SpawnAsync(data.enemy.model);
            obj.GetComponent<EnemyBase>().InitPath(Waypoints);
            obj.GetComponent<EnemyBase>().StartRun();
            count++;
            GameEntry.Event.Dispatch(Constants.EventName.UpdateEnemyCount,new UpdateEnemyCountEvent(1));
            await UniTask.Delay(TimeSpan.FromSeconds(data.enemy.interval));
        }
    }



}

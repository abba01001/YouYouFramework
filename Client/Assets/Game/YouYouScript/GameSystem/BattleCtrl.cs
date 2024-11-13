using System;
using System.Collections;
using System.Collections.Generic;
using Main;
using YouYou;

public class BattleCtrl : Singleton<BattleCtrl>
{
    public BattleParticipants Participants { get; set; } // 参与者管理器
    public int TotalRounds { get; set; }  // 总回合数

    private float RoundTimeLimit = 15f;
    private TimeAction TimeAction { get; set; }

    //注册事件
    public void InitBattle()
    {
        BattleQueueManager.Instance.Start();
        GameEntry.Event.AddEventListener(Constants.EventName.InitBattleData, InitBattleData);
        GameEntry.Event.AddEventListener(Constants.EventName.StartNewRound, StartNewRound);
        GameEntry.Event.AddEventListener(Constants.EventName.StopRoundTimer, StopRoundTimer);
        GameEntry.Event.AddEventListener(Constants.EventName.RefreshGameTimer, RefreshGameTimer);
    }

    //卸载事件
    public void EndBattle()
    {
        GameEntry.Event.RemoveEventListener(Constants.EventName.InitBattleData, InitBattleData);
        GameEntry.Event.RemoveEventListener(Constants.EventName.StartNewRound, StartNewRound);
        GameEntry.Event.RemoveEventListener(Constants.EventName.StopRoundTimer, StopRoundTimer);
        GameEntry.Event.RemoveEventListener(Constants.EventName.RefreshGameTimer, RefreshGameTimer);
    }

    public void StopRoundTimer(object userData)
    {
        TimeAction.Stop();
        RoundTimeLimit = 15f;
    }
    
    public void ProcessTurn()
    {
        // 当前出牌角色进行操作
        var currentCharacter = Participants.CurrentPlayer;
        // 交给玩家或敌人选择卡牌并出牌
        if (currentCharacter is Player player)
        {
            Participants.PlayerChooseCard();  // 玩家选择卡牌
        }
        else if (currentCharacter is Enemy enemy)
        {
            Participants.EnemyChooseCard();
        }

        // 结束回合，恢复精力，更新回合数
        Participants.EndTurn();
        TotalRounds++;
    }

    public void StartNewRound(object userData)
    {
        TimeAction = GameEntry.Time.CreateTimerLoop(this, 1f, -1, (int loop) =>
        {
            RoundTimeLimit--;
            if (RoundTimeLimit == 0)
            {
                TimeAction.Stop();
                RoundTimeLimit = 15f;
                ProcessTurn();
            }
        });
        ProcessTurn();
    }
    
    
    
    public void InitBattleData(object userData)
    {
        //先初始化玩家。然后玩家AI
        //初始化敌方
        Participants = new BattleParticipants(GetPlayers(), GetEnemies());
        Participants.DetermineTurnOrder();
        TotalRounds = 1;
    }

    private List<Player> GetPlayers()
    {
        return new List<Player>();
    }

    private List<Enemy> GetEnemies()
    {
        return new List<Enemy>();
    }

    
    public void RefreshGameTimer(object userData)
    {
    }

    public void Update()
    {
        BattleQueueManager.Instance.Update();
    }
}

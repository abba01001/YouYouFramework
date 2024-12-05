using System;
using System.Collections;
using System.Collections.Generic;
using Main;
using YouYou;

public class BattleCtrl : Singleton<BattleCtrl>
{
    public int TotalRounds { get; set; }  // 总回合数
    private float RoundTimeLimit = 15f;
    private TimeAction TimeAction { get; set; }

    //注册事件
    public void Init()
    {
        GameEntry.Event.AddEventListener(Constants.EventName.InitBattleData, InitBattleData);
        GameEntry.Event.AddEventListener(Constants.EventName.StopRoundTimer, StopRoundTimer);
    }

    //卸载事件
    public void End()
    {
        GameEntry.Event.RemoveEventListener(Constants.EventName.InitBattleData, InitBattleData);
        GameEntry.Event.RemoveEventListener(Constants.EventName.StopRoundTimer, StopRoundTimer);
    }

    public void StopRoundTimer(object userData)
    {
        TimeAction.Stop();
        RoundTimeLimit = 15f;
    }

    public void Update()
    {
        
    }
    
    public void InitBattleData(object userData)
    {
        //先初始化玩家。然后玩家AI
        //初始化敌方
        //初始化路径
        
    }


}

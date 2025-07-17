using UnityEngine;
using YouYou;

public class GameEvent { }

public class PopupActionEvent : GameEvent
{
    public string Name;
    public UIActionType UIActionType;
    public PopupActionEvent(string name,UIActionType actionType)
    {
        Name = name;
        UIActionType = actionType;
    }
}

public class ViewActionEvent : GameEvent
{
    public string Name;
    public UIActionType UIActionType;
    public ViewActionEvent(string name,UIActionType actionType)
    {
        Name = name;
        UIActionType = actionType;
    }
}

public class EventMessage : GameEvent
{
    public string EventName;

    public EventMessage(string eventName)
    {
        EventName = eventName;
    }
}

public class SpawnCustomerEvent : GameEvent
{
    public Transform bornPoint;
    public Transform exitPoint;

    public SpawnCustomerEvent(Transform bp, Transform ep)
    {
        bornPoint = bp;
        exitPoint = ep;
    }
}

public class UpdateBuildingSpendEvent : GameEvent
{
    public int BuildingId;
    public int AddCount;
    public bool Unlock;
    public void Init(int id,int count,bool unlock)
    {
        BuildingId = id;
        AddCount = count;
        Unlock = unlock;
    }
}

public class UpdateBattleTimerEvent : GameEvent
{
    public int Interval;
    public UpdateBattleTimerEvent(int interval)
    {
        Interval = interval;
    }
}

public class UpdateBattleRoundEvent : GameEvent
{
    public LevelStage Stage;
    public int StageCount;
    public UpdateBattleRoundEvent(LevelStage stage,int stageCount)
    {
        Stage = stage;
        StageCount = stageCount;
    }
}

public class UpdateEnemyCountEvent : GameEvent
{
    public int Count;
    public UpdateEnemyCountEvent(int count)
    {
        Count = count;
    }
}
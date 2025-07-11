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
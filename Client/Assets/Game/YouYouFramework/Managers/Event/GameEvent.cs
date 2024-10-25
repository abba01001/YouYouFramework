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
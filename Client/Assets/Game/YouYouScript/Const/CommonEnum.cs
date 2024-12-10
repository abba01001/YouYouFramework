
public enum PrefabName : uint
{
    Player,
    Archer,
    PlayerCtrl,
    PlayerCamera,
    Monster,
    UIGlobal_HUD,
    UIGlobal_HeaderBar,
    Buff_xuanyun,
    DungeonGenerator,
}

public enum AtlasName : uint
{
    Common,
    FormBattle
}

public enum BGMName : uint
{
    None,
    maintheme1,
    button_sound,
}
public enum AudioName : uint
{
    button_sound,
}
public enum SceneGroupName : uint
{
    None,
    Main,
    Battle,
    MapEditor
}

public enum EventName : uint
{
    TestEvent,
    GameEntryOnUpdate,
    GameEntryOnApplicationQuit,
    GameEntryOnApplicationPause,
    LoadingSceneUpdate,
    PopupAction,
    ViewAction,
    EventMessage,
    UpdatePlayerPos,
    FinishLoadDataTable,
}

public enum UIActionType
{
    ShowUI,
    HideUI,
}

public enum InputName
{
    Horizontal,
    Vertical,
    MouseX,
    MouseY,
    MouseScrollWheel,
    BuyTower,
}

public enum WebSocketMethod : uint
{
    TestWebSocket,
    DueTimeCallBack,
}

public enum BGType : uint
{
    Main,
    Battle
}
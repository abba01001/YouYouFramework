using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Collections;
using Cysharp.Threading.Tasks;
using Main;
using MessagePack;
using MessagePack.Unity;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Watermelon;
using YouYouFramework;
using PoolManager = YouYouFramework.PoolManager;

public class GameEntry : MonoBehaviour
{
    
    /// <summary>
    /// Http调用失败后重试次数
    /// </summary>
    public static int HttpRetry { get; private set; }
    /// <summary>
    /// Http调用失败后重试间隔（秒）
    /// </summary>
    public static int HttpRetryInterval { get; private set; }
    
    //全局参数设置
    [FoldoutGroup("ParamsSettings")]
    [SerializeField]
    private ParamsSettings m_ParamsSettings;
    public static ParamsSettings ParamsSettings { get; private set; }

    [FoldoutGroup("MacroSettings")]
    [SerializeField]
    public MacroSettings m_MacroSettings;
    public static MacroSettings MacroSettings { get; private set; }
        
    //当前设备等级
    [FoldoutGroup("ParamsSettings")]
    [SerializeField]
    private ParamsSettings.DeviceGrade m_CurrDeviceGrade;
    public static ParamsSettings.DeviceGrade CurrDeviceGrade { get; private set; }
    
    
    [FoldoutGroup("ResourceGroup")] [Header("游戏物体对象池分组")]
    public SpawnPoolEntity[] GameObjectPoolGroups;

    [FoldoutGroup("ResourceGroup")] [Header("对象池锁定的资源包")]
    public string[] LockedAssetBundle;

    [FoldoutGroup("UIGroup")] [Header("UI摄像机")]
    public Camera UICamera;

    [Header("主摄像机")] public Camera MainCamera;

    [FoldoutGroup("UIGroup")] [Header("根画布的缩放")]
    public CanvasScaler UIRootCanvasScaler;

    public RectTransform UIRootRectTransform { get; private set; }

    [FoldoutGroup("UIGroup")] [Header("UI分组")]
    public UIGroup[] UIGroups;

    [FoldoutGroup("UIGroup")] [Header("主页背景")] [FoldoutGroup("AudioGroup")] [Header("声音主混合器")]
    public AudioMixer MonsterMixer;

    public RawImage BackgroundImage;

    [Header("当前语言（要和本地化表的语言字段 一致）")] [SerializeField]
    private YouYouLanguage m_CurrLanguage;

    public static YouYouLanguage CurrLanguage;

    [Header("声音主混合器")]
    public AudioMixer MasterMixer;

    //管理器属性
    public static LoggerManager Logger { get; private set; }
    public static EventManager Event { get; private set; }
    public static DataManager Data { get; private set; }
    public static FsmManager Fsm { get; private set; }
    public static ProcedureManager Procedure { get; private set; }
    public static DataTableManager DataTable { get; private set; }
    public static HttpManager Http { get; private set; }
    public static LocalizationManager Localization { get; private set; }
    public static PoolManager Pool { get; private set; }
    public static YouYouSceneManager Scene { get; private set; }
    public static LoaderManager Loader { get; private set; }
    public static UIManager UI { get; private set; }
    public static NetManager Net { get; private set; }
    public static AudioManager Audio { get; private set; }
    public static TaskManager Task { get; private set; }
    public static QualityManager Quality { get; private set; }
    public static SDKManager SDK { get; private set; }
    public static DialogueManager Dialogue { get; private set; }
    public static GuideManager Guide { get; private set; }
    public Camera SceneCamera { get; set; }
    public static ClassObjectPool ClassObjectPool { get; private set; }

    /// <summary>
    /// 单例
    /// </summary>
    public static GameEntry Instance { get; private set; }

    private void Awake()
    {
        Log(LogCategory.Procedure, "GameEntry.OnAwake()");
        Instance = this;
        ClassObjectPool = new ClassObjectPool();
        UIRootRectTransform = UIRootCanvasScaler.GetComponent<RectTransform>();
        // if (MainEntry.Reporter != null) MainEntry.Reporter.ShowLogPanel(false);
        CurrLanguage = m_CurrLanguage;
        Application.targetFrameRate = 120;
        
        
        
        
        if (MacroSettings == null)
        {
            MacroSettings = m_MacroSettings;
        }
        
        //此处以后判断如果不是编辑器模式 要根据设备信息判断等级
        CurrDeviceGrade = m_CurrDeviceGrade;
        ParamsSettings = m_ParamsSettings;
        MacroSettings = m_MacroSettings;

        
        
        //初始化系统参数
        HttpRetry = ParamsSettings.GetGradeParamData(GameConst.Http_Retry, CurrDeviceGrade);
        HttpRetryInterval = ParamsSettings.GetGradeParamData(GameConst.Http_RetryInterval, CurrDeviceGrade);
    }

    private void Start()
    {
        Log(LogCategory.Procedure, "GameEntry.OnStart()");
        Logger = new LoggerManager();
        Event = new EventManager();
        Data = new DataManager();
        Fsm = new FsmManager();
        Procedure = new ProcedureManager();
        DataTable = new DataTableManager();
        Http = new HttpManager();
        Localization = new LocalizationManager();
        Pool = new PoolManager();
        Scene = new YouYouSceneManager();
        Loader = new LoaderManager();
        UI = new UIManager();
        Net = new NetManager();
        Audio = new AudioManager();
        Task = new TaskManager();
        Quality = new QualityManager();
        SDK = new SDKManager();
        Dialogue = new DialogueManager();
        Guide = new GuideManager();
        Logger.Init();
        Procedure.Init();
        DataTable.Init();
        Http.Init();
        Localization.Init();
        // Pool.Init();
        UI.Init();
        Net.Init();
        Audio.Init();
        SDK.Init();
        Dialogue.Init();
        Task.Init();
        Guide.Init();
        //进入第一个流程
        Procedure.ChangeState(ProcedureState.Launch);
        GameEntry.UI.OpenUIForm<FormMask>();
        Dictionary<(Key, Key?), Action> keyMappings = new Dictionary<(Key, Key?), Action>
        {
            { (Key.Numpad0, Key.LeftCtrl), Test0 },
            { (Key.Numpad1, Key.LeftCtrl), Test1 },
            { (Key.Numpad2, Key.LeftCtrl), Test2 },
            { (Key.Numpad3, Key.LeftCtrl), Test3 },
            { (Key.Numpad4, Key.LeftCtrl), Test4 }
        };
        StopCoroutine(GameUtil.CheckKeys(keyMappings));
        StartCoroutine(GameUtil.CheckKeys(keyMappings));
        ViewQueueManager.Instance.RegisterEvents();
        Initialize();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private void Initialize()
    {
        MessagePackSerializer.DefaultOptions =
            MessagePackSerializerOptions.Standard.WithResolver(UnityResolver.InstanceWithStandardResolver);
    }

    private bool isOpen = false;

    private void Test0()
    {
        return;
        // isOpen = !isOpen;
        // MainEntry.Reporter.ShowLogPanel(isOpen);
        StartCoroutine(GameUtil.LocationInfoCoroutine(null));
    }

    private void Test1()
    {
        return;
        GameEntry.Data.PlayerRoleData.roleAttr["role_level"]++;
        GameEntry.Event.Dispatch(Constants.EventName.UpdateBtnUnlockStatus);
        //QueueManager.Instance.AddEventTask("Hello","CloseHello");
    }

    private void Test2()
    {

        // FormMask.CloseCircleMask();
        // QueueManager.Instance.AddTimeTask(1f, () =>
        // {
        //     Debugger.LogError("你好");
        // }, () =>
        // {
        //     Debugger.LogError("结束，跳转下一个队列");
        // });
    }

    private void Test3()
    {
        return;
        GameEntry.Event.Dispatch(Constants.EventName.EventMessage, new EventMessage("CloseHello"));
    }

    private void Test4()
    {
        return;
        // Debugger.LogError(typeof(FormMain).FullName);
        // GameEntry.UI.OpenUIFormByName("FormChangeName");
        GameEntry.Data.InitGameData(null);
        GameEntry.Data.SaveData(true);
        return;
        GameEntry.Data.InitGameData(null);
        GameEntry.Data.SaveData(true);
        GameUtil.ShowTip("测试文本哈哈哈哈哈哈！！！");
    }

    void Update()
    {
        Data.OnUpdate();
        Procedure.OnUpdate();
        Pool.OnUpdate();
        Scene.OnUpdate();
        UI.OnUpdate();
        Net.OnUpdate();
        Audio.OnUpdate();
        SDK.OnUpdate();
        Dialogue.OnUpdate();
        Task.OnUpdate();
        Guide.OnUpdate();
        GameEntry.Event.Dispatch(Constants.EventName.GameEntryOnUpdate);
    }

    private void LateUpdate()
    {
    }

    private void OnApplicationQuit()
    {
        Data.SaveData(true);
        Net.DisConnectServer();
        Logger.SyncLog();
        Logger.Dispose();
        Fsm.Dispose();
        SDK.UploadLogData(Data.UserId);
        GameEntry.Event.Dispatch(Constants.EventName.GameEntryOnApplicationQuit);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Data.SaveData(true);
            GameEntry.Event.Dispatch(Constants.EventName.GameEntryOnApplicationPause);
        }
    }

    public Transform GetSayItemParent()
    {
        return UIGroups[4].Group;
    }

    public static void Log(LogCategory catetory, object message, params object[] args)
    {
#if DEBUG_LOG_NORMAL
            string value = string.Empty;
            if (args.Length == 0)
            {
                value = message.ToString();
            }
            else
            {
                value = string.Format(message.ToString(), args);
            }
            Debug.Log(string.Format("youyouLog=={0}=={1}", catetory.ToString(), value));
#endif
    }

    public static void LogWarning(LogCategory catetory, object message, params object[] args)
    {
#if DEBUG_LOG_WARNING
            string value = string.Empty;
            if (args.Length == 0)
            {
                value = message.ToString();
            }
            else
            {
                value = string.Format(message.ToString(), args);
            }
            Debug.LogWarning(string.Format("youyouLog=={0}=={1}", catetory.ToString(), value));
#endif
    }

    public static void LogError(LogCategory catetory, object message, params object[] args)
    {
#if DEBUG_LOG_ERROR
            string value = string.Empty;
            if (args.Length == 0)
            {
                value = message.ToString();
            }
            else
            {
                value = string.Format(message.ToString(), args);
            }
            Debug.LogError(string.Format("youyouLog=={0}=={1}", catetory.ToString(), value));
#endif
    }

    public static void LogError(params object[] messages)
    {
        string combinedMessage = StringUtil.JointString(messages);
        Debug.LogError(combinedMessage);
    }

    private void OnDestroy()
    {
        Net.OnDestroy();
        Debugger.LogError("销毁",this.gameObject.name);
    }
}
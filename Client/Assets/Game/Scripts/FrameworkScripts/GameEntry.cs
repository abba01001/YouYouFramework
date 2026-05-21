using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameScripts;
using Main;
using MessagePack;
using MessagePack.Unity;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GameScripts
{
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
        [FoldoutGroup("ParamsSettings")] [SerializeField]
        private ParamsSettings m_ParamsSettings;
    
        public static ParamsSettings ParamsSettings { get; private set; }
    
        [FoldoutGroup("MacroSettings")] [SerializeField]
        public MacroSettings m_MacroSettings;
    
        public static MacroSettings MacroSettings { get; private set; }
    
        //当前设备等级
        [FoldoutGroup("ParamsSettings")] [SerializeField]
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
    
        [Header("当前语言（要和本地化表的语言字段 一致）")] [SerializeField]
        private FrameworkLanguage m_CurrLanguage;
    
        public static FrameworkLanguage CurrLanguage;
    
        [Header("声音主混合器")] public AudioMixer MasterMixer;
        [Header("FPS")] public GameObject FpsGraphy;
    
        //管理器属性
        public static EventManager Event { get; private set; }
        public static DataManager Data { get; private set; }
        public static FsmManager Fsm { get; private set; }
        public static ProcedureManager Procedure { get; private set; }
        public static DataTableManager DataTable { get; private set; }
        public static LocalizationManager Localization { get; private set; }
        public static PoolManager Pool { get; private set; }
        public static SceneManager Scene { get; private set; }
        public static LoaderManager Loader { get; private set; }
        public static UIManager UI { get; private set; }
        public static TaskManager Task { get; private set; }
        public static ClassObjectPool ClassObjectPool { get; private set; }
    
        /// <summary>
        /// 单例
        /// </summary>
        public static GameEntry Instance { get; private set; }
        private void Awake()
        {
            Debugger.Log("GameEntry.OnAwake()");
            Instance = this;
            TimeUtils.TimeZone = DateTimeOffset.Now.Offset.Hours;
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
            Debugger.Log("GameEntry.OnStart()");
            Event = new EventManager();
            Data = new DataManager();
            Fsm = new FsmManager();
            Procedure = new ProcedureManager();
            DataTable = new DataTableManager();
            Localization = new LocalizationManager();
            Pool = new PoolManager();
            Scene = new SceneManager();
            Loader = new LoaderManager();
            UI = new UIManager();
            Task = new TaskManager();
            
            //进入第一个流程
            Procedure.ChangeState(ProcedureState.Launch);
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
            Initialize();
            StartAutoSave();
        }

        private CancellationTokenSource _cts;
        private async UniTaskVoid StartAutoSave()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: _cts.Token);
                    Data.SaveData();
                }
            }
            catch (OperationCanceledException) { }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            // 当玩家切出游戏（比如接电话、滑出控制中心）时，失去焦点即保存
            if (!hasFocus)
            {
                Data.SaveData(true);
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                // 游戏进入后台时保存
                Data.SaveData(true);
                GameEntry.Event.Dispatch(Constants.EventName.GameEntryOnApplicationPause);
            }
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
            FpsGraphy.SetActive(true);
            return;
            // isOpen = !isOpen;
            // MainEntry.Reporter.ShowLogPanel(isOpen);
            StartCoroutine(GameUtil.LocationInfoCoroutine(null));
        }
    
        private void Test1()
        {
            QueueManager.Instance.AddEventTask("Hello","CloseHello");
        }
    
        
        public async UniTask LoginTest()
        {
// 1. 尝试使用 GET 而不是 POST，很多公共接口对 GET 校验较松
            string url = "https://api.live.bilibili.com/client/v1/Ip/getInfoNew";
    
            // 注意：这里由于你的 HttpManager 内部没有暴露添加 Header 的接口
            // 如果依然返回 bad token，说明 B 站强制校验了 Cookie
            var args = await HttpManager.Instance.GetArgsAsync(url, loadingCircle: false);
    
            if (!args.HasError)
            {
                // 这里的 Value 依然是 {"code":65530...} 或者正确的数据
                Debug.Log("B站回执: " + args.Value);
            }
        }
        
        private void Test2()
        {
            LoginTest();

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
            GameEntry.Event.Dispatch(Constants.EventName.EventMessage, new EventMessage("CloseHello"));
        }
    
        private void Test4()
        {
        }
    
        void Update()
        {
            Procedure.OnUpdate();
            Pool.OnUpdate();
            Scene.OnUpdate();
            UI.OnUpdate();
            Task.OnUpdate();
        }
    
        private void LateUpdate()
        {
        }
    
        private void OnApplicationQuit()
        {
            Data.SaveData(true);
            NetManager.Instance.DisConnectServer();
            LoggerManager.Instance.SyncLog();
            LoggerManager.Instance.Dispose();
            Fsm.Dispose();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
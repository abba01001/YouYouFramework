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

        private static FrameworkLanguage _currLanguage;
        public static FrameworkLanguage CurrLanguage
        {
            get => _currLanguage;
            set
            {
                if (_currLanguage == value) return;
                Debugger.Log("设置语言=>",value);
                _currLanguage = value;
                Config.Sys_LocalizationDBModel.RefreshLocaleDict();
                Event.Dispatch(Constants.EventName.LanguageChangedEvent);
            }
        }
    
        [Header("声音主混合器")] public AudioMixer MasterMixer;
        [Header("FPS")] public GameObject FpsGraphy;
    
        //管理器属性
        public static EventManager Event { get; private set; }
        public static DataManager Data { get; private set; }
        public static ProcedureManager Procedure { get; private set; }
        public static ConfigManager Config { get; private set; }
        public static PoolManager Pool { get; private set; }
        public static SceneManager Scene { get; private set; }
        public static LoaderManager Loader { get; private set; }
        public static UIManager UI { get; private set; }
        public static TaskManager Task { get; private set; }
        public static NotifyManager Notify { get; private set; }

        public static GameEntry Instance { get; private set; }
        private void Awake()
        {
            Debugger.Log("GameEntry.OnAwake()");
            Instance = this;
            TimeUtils.TimeZone = DateTimeOffset.Now.Offset.Hours;
            UIRootRectTransform = UIRootCanvasScaler.GetComponent<RectTransform>();
            // if (MainEntry.Reporter != null) MainEntry.Reporter.ShowLogPanel(false);
            switch (Application.systemLanguage)
            {
                default:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                case SystemLanguage.Chinese:
                    CurrLanguage = FrameworkLanguage.Chinese;
                    break;
                case SystemLanguage.English:
                    CurrLanguage = FrameworkLanguage.English;
                    break;
            }
            Application.targetFrameRate = 120;
    
    
            if (MacroSettings == null)
            {
                MacroSettings = m_MacroSettings;
            }
    
            //此处以后判断如果不是编辑器模式 要根据设备信息判断等级
            CurrDeviceGrade = m_CurrDeviceGrade;
            ParamsSettings = m_ParamsSettings;
            MacroSettings = m_MacroSettings;
        }
    
        private void Start()
        {
            Debugger.Log("GameEntry.OnStart()");
            Event = new EventManager();
            Data = new DataManager();

            Procedure = new ProcedureManager();
            Config = new ConfigManager();
            Pool = new PoolManager();
            Scene = new SceneManager();
            Loader = new LoaderManager();
            UI = new UIManager();
            Task = new TaskManager();
            Notify = new NotifyManager();
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
        }

        private CancellationTokenSource _cts;
        public async UniTaskVoid StartAutoSave()
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
            CurrLanguage = FrameworkLanguage.English;
            return;
            QueueManager.Instance.AddEventTask("Hello","CloseHello");
        }
    
        
        public async UniTask LoginTest()
        {
            // 1. 尝试使用 GET 而不是 POST，很多公共接口对 GET 校验较松
            string url = "https://api.live.bilibili.com/client/v1/Ip/getInfoNew";
            // 注意：这里由于你的 HttpManager 内部没有暴露添加 Header 的接口
            // 如果依然返回 bad token，说明 B 站强制校验了 Cookie
            var args = await HttpManager.Instance.GetStringAsync(url);
    
            Debugger.LogError("B站回执: " + args);
        }
        
        private void Test2()
        {
            CurrLanguage = FrameworkLanguage.Chinese;
            return;
            for (int i = 0; i < 5; i++)
            {
                LoginTest();
            }

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
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
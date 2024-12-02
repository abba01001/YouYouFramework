using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Collections;
using Main;
using MessagePack;
using MessagePack.Resolvers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace YouYou
{
    public class GameEntry : MonoBehaviour
    {
        [FoldoutGroup("ResourceGroup")]
        [Header("游戏物体对象池分组")]
        public GameObjectPoolEntity[] GameObjectPoolGroups;

        [FoldoutGroup("ResourceGroup")]
        [Header("对象池锁定的资源包")]
        public string[] LockedAssetBundle;

        [FoldoutGroup("UIGroup")]
        [Header("UI摄像机")]
        public Camera UICamera;

        [FoldoutGroup("UIGroup")]
        [Header("根画布的缩放")]
        public CanvasScaler UIRootCanvasScaler;
        public RectTransform UIRootRectTransform { get; private set; }

        [FoldoutGroup("UIGroup")]
        [Header("UI分组")]
        public UIGroup[] UIGroups;

        [FoldoutGroup("UIGroup")]
        [Header("屏幕阻挡这招")]
        public GameObject BlockMask;
        
        [FoldoutGroup("AudioGroup")]
        [Header("声音主混合器")]
        public AudioMixer MonsterMixer;

        [Header("当前语言（要和本地化表的语言字段 一致）")]
        [SerializeField]
        private YouYouLanguage m_CurrLanguage;
        public static YouYouLanguage CurrLanguage;

        
        //管理器属性
        public static LoggerManager Logger { get; private set; }
        public static EventManager Event { get; private set; }
        public static TimeManager Time { get; private set; }
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
        public static AudioManager Audio { get; private set; }
        public static AtlasManager Atlas { get; private set; }
        public static CrossPlatformInputManager Input { get; private set; }
        public static TaskManager Task { get; private set; }
        public static QualityManager Quality { get; private set; }
        public static SDKManager SDK { get; private set; }
        public static DialogueManager Dialogue { get; private set; }
        public static GuideManager Guide { get; private set; }

        /// <summary>
        /// 单例
        /// </summary>
        public static GameEntry Instance { get; private set; }

        private void Awake()
        {
            Log(LogCategory.Procedure, "GameEntry.OnAwake()");
            Instance = this;
            ShowBlockMask(false);
            UIRootRectTransform = UIRootCanvasScaler.GetComponent<RectTransform>();
            MainEntry.Reporter.ShowLogPanel(false);
            CurrLanguage = m_CurrLanguage;
        }
        private void Start()
        {
            Log(LogCategory.Procedure, "GameEntry.OnStart()");
            Logger = new LoggerManager();
            Event = new EventManager();
            Time = new TimeManager();
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
            Audio = new AudioManager();
            Atlas = new AtlasManager();
            Input = new CrossPlatformInputManager();
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
            Pool.Init();
            Scene.Init();
            Loader.Init();
            UI.Init();
            Audio.Init();
            Atlas.Init();
            SDK.Init();
            Dialogue.Init();
            Task.Init();
            Time.Init();
            //进入第一个流程
            Procedure.ChangeState(ProcedureState.Launch);
            
            Dictionary<(KeyCode, KeyCode?), Action> keyMappings = new Dictionary<(KeyCode, KeyCode?), Action>
            {
                {(KeyCode.Keypad0, KeyCode.LeftControl), Test0},
                {(KeyCode.Keypad1, KeyCode.LeftControl), Test1},
                {(KeyCode.Keypad2, KeyCode.LeftControl), Test2},
                {(KeyCode.Keypad3, KeyCode.LeftControl), Test3},
                {(KeyCode.Keypad4, KeyCode.LeftControl), Test4}
            };
            StopCoroutine(GameUtil.CheckKeys(keyMappings));
            StartCoroutine(GameUtil.CheckKeys(keyMappings));
            ViewQueueManager.Instance.RegisterEvents();
            Initialize();
        }

        public void ShowBlockMask(bool state)
        {
            BlockMask.SetActive(state);
        }
        
        private void Initialize()
        {
            StaticCompositeResolver.Instance.Register(
                MessagePack.Resolvers.GeneratedResolver.Instance,
                MessagePack.Resolvers.StandardResolver.Instance
            );
            var option = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);

            MessagePackSerializer.DefaultOptions = option;
        }

        private bool isOpen = false;
        private void Test0()
        {
            isOpen = !isOpen;
            MainEntry.Reporter.ShowLogPanel(isOpen);
        }
        
        private void Test1()
        {
            QueueManager.Instance.AddEventTask("Hello","CloseHello");
        }
        
        private void Test2()
        {
            QueueManager.Instance.AddTimeTask(1f, () =>
            {
                GameUtil.LogError("你好");
            }, () =>
            {
                GameUtil.LogError("结束，跳转下一个队列");
            });
        }
        
        private void Test3()
        {
            GameEntry.Event.Dispatch(Constants.EventName.EventMessage,new EventMessage("CloseHello"));
        }
        
        private void Test4()
        {
            
        }

        void Update()
        {
            Time.OnUpdate();
            Data.OnUpdate();
            Procedure.OnUpdate();
            Pool.OnUpdate();
            Scene.OnUpdate();
            Loader.OnUpdate();
            UI.OnUpdate();
            Audio.OnUpdate();
            Atlas.OnUpdate();
            SDK.OnUpdate();
            Dialogue.OnUpdate();
            Input.OnUpdate();
            Task.OnUpdate();
            GameEntry.Event.Dispatch(Constants.EventName.GameEntryOnUpdate);
        }

        private void LateUpdate()
        {
            
        }

        private void OnApplicationQuit()
        {
            Data.SaveData(true,true, true, true);
            Logger.SyncLog();
            Logger.Dispose();
            Fsm.Dispose();
            UploadLogData();
            GameEntry.Event.Dispatch(Constants.EventName.GameEntryOnApplicationQuit);
        }

        private void UploadLogData()
        {
            MainEntry.Reporter.WriteLogsToFile();
            SDK.UploadLogData(Data.UserId);
        }
        
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                Data.SaveData(true,true, true, true);
                GameEntry.Event.Dispatch(Constants.EventName.GameEntryOnApplicationPause);
            }
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
    }
}
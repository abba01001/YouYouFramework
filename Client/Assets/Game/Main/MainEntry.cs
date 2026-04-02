using Sirenix.OdinInspector;
using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using YooAsset;

namespace Main
{
    public class MainEntry : MonoBehaviour
    {
        /// <summary>
        /// 日志分类
        /// </summary>
        public enum LogCategory
        {
            /// <summary>
            /// 框架日志
            /// </summary>
            Framework,
            /// <summary>
            /// 流程
            /// </summary>
            Procedure,
            /// <summary>
            /// 资源管理
            /// </summary>
            Assets,
            /// <summary>
            /// 网络消息
            /// </summary>
            NetWork,
            /// <summary>
            /// 玩家游戏数据
            /// </summary>
            GameData,
        }

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

        /// <summary>
        /// Http调用失败后重试次数
        /// </summary>
        public static int HttpRetry { get; private set; }
        /// <summary>
        /// Http调用失败后重试间隔（秒）
        /// </summary>
        public static int HttpRetryInterval { get; private set; }

        //预加载相关事件
        public event Action ActionPreloadBegin;
        public event Action<float> ActionPreloadUpdate;
        public event Action ActionPreloadComplete;

        public static ClassObjectPool ClassObjectPool { get; private set; }

        public static ReporterManager Reporter { get; private set; }
        
        public static MainEntry Instance { get; private set; }
        EPlayMode ePlayMode;
        public static bool IsOfflineMode { get; set; } = false;//离线模式
        void OnValidate()
        {
            if (MacroSettings == null)
            {
                MacroSettings = m_MacroSettings;
            }
        }
        
        private void Awake()
        {
            Instance = this;
            Screen.sleepTimeout = SleepTimeout.NeverSleep; //屏幕常亮

            //此处以后判断如果不是编辑器模式 要根据设备信息判断等级
            CurrDeviceGrade = m_CurrDeviceGrade;
            ParamsSettings = m_ParamsSettings;
            MacroSettings = m_MacroSettings;
            
            //初始化系统参数
            HttpRetry = ParamsSettings.GetGradeParamData(YFConstDefine.Http_Retry, CurrDeviceGrade);
            HttpRetryInterval = ParamsSettings.GetGradeParamData(YFConstDefine.Http_RetryInterval, CurrDeviceGrade);
        }
        private async void Start()
        {
            ClassObjectPool = new ClassObjectPool();
            Reporter = gameObject.GetComponentInChildren<ReporterManager>();
            
            //开始检查更新

#if UNITY_EDITOR
            ePlayMode = EPlayMode.EditorSimulateMode;
#else
            ePlayMode = EPlayMode.HostPlayMode;
#endif
            
            CheckVersionCtrl.Instance.CheckVersionChange(ePlayMode, async () =>
            {
                // 检查更新完成, 加载Hotfix代码(HybridCLR)
                await HotfixManager.Instance.LoadHotifx();

                //启动YouYouFramework框架入口
                var operation = CheckVersionCtrl.Instance.DefaultPackage.LoadAssetAsync("Assets/Game/Download/Prefab/GameEntry.prefab");
                await operation.Task;
                GameObject gameEntryAsset = operation.AssetObject as GameObject;
                Instantiate(gameEntryAsset);
            });
            
        }
        
        private void Update()
        {
        }
        private void OnApplicationQuit()
        {
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
        
        public static void Log(object message)
        {
// #if DEBUG_LOG_NORMAL
            //由于性能原因，如果在Build Settings中没有勾上“Development Build”
            //即使开启了DEBUG_LOG_NORMAL也依然不打印普通日志， 只打印警告日志和错误日志
            // if (!Debug.isDebugBuild)
            // {
                // return;
            // }
            Debug.Log($"MainEntryLog==>{message}");
// #endif
        }
        public static void LogWarning(object message)
        {
// #if DEBUG_LOG_WARNING
            Debug.LogWarning($"MainEntryLog==>{message}");
// #endif
        }
        public static void LogError(object message)
        {
// #if DEBUG_LOG_ERROR
            Debug.LogError($"MainEntryLog==>{message}");
// #endif
        }

        public void PreloadBegin()
        {
            ActionPreloadBegin?.Invoke();
        }
        public void PreloadUpdate(float progress)
        {
            ActionPreloadUpdate?.Invoke(progress);
        }
        public void PreloadComplete()
        {
            ActionPreloadComplete?.Invoke();
        }
    }
}
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        /// <summary>
        /// 下载管理器
        /// </summary>
        public static DownloadManager Download { get; private set; }
        /// <summary>
        /// 资源管理器
        /// </summary>
        public static CheckVersionManager Assets { get; private set; }
        /// <summary>
        /// 类对象池
        /// </summary>
        public static ClassObjectPool ClassObjectPool { get; private set; }

        /// <summary>
        /// 热更新管理器
        /// </summary>
        public static HotfixManager Hotfix { get; private set; }

        public static ReporterManager Reporter { get; private set; }
        
        /// <summary>
        /// 单例
        /// </summary>
        public static MainEntry Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            //屏幕常亮
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            //此处以后判断如果不是编辑器模式 要根据设备信息判断等级
            CurrDeviceGrade = m_CurrDeviceGrade;
            ParamsSettings = m_ParamsSettings;

            //初始化系统参数
            HttpRetry = ParamsSettings.GetGradeParamData(YFConstDefine.Http_Retry, CurrDeviceGrade);
            HttpRetryInterval = ParamsSettings.GetGradeParamData(YFConstDefine.Http_RetryInterval, CurrDeviceGrade);

        }
        private void Start()
        {
            //初始化管理器
            Download = new DownloadManager();
            Assets = new CheckVersionManager();
            ClassObjectPool = new ClassObjectPool();
            Hotfix = new HotfixManager();
            Reporter = gameObject.GetComponentInChildren<ReporterManager>();
            
            Download.Init();
            Assets.Init();
            Hotfix.Init();
        }
        private void Update()
        {
            Download.OnUpdate();
        }
        private void OnApplicationQuit()
        {
            Download.Dispose();
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
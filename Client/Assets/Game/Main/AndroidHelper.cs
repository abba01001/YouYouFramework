using UnityEngine;

namespace Main
{
    /// <summary>
    /// 安卓原生工具类 (Framework 扩展)
    /// 对应 Java 类: com.framework.app.UnityAndroidUtils
    /// </summary>
    public static class AndroidHelper
    {
        private const string JavaClassName = "com.framework.app.UnityAndroidUtils";
        private static AndroidJavaClass _utils;
        private static bool _isInitialized = false;

        /// <summary>
        /// 初始化工具类 (建议在游戏初始化流程中调用)
        /// </summary>
        public static void Init()
        {
            if (_isInitialized) return;

#if !UNITY_EDITOR && UNITY_ANDROID
        try
        {
            _utils = new AndroidJavaClass(JavaClassName);
            _isInitialized = true;
            Debug.Log("[AndroidHelper] Android Native Utils Initialized.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AndroidHelper] Initialization Failed: {e.Message}");
        }
#else
            Debug.LogWarning("[AndroidHelper] Non-Android environment, native features are disabled.");
#endif
        }

        /// <summary>
        /// 内部安全检查
        /// </summary>
        private static bool CheckReady()
        {
            if (!_isInitialized)
            {
                Init();
            }

            return _isInitialized;
        }

        // --- 1. UI 与 交互 ---

        /// <summary>
        /// 弹出原生 Toast 提示
        /// </summary>
        public static void ShowToast(string msg)
        {
            if (!CheckReady()) return;
            _utils.CallStatic("showToast", msg);
        }

        /// <summary>
        /// 复制文本到剪贴板
        /// </summary>
        public static void CopyToClipboard(string text)
        {
            if (!CheckReady()) return;
            _utils.CallStatic("copyToClipboard", text);
        }

        // --- 2. 设备信息 (用于性能分级) ---

        /// <summary>
        /// 获取系统总内存 (MB)
        /// </summary>
        public static long GetTotalMemory()
        {
            if (!CheckReady()) return 0;
            return _utils.CallStatic<long>("getTotalMemory");
        }

        /// <summary>
        /// 获取当前可用内存 (MB)
        /// </summary>
        public static long GetAvailableMemory()
        {
            if (!CheckReady()) return 0;
            return _utils.CallStatic<long>("getAvailableMemory");
        }

        /// <summary>
        /// 获取设备唯一标识 (AndroidID)
        /// </summary>
        public static string GetAndroidID()
        {
            if (!CheckReady()) return string.Empty;
            return _utils.CallStatic<string>("getAndroidID");
        }

        // --- 3. 屏幕与适配 ---

        /// <summary>
        /// 获取异形屏/刘海屏顶部安全高度 (像素)
        /// </summary>
        public static int GetNotchHeight()
        {
            if (!CheckReady()) return 0;
            return _utils.CallStatic<int>("getNotchHeight");
        }

        // --- 4. 网络状态 ---

        /// <summary>
        /// 获取网络状态 (0:无网络, 1:WiFi, 2:蜂窝数据, 3:其他)
        /// </summary>
        public static int GetNetworkStatus()
        {
            if (!CheckReady()) return 0;
            return _utils.CallStatic<int>("getNetworkStatus");
        }

        // --- 5. 系统功能 ---

        /// <summary>
        /// 安装指定路径的 APK (需配合 FileProvider 使用)
        /// </summary>
        /// <param name="apkPath">APK 文件的绝对路径</param>
        public static void InstallApk(string apkPath)
        {
            if (!CheckReady()) return;
            _utils.CallStatic("installApk", apkPath);
        }

        /// <summary>
        /// 获取应用版本号 (VersionCode)
        /// </summary>
        public static long GetVersionCode()
        {
            if (!CheckReady()) return 0;
            return _utils.CallStatic<long>("getVersionCode");
        }
    }
}
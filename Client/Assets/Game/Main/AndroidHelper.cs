using UnityEngine;

/// <summary>
/// 安卓原生工具类 (Framework 扩展)
/// 对应 Java 类: com.framework.app.UnityAndroidUtils
/// </summary>
namespace Main
{
    public static class AndroidHelper
    {
        private const string JavaClassName = "com.framework.app.UnityAndroidUtils";
        private static AndroidJavaClass _utils;
        private static bool _isInitialized = false;

        
        static AndroidHelper()
        {
            Init();
        }
        
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
            catch (System.Exception e) { Debug.LogError($"[AndroidHelper] Init Failed: {e}"); }
#else
            Debug.LogWarning("[AndroidHelper] Non-Android environment, native features are disabled.");
#endif
        }

        // --- 设备信息 (保留原属性) ---

        /// <summary> 获取唯一设备标识符 </summary>
        public static string DeviceIdentifier => SystemInfo.deviceUniqueIdentifier;

        /// <summary> 获取设备型号 </summary>
        public static string DeviceModel
        {
            get
            {
#if UNITY_IPHONE && !UNITY_EDITOR
                return UnityEngine.iOS.Device.generation.ToString();
#else
                return SystemInfo.deviceModel;
#endif
            }
        }
        
        // --- 1. UI 与 交互 ---

        /// <summary> 显示原生 Toast 提示 </summary>
        public static void ShowToast(string msg) => Execute("showToast", msg);

        /// <summary> 复制文本到剪贴板 </summary>
        public static void CopyToClipboard(string text) => Execute("copyToClipboard", text);

        // --- 2. 设备信息 (扩展) ---

        /// <summary> 获取总内存 (MB) </summary>
        public static long GetTotalMemory() => GetValue<long>("getTotalMemory", 0);

        /// <summary> 获取可用内存 (MB) </summary>
        public static long GetAvailableMemory() => GetValue<long>("getAvailableMemory", 0);

        /// <summary> 获取 Android ID </summary>
        public static string GetAndroidID() => GetValue<string>("getAndroidID", string.Empty);

        // --- 3. 屏幕与适配 ---

        /// <summary> 获取异形屏顶部安全高度 (px) </summary>
        public static int GetNotchHeight() => GetValue<int>("getNotchHeight", 0);

        /// <summary> 获取屏幕密度 (density) </summary>
        public static float GetScreenDensity() => GetValue<float>("getScreenDensity", 1.0f);

        // --- 4. 网络状态 ---

        /// <summary> 获取网络状态: 0=无, 1=WIFI, 2=移动网络, 3=其他 </summary>
        public static int GetNetworkStatus() => GetValue<int>("getNetworkStatus", 0);

        // --- 5. 系统功能 ---

        /// <summary> 安装 APK (需传入本地路径) </summary>
        public static void InstallApk(string apkPath) => Execute("installApk", apkPath);

        /// <summary> 获取当前版本号 (VersionCode) </summary>
        public static long GetVersionCode() => GetValue<long>("getVersionCode", 0);

        // --- 辅助调用方法 (统一异常捕获) ---

        private static T GetValue<T>(string methodName, T defaultValue)
        {
            try { return _utils.CallStatic<T>(methodName); }
            catch (System.Exception e) { Debug.LogError($"[AndroidHelper] {methodName} Error: {e}"); return defaultValue; }
        }

        private static void Execute(string methodName, params object[] args)
        {
            try { _utils.CallStatic(methodName, args); }
            catch (System.Exception e) { Debug.LogError($"[AndroidHelper] {methodName} Error: {e}"); }
        }
    }
}
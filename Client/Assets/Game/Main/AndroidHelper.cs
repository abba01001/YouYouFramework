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

        /// <summary>
        /// 初始化工具类
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

        private static bool CheckReady()
        {
            if (!_isInitialized) Init();
            return _isInitialized;
        }

        // --- 1. UI 与 交互 ---

        public static void ShowToast(string msg) => Execute(() => _utils.CallStatic("showToast", msg));
        
        public static void CopyToClipboard(string text) => Execute(() => _utils.CallStatic("copyToClipboard", text));

        // --- 2. 设备信息 ---

        public static long GetTotalMemory() => GetValue<long>("getTotalMemory", 0);
        
        public static long GetAvailableMemory() => GetValue<long>("getAvailableMemory", 0);
        
        public static string GetAndroidID() => GetValue<string>("getAndroidID", string.Empty);

        // --- 3. 屏幕与适配 ---

        /// <summary>
        /// 获取异形屏顶部安全高度 (px)
        /// 注意：在小米 14 等手机上，需配合 "Render outside safe area" 设置使用
        /// </summary>
        public static int GetNotchHeight() => GetValue<int>("getNotchHeight", 0);

        /// <summary>
        /// 获取屏幕密度 (density)
        /// </summary>
        public static float GetScreenDensity() => GetValue<float>("getScreenDensity", 1.0f);

        // --- 4. 网络状态 ---

        public static int GetNetworkStatus() => GetValue<int>("getNetworkStatus", 0);

        // --- 5. 系统功能 ---

        public static void InstallApk(string apkPath) => Execute(() => _utils.CallStatic("installApk", apkPath));

        public static long GetVersionCode() => GetValue<long>("getVersionCode", 0);

        // --- 辅助方法 ---

        private static void Execute(System.Action action)
        {
            if (CheckReady()) action.Invoke();
        }

        private static T GetValue<T>(string methodName, T defaultValue)
        {
            if (!CheckReady()) return defaultValue;
            try { return _utils.CallStatic<T>(methodName); }
            catch { return defaultValue; }
        }
    }
}
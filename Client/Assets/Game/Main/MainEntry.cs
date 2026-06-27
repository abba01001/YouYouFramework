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

        public VersionUpdatePanel VersionUpdatePanel;
        public static MainEntry Instance { get; private set; }
        public static bool IsOfflineMode { get; set; } = false;//离线模式

        private void Awake()
        {
            Instance = this;
            Screen.sleepTimeout = SleepTimeout.NeverSleep; //屏幕常亮
            Application.logMessageReceived += HandleLog;

        }
        private async void Start()
        {
            EPlayMode ePlayMode;
            //开始检查更新
#if UNITY_EDITOR
            ePlayMode = EPlayMode.EditorSimulateMode;
#elif UNITY_WEBGL
            ePlayMode = EPlayMode.WebPlayMode;
#else
            ePlayMode = EPlayMode.OfflinePlayMode;
            // ePlayMode = EPlayMode.HostPlayMode;
#endif
            bool isNeedInstallAPK = await CheckVersionCtrl.Instance.CheckMajorVersion(ePlayMode);
            if (isNeedInstallAPK)
            {
                VersionUpdatePanel.Show();
                var progress = new Progress<float>(value => 
                {
                    VersionUpdatePanel.UpdateProgress(value);
                });
                VersionUpdatePanel.DownLoadAction = async () =>
                {
                    string apkPath = await CheckVersionCtrl.Instance.DownloadAndInstallFullAPK(progress);
                    if (!string.IsNullOrEmpty(apkPath))
                    {
                        AndroidHelper.InstallApk(apkPath);
                    }
                    else
                    {
                        // 这里处理下载失败的逻辑（例如弹出提示框）
                    }
                };
                return;
            }
            
            //先不走资源比较了
            CheckVersionCtrl.Instance.CheckVersionChange(ePlayMode, async () =>
            {
                // 检查更新完成, 加载Hotfix代码(HybridCLR)
                await HotfixManager.Instance.LoadHotifx();

                //启动Framework框架入口
                var operation = CheckVersionCtrl.Instance.DefaultPackage.LoadAssetAsync("Assets/Game/Download/Prefab/GameEntry.prefab");
                await operation.Task;
                GameObject gameEntryAsset = operation.AssetObject as GameObject;
                Instantiate(gameEntryAsset);
            });
            
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void Update()
        {
        }
        private void OnApplicationQuit()
        {
        }
        
        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            // logString: 日志内容
            // stackTrace: 日志对应的堆栈信息
            // type: 日志类型 (Log, Warning, Error, Exception, Assert)

            if (type == LogType.Error || type == LogType.Exception)
            {
                // 在这里处理你的全局错误逻辑
                // 例如：发送到服务器，或显示在自定义的 UI 调试窗口中
                Debug.Log($"全局拦截到错误日志: {logString}");
            }
        }
    }
}
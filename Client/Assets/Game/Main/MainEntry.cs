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
        //预加载相关事件
        public event Action ActionPreloadBegin;
        public event Action<float> ActionPreloadUpdate;
        public event Action ActionPreloadComplete;


        public static MainEntry Instance { get; private set; }
        EPlayMode ePlayMode;
        public static bool IsOfflineMode { get; set; } = false;//离线模式

        private void Awake()
        {
            Instance = this;
            Screen.sleepTimeout = SleepTimeout.NeverSleep; //屏幕常亮

        }
        private async void Start()
        {
            //开始检查更新
#if UNITY_EDITOR
            ePlayMode = EPlayMode.EditorSimulateMode;
#else
            ePlayMode = EPlayMode.OfflinePlayMode;
            // ePlayMode = EPlayMode.HostPlayMode;
#endif
            bool isNeedInstallAPK = await CheckVersionCtrl.Instance.CheckMajorVersion(ePlayMode);
            if (isNeedInstallAPK)
            {
                // 如果需要大更，直接调用你写好的 installApk
                // 这里的回调就不执行了，因为进程会被杀掉或退出
                CheckVersionCtrl.Instance.DownloadAndInstallFullAPK();
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
        
        private void Update()
        {
        }
        private void OnApplicationQuit()
        {
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
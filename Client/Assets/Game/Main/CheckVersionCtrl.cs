using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using Main;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

namespace Main
{
    public class CheckVersionCtrl
    {
        public static CheckVersionCtrl Instance = new();

        public string LocalPackageVersion => PlayerPrefs.GetString("PackageVersion", "");
        public string RemotePackageVersion { get; private set; }

        public event Action CheckVersionBeginDownload;
        public event Action<DownloadStatus> CheckVersionDownloadUpdate;
        public event Action CheckVersionDownloadComplete;

        public string DefaultPackageName { get; private set; } = "DefaultPackage";
        public ResourcePackage DefaultPackage { get; private set; }

        private Action CheckVersionComplete;

        public async void CheckVersionChange(EPlayMode playMode, Action onComplete)
        {
            CheckVersionComplete = onComplete;
            CheckVersionComplete += () =>
            {
                PlayerPrefs.SetString("PackageVersion",DefaultPackage.GetPackageVersion());
            };
            // 初始化资源系统
            YooAssets.Initialize();

            // 创建默认的资源包
            DefaultPackage = YooAssets.CreatePackage(DefaultPackageName);

            // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
            YooAssets.SetDefaultPackage(DefaultPackage);

            // 编辑器下的模拟模式
            InitializationOperation initializationOperation = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(DefaultPackageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters =
                    FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
                initializationOperation = DefaultPackage.InitializeAsync(createParameters);
            }

            // 单机运行模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                initializationOperation = DefaultPackage.InitializeAsync(createParameters);
            }

            // 联机运行模式
            if (playMode == EPlayMode.HostPlayMode)
            {
                string defaultHostServer = GetHostServerURL();
                string fallbackHostServer = GetHostServerURL();
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                var createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters =
                    FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                createParameters.CacheFileSystemParameters =
                    FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                initializationOperation = DefaultPackage.InitializeAsync(createParameters);
            }

            // WebGL运行模式
            if (playMode == EPlayMode.WebPlayMode)
            {
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
			string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);

            // 微信小游戏缓存根目录
            // 注意：此处代码根据微信插件配置来填写！
            string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE/yoo";
            
            var createParameters = new WebPlayModeParameters();
            createParameters.WebServerFileSystemParameters =
 WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
            initializationOperation = DefaultPackage.InitializeAsync(createParameters);
#else
                var createParameters = new WebPlayModeParameters();
                createParameters.WebServerFileSystemParameters =
                    FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
                initializationOperation = DefaultPackage.InitializeAsync(createParameters);
#endif
            }

            await initializationOperation;

            if (initializationOperation.Status != EOperationStatus.Succeed)
            {
                Debugger.LogWarning($"资源包初始化失败：{initializationOperation.Error}");
                return;
            }

            Debugger.Log("资源包初始化成功！");

            //获取资源版本
            var operationVersion = DefaultPackage.RequestPackageVersionAsync();
            await operationVersion;
            Debugger.Log($"获取资源版本！operationVersion状态:{operationVersion.Status}");
            
            if (operationVersion.Status != EOperationStatus.Succeed)
            {
                Debugger.LogWarning($"获取资源版本失败：{operationVersion.Error}");
                return;
            }

            Debugger.Log($"获取资源版本成功 : {operationVersion.PackageVersion}");
            RemotePackageVersion = operationVersion.PackageVersion;
            
            //更新资源清单
            var operationManifest = DefaultPackage.UpdatePackageManifestAsync(operationVersion.PackageVersion);
            await operationManifest;
            if (operationManifest.Status != EOperationStatus.Succeed)
            {
                Debugger.LogWarning($"更新资源清单失败：{operationManifest.Error}");
                return;
            }

            Debugger.Log("更新资源清单成功");

#if UNITY_EDITOR
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                Debugger.Log("编辑器加载模式 不需要检查更新");
                CheckVersionComplete.Invoke();
                return;
            }
#endif

            //资源包下载
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var downloader = DefaultPackage.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

            if (downloader.TotalDownloadCount == 0)
            {
                Debugger.Log("没有需要下载的资源");
                CheckVersionComplete?.Invoke();
                return;
            }

            // TODO: 注意：开发者需要在下载前检测磁盘空间不足
            // 需要下载的文件总数和总大小
            int totalDownloadCount = downloader.TotalDownloadCount;
            long totalDownloadBytes = downloader.TotalDownloadBytes;

            //注册回调方法
            // downloader.DownloadFinishCallback = OnDownloadFinishFunction; //当下载器结束（无论成功或失败）
            // downloader.DownloadErrorCallback = OnDownloadErrorFunction; //当下载器发生错误
            // downloader.DownloadUpdateCallback = OnDownloadUpdateFunction; //当下载进度发生变化
            // downloader.DownloadFileBeginCallback = OnDownloadFileBeginFunction; //当开始下载某个文件

            //开启下载
            downloader.BeginDownload();
            await downloader;

            //检测下载结果
            if (downloader.Status == EOperationStatus.Succeed)
            {
                Debugger.Log("检查更新下载完毕, 进入预加载流程");

                CheckVersionDownloadComplete?.Invoke();
                CheckVersionComplete?.Invoke();
            }
            else
            {
                Debugger.LogError("检查更新失败, 请点击重试");
                // MainDialogForm.ShowForm("检查更新失败, 请点击重试", "Error", "重试", "", MainDialogForm.DialogFormType.Affirm, () =>
                // {
                //     CheckVersionChange(playMode, CheckVersionComplete);
                // });
            }
        }

        /// <summary>
        /// 获取资源服务器地址
        /// </summary>
        private string GetHostServerURL()
        {
            string url = HotfixManager.Instance.GetAssetIP();
            return url;
        }

        private string GetHostVersionURL()
        {
            string url = HotfixManager.Instance.GetVersionIP();
            return url;
        }

        public async UniTask<string> RequestRemoteVersion()
        {
            string url = $"{GetHostVersionURL()}/Version.txt";
            // 调用现有的 HttpManager，自动处理重试、并发拦截和超时
            string version = await HttpManager.Instance.GetStringAsync(url);
            // 如果 result.HasError 为 true，GetStringAsync 会返回 null
            return version?.Trim();
        }
        public async UniTask<bool> CheckMajorVersion(EPlayMode playMode)
        {
            if (playMode != EPlayMode.HostPlayMode) return false;

            // 1. 获取版本号，增加空值处理
            string remotePackageVersion = await RequestRemoteVersion();
            if (string.IsNullOrEmpty(remotePackageVersion))
            {
                Debugger.LogError("[大版本更新] 无法获取远端版本号，请检查网络或 URL");
                // 根据业务需求，这里可以返回 true 强制弹窗提示网络异常，或返回 false 跳过
                return false; 
            }

            // 2. 使用安全分割，防止格式不正确导致报错
            string[] parts = remotePackageVersion.Split('_');
            string remoteAppVersion = parts[0]; 

            Debugger.Log($"本地版本{Application.version}远端版本{remoteAppVersion}");
            // 3. 版本对比
            if (remoteAppVersion != Application.version)
            {
                Debugger.Log($"[大版本更新] 远端版本 {remoteAppVersion} != 当前版本 {Application.version}");
                return true;
            }
    
            Debugger.Log("APK 版本匹配，准备进入 YooAsset 资源热更流程...");
            return false;
        }
        
        public async UniTask<string> DownloadAndInstallFullAPK(IProgress<float> progress = null)
        {
            string savePath = Path.Combine(Application.persistentDataPath, "test.apk");
            string downloadPath = GetHostVersionURL() + "/test.apk";

            long localFileSize = File.Exists(savePath) ? new FileInfo(savePath).Length : 0;
            long totalFileSize = await GetRemoteFileSize(downloadPath);

            float sizeInMB = localFileSize / 1048576f;
            Debugger.LogError($"下载地址:{downloadPath}\n已下载文件大小: {sizeInMB:F2} MB");
                        
            // 容错关键点：如果发现本地文件大小异常或服务器文件更新，直接清理
            if (localFileSize >= totalFileSize && totalFileSize > 0)
            {
                Debugger.Log("本地文件已存在且完整，跳过下载或重新验证...");
                return savePath;
            }

            using (var request = new UnityWebRequest(downloadPath, UnityWebRequest.kHttpVerbGET))
            {
                // 只有当本地有文件时才尝试断点续传
                if (localFileSize > 0)
                {
                    request.SetRequestHeader("Range", $"bytes={localFileSize}-");
                }

                request.downloadHandler = new DownloadHandlerFile(savePath, true);
                var operation = request.SendWebRequest();

                await operation; // 等待请求完成，确认响应码

                // 【核心容错逻辑】：如果服务器不支持续传（返回 200），则重置本地文件并重新下载
                if (localFileSize > 0 && request.responseCode == 200)
                {
                    Debugger.LogWarning("服务器不支持断点续传，检测到续传请求被降级为全量下载，正在清空旧文件并重试...");
                    File.Delete(savePath);
                    return await DownloadAndInstallFullAPK(progress); // 递归调用一次，此时 localFileSize 为 0
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return savePath;
                }
                else
                {
                    Debugger.LogError($"下载失败: {request.error}");
                    return null;
                }
            }
        }

        // 辅助方法：获取文件总大小
        private async UniTask<long> GetRemoteFileSize(string url)
        {
            using (var request = UnityWebRequest.Head(url))
            {
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return long.Parse(request.GetResponseHeader("Content-Length"));
                }
            }
            return 0;
        }

        /// <summary>
        /// 远端资源地址查询服务类
        /// </summary>
        private class RemoteServices : IRemoteServices
        {
            private readonly string _defaultHostServer;
            private readonly string _fallbackHostServer;

            public RemoteServices(string defaultHostServer, string fallbackHostServer)
            {
                _defaultHostServer = defaultHostServer;
                _fallbackHostServer = fallbackHostServer;
            }

            string IRemoteServices.GetRemoteMainURL(string fileName)
            {
                return $"{_defaultHostServer}/{fileName}";
            }

            string IRemoteServices.GetRemoteFallbackURL(string fileName)
            {
                return $"{_fallbackHostServer}/{fileName}";
            }
        }

    }
}
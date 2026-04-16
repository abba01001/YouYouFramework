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

public class CheckVersionCtrl
{
    public static CheckVersionCtrl Instance = new();

    public event Action CheckVersionBeginDownload;
    public event Action<DownloadStatus> CheckVersionDownloadUpdate;
    public event Action CheckVersionDownloadComplete;

    public string DefaultPackageName { get; private set; } = "DefaultPackage";
    public ResourcePackage DefaultPackage { get; private set; }

    private Action CheckVersionComplete;
    public async void CheckVersionChange(EPlayMode playMode, Action onComplete)
    {
        CheckVersionComplete = onComplete;

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
            createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            initializationOperation = DefaultPackage.InitializeAsync(createParameters);
        }

        // 单机运行模式
        if (playMode == EPlayMode.OfflinePlayMode)
        {
            var createParameters = new OfflinePlayModeParameters();
            createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            initializationOperation = DefaultPackage.InitializeAsync(createParameters);
        }

        // 联机运行模式
        if (playMode == EPlayMode.HostPlayMode)
        {
            string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            var createParameters = new HostPlayModeParameters();
            createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            createParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            initializationOperation = DefaultPackage.InitializeAsync(createParameters);
            Debug.LogError($"111地址===>{defaultHostServer}");
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
            createParameters.WebServerFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
            initializationOperation = DefaultPackage.InitializeAsync(createParameters);
#else
            var createParameters = new WebPlayModeParameters();
            createParameters.WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
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
        string url = $"{GetHostVersionURL()}/Version.txt"; // 里面只写 1.4.0_xxx
        var request = UnityEngine.Networking.UnityWebRequest.Get(url);
        await request.SendWebRequest();

        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            return request.downloadHandler.text.Trim();
        }
        return null;
    }

    public async UniTask<bool> CheckMajorVersion(EPlayMode playMode)
    {
        if (playMode != EPlayMode.HostPlayMode) return false;
        //这里检测是否跨版本
        string remotePackageVersion = await RequestRemoteVersion();
        string remoteAppVersion = remotePackageVersion.Split('_')[0]; // 结果为 "1.3.0"
        if (remoteAppVersion != Application.version)
        {
            // 如果不一致，说明有大版本更新
            Debug.Log($"[大版本更新] 远端 APK 版本 {remoteAppVersion} != 当前版本 {Application.version}");
            return true;
        }
        else
        {
            // 如果一致，说明只需要热更资源
            Debug.Log("APK 版本匹配，准备进入 YooAsset 资源热更流程...");
            return false;
        }
    }

    public async UniTask DownloadAndInstallFullAPK()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "test.apk");
        string downloadPath = GetHostVersionURL() + "/test.apk";

        // --- 1. 检查并清理旧文件 ---
        if (File.Exists(savePath))
        {
            Debug.Log("检测到本地已存在同名APK，正在清理以准备新下载...");
            File.Delete(savePath);
        }

        // --- 2. 确保目录存在 ---
        string directory = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using (var request = new UnityWebRequest(downloadPath, UnityWebRequest.kHttpVerbGET))
        {
            Debug.Log("下载地址:"+downloadPath);
            // DownloadHandlerFile(path, append) 
            // 第二个参数默认为 false，表示覆盖写入。但物理删除（Step 1）更保险。
            request.downloadHandler = new DownloadHandlerFile(savePath);

            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                // 1. 检查 HTTP 状态码
                // 如果是 404, 进度永远是 0
                if (request.responseCode > 0 && request.responseCode != 200)
                {
                    Debug.LogError($"服务器返回错误码: {request.responseCode} (如果是 404 说明文件不存在)");
                    break; 
                }
                // 2. 检查是否有数据流入
                // 如果这里一直为 0，说明服务器根本没吐数据出来
                Debug.Log($"数据检查 - 已下载字节: {request.downloadedBytes} | 原始进度: {request.downloadProgress}");
                await UniTask.NextFrame();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                // 如果是 404 或连接失败，这里会报详细原因
                Debug.LogError($"[网络故障] 原因: {request.error} | URL: {downloadPath}");
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("<color=#00FF00>下载完成，执行安装。</color>");
                AndroidHelper.InstallApk(savePath);
            }
            else
            {
                Debug.LogError($"下载 APK 失败: {request.error}");
                // 如果下载失败，清理掉可能下载了一半的残包，防止下次进来误判
                if (File.Exists(savePath)) File.Delete(savePath);
            }
        }
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

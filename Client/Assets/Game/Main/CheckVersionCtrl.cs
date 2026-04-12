using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using Main;
using UnityEngine;
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
    
    string GetLocalIPAddress()
    {
        foreach (var netInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        {
            if (netInterface.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
                continue;

            var props = netInterface.GetIPProperties();
            foreach (var addr in props.UnicastAddresses)
            {
                if (addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !addr.Address.ToString().StartsWith("127"))
                {
                    return "http://" + addr.Address.ToString() + ":8000";
                }
            }
        }
        return "http://127.0.0.1:8000";
    }

    /// <summary>
    /// 获取资源服务器地址
    /// </summary>
    private string GetHostServerURL()
    {
        // return $"http://storage.abba01001.cn/private_files/ServerBundles/Android/{Application.version}";
        return $"http://192.168.160.47:8000/Android/{Application.version}";
        return $"http://192.168.124.45:8000/Android/{Application.version}";
        //string hostServerIP = "http://10.0.0.127"; //安卓模拟器地址
        // string hostServerIP = "http://127.0.0.1";
        string hostServerIP = GetLocalIPAddress();
        return $"{hostServerIP}/Android/{Application.version}";
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

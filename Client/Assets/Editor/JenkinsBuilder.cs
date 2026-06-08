using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

public static class JenkinsBuilder
{
    private const string KeystoreRelativePath = "Tools/user.keystore";
    private const string KeystorePassword = "FrameWork";
    private const string KeyAlias = "key";
    private const string KeyPassword = "FrameWork";
    const string BuildAppConfigFile = "Tools/Jenkins/BuildAppConfig.json";
    const string BuildResourceConfigFile = "Tools/Jenkins/BuildResourceConfig.json";


    #region --- 通用辅助方法 ---

    private static void SetKeystoreInfo()
    {
        PlayerSettings.Android.keystoreName = KeystoreRelativePath;
        PlayerSettings.Android.keystorePass = KeystorePassword;
        PlayerSettings.Android.keyaliasName = KeyAlias;
        PlayerSettings.Android.keyaliasPass = KeyPassword;
    }

    private static string[] GetProjectScenes()
    {
        string targetScene = "Assets/Game/Scene_Launch.unity";
        return File.Exists(targetScene) ? new string[] { targetScene } : new string[] { "Assets/Game/Main.unity" };
    }

    private static bool ValidateKeystore()
    {
        string absKeystorePath = Path.Combine(Environment.CurrentDirectory, KeystoreRelativePath);
        if (File.Exists(absKeystorePath))
        {
            SetKeystoreInfo();
            return true;
        }

        Debug.LogError($"[Jenkins] 错误: 签名文件不存在 -> {absKeystorePath}");
        return false;
    }

    private static T LoadConfig<T>(string path) where T : class
    {
        var fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, path).Replace("\\", "/");
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"[Jenkins] 配置文件不存在: {fullPath}");
            return null;
        }

        string jsonStr = File.ReadAllText(fullPath);
        T config = JsonConvert.DeserializeObject<T>(jsonStr);
        if (config != null)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"\n[Jenkins] 成功加载配置: {path}");
            sb.AppendLine("[Jenkins] 参数列表如下：");
            sb.AppendLine("--------------------------------------------------");

            var fields = typeof(T).GetFields();
            foreach (var field in fields)
            {
                sb.AppendLine($"  {field.Name,-20} : {field.GetValue(config)}");
            }

            sb.AppendLine("--------------------------------------------------");
            Debug.Log(sb.ToString());
        }

        return config;
    }

    private static void TryDeleteStreamingAssets()
    {
        string saPath = Application.streamingAssetsPath;
        // 使用带重试机制的删除
        if (Directory.Exists(saPath))
        {
            try
            {
                Directory.Delete(saPath, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Jenkins] 无法完全清理 StreamingAssets: {e.Message}");
            }
        }
    }

    private static bool RunYooAsset(BuildTarget target, JenkinsBuildResourceConfig jenkinsBuildResourceConfig)
    {
        CustomYooAssetBuild.CopyHotfixDll(target);
        string saPath = Application.streamingAssetsPath;
        // 强制同步导入，确保 YooAsset 读取到的是最新的文件
        AssetDatabase.Refresh();
        Directory.CreateDirectory(saPath);
        return CustomYooAssetBuild.BuildInternal(target, jenkinsBuildResourceConfig);
    }

    private static bool RunHybridCLR()
    {
        try
        {
            HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Jenkins] HybridCLR 错误: {e.Message}");
            return false;
        }
    }

    private static bool RunPlayerBuild(JenkinsBuildAppConfig config)
    {
        string folder = Path.Combine(config.OutputDir, config.Platform.ToString(), config.BuildType, config.Version);
        string path = (config.Platform == BuildTarget.Android) ? Path.Combine(folder, "MonsterSurvivors.apk") : folder;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetProjectScenes(),
            locationPathName = path,
            target = config.Platform,
            options = BuildOptions.CompressWithLz4 | (config.IsDebugMode ? BuildOptions.Development : BuildOptions.None)
        };

        var report = BuildPipeline.BuildPlayer(options);
        return report.summary.result == BuildResult.Succeeded;
    }

    private static void Exit(BuildErrorCode code)
    {
        string desc = code.GetDescription();
        string tip = $"构建终止！错误代码: {(int)code} | 错误详情: {desc}";
        Debug.LogError($"[Jenkins] {tip}");
        NotifyFeiShu(tip);
        EditorApplication.Exit((int)code);
    }
    
    static string url = "https://open.feishu.cn/open-apis/bot/v2/hook/dbaa8e33-bb70-46a5-98b8-484574bb2519";
    public static async UniTask NotifyFeiShu(string desc)
    {
        string keyword = "[Jenkins] ";
        var payload = new
        {
            msg_type = "text",
            content = new { text = keyword + desc }
        };

        string json = JsonConvert.SerializeObject(payload);
        using var client = new HttpClient();
        var contentBody = new StringContent(json, Encoding.UTF8, "application/json");
        try 
        {
            // 使用 await 确保请求真正发送出去
            var response = await client.PostAsync(url, contentBody);
            string responseString = await response.Content.ReadAsStringAsync();
            UnityEngine.Debug.Log("飞书响应: " + responseString); 
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("网络请求失败: " + e.Message);
        }
    }

    private static bool CheckAndSwitchPlatform(BuildTarget platform)
    {
        if (EditorUserBuildSettings.activeBuildTarget != platform)
        {
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(platform);
            Debug.Log($"#########切换平台,TargetGroup:{buildTargetGroup}, BuildTarget:{platform}#######");
            return EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, platform);
        }

        return true;
    }
    #endregion
    
    //Jenkins构建APP
    public static void BuildApp()
    {
        DateTime startTime = DateTime.Now;
        
        AssetDatabase.Refresh();
        var config = LoadConfig<JenkinsBuildAppConfig>(BuildAppConfigFile);
        if (config == null) Exit(BuildErrorCode.ConfigParseFailed);
        AssetBundleEditor.SynchronizedVersion(config.Version);
        if (!CheckAndSwitchPlatform(config.Platform)) Exit(BuildErrorCode.PlatformSwitchFailed);
        if (config.Platform == BuildTarget.Android && !ValidateKeystore()) Exit(BuildErrorCode.KeystoreMissing);

        // HybridCLR
        if (config.FullBuild && !RunHybridCLR()) Exit(BuildErrorCode.HybridCLRFailed);

        // YooAsset & 资源
        TryDeleteStreamingAssets();
        if (config.IncludeInitialResources && !RunYooAsset(config.Platform, null)) Exit(BuildErrorCode.YooAssetFailed);

        // Build Player
        if (!RunPlayerBuild(config)) Exit(BuildErrorCode.PlayerBuildFailed);

        DateTime endTime = DateTime.Now;
        TimeSpan duration = endTime - startTime;
        string tip = $"App 构建成功,版本号{config.Version}\n" +
                     $"开始时间:{startTime}\n" +
                     $"结束时间:{endTime}\n" +
                     $"耗时:{duration.Minutes}分{duration.Seconds}秒";
        Debug.Log("[Jenkins] {tip}");
        _ = NotifyFeiShu(tip);
        EditorApplication.Exit(0);
    }

    //Jenkins构建资源
    public static void BuildResource()
    {
        AssetDatabase.Refresh();
        var config = LoadConfig<JenkinsBuildResourceConfig>(BuildResourceConfigFile);
        if (config == null) Exit(BuildErrorCode.ConfigParseFailed);
        AssetBundleEditor.SynchronizedVersion(config.Version);
        if (!CheckAndSwitchPlatform(config.Platform)) Exit(BuildErrorCode.PlatformSwitchFailed);

        TryDeleteStreamingAssets();
        if (!RunYooAsset(config.Platform, config)) Exit(BuildErrorCode.YooAssetFailed);

        Debug.Log("[Jenkins] 资源构建成功!");
        EditorApplication.Exit(0);
    }
}

public class JenkinsBuildAppConfig
{
    public string OutputDir; //构建APP目录
    public BuildTarget Platform; //构建平台
    public bool FullBuild; //打包前先为热更生成AOT dll
    public bool IncludeInitialResources; //是否包含初始资源 (例如：是否将资源复制到 StreamingAssets)
    public string BuildType; // "Debug" 或 "Release"
    [JsonIgnore] public bool IsDebugMode => BuildType == "Debug"; // 防止被序列化到 JSON 中 
    public bool BuildAppBundle; //打Google Play aab包
    public string Version; //版本号
}

public class JenkinsBuildResourceConfig
{
    public string OutputDir; //构建资源输出目录
    public BuildTarget Platform; //构建平台
    public string Version; //版本号
    public bool ForceRebuild; //强制重新构建全部资源

    public string UpdatePrefixUrl; //热更资源服务器地址
    public bool ForceUpdate; //是否强制更新App
    public string AppUpdateUrl; //App更新地址
    public string AppUpdateDescription; //App更新说明
}

public enum BuildErrorCode
{
    [Description("构建成功")] Success = 0,
    [Description("配置文件解析失败")] ConfigParseFailed = 1,
    [Description("切换打包平台失败")] PlatformSwitchFailed = 2,
    [Description("签名文件不存在")] KeystoreMissing = 3,
    [Description("HybridCLR生成失败")] HybridCLRFailed = 4,
    [Description("YooAsset资源构建失败")] YooAssetFailed = 5,
    [Description("Unity打包失败")] PlayerBuildFailed = 6
}

public static class EnumExtensions
{
    public static string GetDescription(this Enum val)
    {
        var field = val.GetType().GetField(val.ToString());
        var attributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : val.ToString();
    }
}
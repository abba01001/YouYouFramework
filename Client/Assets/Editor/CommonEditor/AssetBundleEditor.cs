using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using HybridCLR.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using YooAsset.Editor;
using BuildReport = UnityEditor.Build.Reporting.BuildReport;
using BuildResult = UnityEditor.Build.Reporting.BuildResult;
using Debug = UnityEngine.Debug;

[CreateAssetMenu(menuName = "框架ScriptableObject/AssetBundleSettings")]
public class AssetBundleEditor : ScriptableObject
{
    public enum CusBuildTarget
    {
        Android,
        Windows,
        WebGL
    }

    public enum AssetLoadTarget
    {
        [LabelText("正式服务器")] SERVERMODE,
        [LabelText("本地服务器")] LOCALMODE
    }

    public enum PackageTarget
    {
        [LabelText("Android")] Android,
        [LabelText("iOS")] iOS,
        [LabelText("Windows")] Windows,
        [LabelText("WebGL")] WebGL
    }

    #region 打包签名
    static string keystoreRelativePath = "Assets/PackageTool/user.keystore";
    static string keystorePassword = "FrameWork";
    static string keyAlias = "key";
    static string keyPassword = "FrameWork";
    #endregion

    [PropertySpace(2f)]
    [HorizontalGroup("Common", LabelWidth = 75)]
    [VerticalGroup("Common/Left")]
    [LabelText("资源版本号")]
    [OnValueChanged(nameof(OnAssetVersionChanged))]
    public string AssetVersion;

    private void OnAssetVersionChanged()
    {
        PlayerSettings.bundleVersion = AssetVersion;
    }
    
    [PropertySpace(1)]
    [VerticalGroup("Common/Left")]
    [LabelText("资源加载方式")][OnValueChanged(nameof(OnAssetLoadTargetChanged))]
    public AssetLoadTarget CurrAssetLoadTarget;
    void OnAssetLoadTargetChanged()
    {
        string macor = string.Empty;
        macor += string.Format("{0};", CurrAssetLoadTarget.ToString());

        List<string> tempDefines = new List<string>();
        PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, out string[] hasDefines);

        for (int i = 0; i < hasDefines.Length; i++)
        {
            if(hasDefines[i] != "EDITORLOAD")
                tempDefines.Add(hasDefines[i]);
        }
        
        AssetLoadTarget[] AssetLoadTargets = (AssetLoadTarget[])Enum.GetValues(typeof(AssetLoadTarget));
        foreach (var item in AssetLoadTargets) tempDefines.Remove(item.ToString());
        
        for (int i = 0; i < tempDefines.Count; i++) macor += string.Format("{0};", tempDefines[i]);

        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, macor);
        AssetDatabase.SaveAssets();
        Debug.LogError("Sava Macro Success====" + macor);
    }

    [PropertySpace(0)]
    [VerticalGroup("Common/Left")]
    [Button("工程资源大小查看器", ButtonSizes.Medium)]
    public void PreviewProject()
    {
        AssetSizeViewer.ShowWindow();
    }
    
    [PropertySpace(1)]
    [VerticalGroup("Common/Left")]
    [Button("AB包资源预览",ButtonSizes.Medium)]
    public void PreviewAB()
    {
        YooAssetReportDiffTool.OpenWindow();
    }

    [PropertySpace(0)]
    [VerticalGroup("Common/Left")]
    [Button("启动本地服务器", ButtonSizes.Medium)]
    void StartLocalMode()
    {
        StartLocalAb();
        StartLocalServer();
    }
    
    void StartLocalAb()
    {
        if (!Directory.Exists(TempServerBundlePath))
        {
            Directory.CreateDirectory(TempServerBundlePath);
        }

        // 👉 获取本地IP
        string localIP = GetLocalIPAddress();

        // 杀掉占用 8000 端口的进程
        KillProcessOnPort(8000);

        // 启动 Python 服务器
        StartPythonServer(TempServerBundlePath);

        Debug.Log($"服务器已启动: http://{localIP}:8000/");
        Debug.Log($"服务目录: {TempServerBundlePath}");
        Debug.Log($"访问地址: http://{localIP}:8000/");
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
                    return addr.Address.ToString();
                }
            }
        }
        return "127.0.0.1";
    }

    void KillProcessOnPort(int port)
    {
        // 获取占用端口的进程 PID
        string command = $"netstat -ano | findstr :{port}";
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/C " + command,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        Process process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd(); // 获取输出结果

        process.WaitForExit();

        // 提取 PID，并结束占用端口的进程
        string[] lines = output.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            // 解析 PID (最后一列是 PID)
            string[] parts = line.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            string pid = parts[parts.Length - 1];

            // 杀掉进程
            if (int.TryParse(pid, out int pidInt))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = $"/PID {pidInt} /F", // 强制结束进程
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                UnityEngine.Debug.Log($"Killed process on port {port} with PID: {pidInt}");
            }
        }
    }

    void StartPythonServer(string directory)
    {
        // 启动 Python HTTP 服务器并指定根目录
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C python -m http.server 8000 --bind 0.0.0.0 --directory {directory}", // 指定选择的目录
            UseShellExecute = true,
            CreateNoWindow = false // 显示命令行窗口
        };

        Process process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();
        UnityEngine.Debug.Log($"Python server started on port 8000, serving directory: {directory}");
    }

    public void StartLocalServer()
    {
        // 构建服务器 exe 路径，相对 Unity 项目 Assets 目录的上两级
        string exePath = Path.Combine(Application.dataPath, "..", "..", "Server", "Publish","win-x64", "TCPServer.exe");
        exePath = Path.GetFullPath(exePath); // 转成绝对路径
        if (!File.Exists(exePath))
        {
            Debug.LogError("服务器 exe 不存在: " + exePath);
            return;
        }
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = Path.GetDirectoryName(exePath),
                UseShellExecute = true,
                Arguments = "local"
            });
            Debug.Log("本地服务器已启动: " + exePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("启动服务器失败: " + ex);
        }
    }

    [VerticalGroup("Common/Right")] [Button(ButtonSizes.Medium)][OnValueChanged(nameof(OnBuildPackageTargetChanged))]
    [LabelText("构建平台")] public PackageTarget BuildPackageTarget;

    // 🔥 修复版：自动切换Unity平台（全Unity版本兼容）
    void OnBuildPackageTargetChanged()
    {
        // 1. 获取Unity官方平台参数
        (BuildTargetGroup group, BuildTarget target) = GetUnityBuildTarget(BuildPackageTarget);

        // 2. 如果已经是当前平台，直接返回
        if (EditorUserBuildSettings.activeBuildTarget == target)
        {
            EditorUtility.DisplayDialog("提示", $"当前已是 {BuildPackageTarget} 平台", "确定");
            return;
        }

        // 3. 确认切换
        if (!EditorUtility.DisplayDialog("切换平台", $"确定要切换到 {BuildPackageTarget} 平台吗？\n切换期间Unity会卡顿，请等待完成！", "确定", "取消"))
        {
            // 切换回原来的平台值
            return;
        }

        // 4. 显示进度条 + 执行官方平台切换（同步方式，全版本稳定）
        EditorUtility.DisplayProgressBar("平台切换中", $"正在切换至 {BuildPackageTarget} 平台...", 0.5f);

        // 官方标准切换API（无返回值，稳定兼容）
        EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);

        // 5. 清理进度条 + 完成提示
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("完成", $"平台已切换为：{BuildPackageTarget}", "确定");
    }

    // 🔥 平台映射工具（你的枚举 → Unity官方平台）
    private (BuildTargetGroup group, BuildTarget target) GetUnityBuildTarget(PackageTarget target)
    {
        switch (target)
        {
            case PackageTarget.Android:
                return (BuildTargetGroup.Android, BuildTarget.Android);
            case PackageTarget.Windows:
                return (BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            case PackageTarget.iOS:
                return (BuildTargetGroup.iOS, BuildTarget.iOS);
            case PackageTarget.WebGL:
                return (BuildTargetGroup.WebGL, BuildTarget.WebGL);
            default:
                throw new System.NotImplementedException("未支持的平台");
        }
    }

    private PackageTarget GetCurrentPackageTarget()
    {
        BuildTarget currentTarget = EditorUserBuildSettings.activeBuildTarget;

        switch (currentTarget)
        {
            case BuildTarget.Android:
                return PackageTarget.Android;
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneWindows:
                return PackageTarget.Windows;
            case BuildTarget.iOS:
                return PackageTarget.iOS;
            case BuildTarget.WebGL:
                return PackageTarget.WebGL;
            default:
                return PackageTarget.Android; // 默认返回Android
        }
    }
    
    [VerticalGroup("Common/Right")]
    [Button("构建资源包",ButtonSizes.Medium)]
    public void BuildAB()
    {
        CustomYooAssetBuild.BuildInternal(BuildPackageTarget);
    }

    private static void SetKeystoreInfo()
    {
        PlayerSettings.Android.keystoreName = keystoreRelativePath;
        PlayerSettings.Android.keystorePass = keystorePassword;
        PlayerSettings.Android.keyaliasName = keyAlias;
        PlayerSettings.Android.keyaliasPass = keyPassword;
    }
    
    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        if (!Directory.Exists(destinationDir))
        {
            Directory.CreateDirectory(destinationDir);
        }
        string[] files = Directory.GetFiles(sourceDir);
        foreach (var file in files)
        {
            string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }
        string[] directories = Directory.GetDirectories(sourceDir);
        foreach (var dir in directories)
        {
            string destDir = Path.Combine(destinationDir, Path.GetFileName(dir));
            CopyDirectory(dir, destDir);
        }
    }

    [VerticalGroup("Common/Right")]
    [Button("构建安装包",ButtonSizes.Medium)]
    public void PublishAPK()
    {
        SetKeystoreInfo();

        // 1. 批量获取：打包目标 + 输出路径（合并重复switch，核心简化）
        BuildTarget buildTarget;
        string outputFullPath;
        if (!GetBuildTargetAndPath(out buildTarget, out outputFullPath))
        {
            Debug.LogError("未支持的打包平台！");
            return;
        }

        // 2. 安全清理：删除同名文件/文件夹（根治二次打包报错）
        if (Directory.Exists(outputFullPath)) Directory.Delete(outputFullPath, true);
        if (File.Exists(outputFullPath)) File.Delete(outputFullPath);

        // 3. 选择打包场景
        string[] scenes = SceneSelectionWindow.ShowWindow();
        if (scenes == null || scenes.Length == 0)
        {
            Debug.LogWarning("没有选择场景，已取消打包。");
            return;
        }

        // 4. 构建配置
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputFullPath,
            target = buildTarget,
            options = BuildOptions.CompressWithLz4
        };

        // 安卓专属设置
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        // 5. 执行打包
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        // 6. 打包结果处理
        if (summary.result == BuildResult.Succeeded)
        {
            EditorUtility.DisplayDialog("打包成功", "已成功生成！", "确定");
            // 打开输出目录
            Process.Start("explorer.exe", Path.GetDirectoryName(outputFullPath));
        }
        else
        {
            string error = $"打包失败！错误数: {summary.totalErrors}";
            Debug.LogError(error);
            EditorUtility.DisplayDialog("打包失败", error, "确定");
        }
    }

    /// <summary>
    /// 【简化核心】提取公共方法：一次性获取 打包平台 + 输出路径
    /// </summary>
    private bool GetBuildTargetAndPath(out BuildTarget buildTarget, out string outputPath)
    {
        buildTarget = BuildTarget.NoTarget;
        outputPath = string.Empty;

        // 确保输出目录存在
        if (!Directory.Exists(TempAPKPath))
            Directory.CreateDirectory(TempAPKPath);

        switch (BuildPackageTarget)
        {
            case PackageTarget.Android:
                buildTarget = BuildTarget.Android;
                outputPath = Path.Combine(TempAPKPath, $"{AssetVersion}.apk");
                return true;

            case PackageTarget.Windows:
                buildTarget = BuildTarget.StandaloneWindows64;
                outputPath = Path.Combine(TempAPKPath, $"{AssetVersion}.exe");
                return true;

            case PackageTarget.WebGL:
                buildTarget = BuildTarget.WebGL;
                outputPath = Path.Combine(TempAPKPath, AssetVersion);
                return true;

            default:
                return false;
        }
    }
    
    [VerticalGroup("Common/Right")]
    [Button("构建Gradle工程",ButtonSizes.Medium)]
    public void ExportGradleProject()
    {
        // 设置Keystore信息（如果需要的话）
        SetKeystoreInfo();

        if (!Directory.Exists(TempGradlePath))
        {
            Directory.CreateDirectory(TempGradlePath);
        }
        
        // 配置导出Gradle项目
        string[] scenes = { "Assets/Game/Scene_Launch.unity" }; // 根据实际场景选择
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes, // 使用当前编辑器中的所有场景
            locationPathName = TempGradlePath, // 用户选择的导出路径
            target = BuildTarget.Android, // 构建目标平台
            options = BuildOptions.None // 允许外部修改，开发模式等
        };

        // 导出Gradle项目
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        // 导出结果反馈
        if (summary.result == BuildResult.Succeeded)
        {
            EditorUtility.DisplayDialog("导出成功", "Gradle 项目已成功导出！", "确定");

            // 打开导出目录
            // 获取目标文件夹路径
            // string folderPath = Path.GetDirectoryName(outputFullPath);
            ExplorerUtil.OpenFolder(TempGradlePath);
            // Process.Start("explorer.exe", TempGradlePath);
        }
        else if (summary.result == BuildResult.Failed)
        {
            string errorMessage = "导出失败！\n错误信息: " + summary.totalErrors;
            Debug.LogError(errorMessage);
            EditorUtility.DisplayDialog("导出失败", errorMessage, "确定");
        }
    }

    [OnInspectorInit]
    private void Init()
    {
        AssetVersion = Application.version;
        BuildPackageTarget = GetCurrentPackageTarget();
        string m_Macor = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (!string.IsNullOrEmpty(m_Macor))
        {
            //该字符串包含AssetLoadTargets[i]
            AssetLoadTarget[] AssetLoadTargets = (AssetLoadTarget[])Enum.GetValues(typeof(AssetLoadTarget));
            for (int i = 0; i < AssetLoadTargets.Length; i++)
            {
                if (m_Macor.IndexOf(AssetLoadTargets[i].ToString()) != -1)
                {
                    CurrAssetLoadTarget = AssetLoadTargets[i];
                }
            }
        }
    }
    
    [MenuItem("工具类/Other/Build Android Launch",false,99800)]
    public static void BuildAndroidLaunchScene()
    {
        SetKeystoreInfo();
    
        // 路径确保与你项目一致
        string[] scenes = { "Assets/Game/Scene_Launch.unity" }; 

        string projectPath = Path.GetDirectoryName(UnityEngine.Application.dataPath);
        string outputDir = Path.Combine(projectPath, "Builds/Android");
        string outputFile = Path.Combine(outputDir, "Game_Launch.apk");

        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputFile,
            target = BuildTarget.Android,
            options = BuildOptions.CompressWithLz4
        };

        UnityEngine.Debug.Log("[Build] 正在通过命令行/菜单开始构建...");
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            UnityEngine.Debug.Log("[Build] 任务成功!");
            // 如果是命令行模式(BatchMode)，执行完就退出，否则 bat 会一直等
            if (UnityEngine.Application.isBatchMode) EditorApplication.Exit(0);
            else EditorUtility.DisplayDialog("打包结果", "构建成功！", "确定");
        }
        else
        {
            UnityEngine.Debug.LogError("[Build] 任务失败!");
            if (UnityEngine.Application.isBatchMode) EditorApplication.Exit(1);
            else EditorUtility.DisplayDialog("打包结果", "构建失败，请看 Console", "确定");
        }
    }
    
    [LabelText("导出Gradle工程路径")]
    [FolderPath]
    public string GradleSavePath;
    private string TempGradlePath => Application.dataPath + "/../" + GradleSavePath;
    
    [LabelText("输出APK包路径")]
    [FolderPath]
    public string PulishAPKPath;
    private string TempAPKPath => Application.dataPath + "/../" + PulishAPKPath;
    
    [LabelText("本地AB包资源存储IP")]
    [FolderPath]
    public string ServerBundlePath;
    private string TempServerBundlePath => Application.dataPath + "/../" + ServerBundlePath;
}

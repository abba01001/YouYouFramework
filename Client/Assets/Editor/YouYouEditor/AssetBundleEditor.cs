using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

[CreateAssetMenu(menuName = "YouYouAsset/AssetBundleSettings")]
public class AssetBundleEditor : ScriptableObject
{
    public enum CusBuildTarget
    {
        Windows,
        Android,
        IOS,
        WebGL
    }

    #region 打包签名
    string keystoreRelativePath = "Assets/PackageTool/user.keystore";
    string keystorePassword = "FrameWork";
    string keyAlias = "key";
    string keyPassword = "FrameWork";
    #endregion

    [PropertySpace(0.5f)]
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
    [Button("AB包资源预览",ButtonSizes.Medium)]
    public void Test()
    {
    }

    [PropertySpace(0)]
    [VerticalGroup("Common/Left")]
    [Button("启动本地AB包资源存储", ButtonSizes.Medium)]
    void StartLocalServer()
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

    [PropertySpace(0)]
    [VerticalGroup("Common/Left")]
    [Button("启动本地服务器", ButtonSizes.Medium)]
    public void StartServer()
    {
        // 构建服务器 exe 路径，相对 Unity 项目 Assets 目录的上两级
        string exePath = Path.Combine(Application.dataPath, "..", "..", "Server", "Server", "TCPServer.exe");
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
                UseShellExecute = true
            });
            Debug.Log("本地服务器已启动: " + exePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("启动服务器失败: " + ex);
        }
    }
    
    [VerticalGroup("Common/Right")]
    [Button("清理AB资源包缓存",ButtonSizes.Medium)]
    public void ClearAB()
    {
        string tempBundlePath = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
        string localServerBundlePath = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot2();
        
        foreach (var path in new List<string>(){tempBundlePath,localServerBundlePath})
        {
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);  // 递归删除
                    AssetDatabase.Refresh();
                    Debug.Log($"{path}资源清理完成！");
                }
                catch (Exception e)
                {
                    Debug.LogError($"删除AssetBundles文件夹时发生异常: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("AssetBundles 文件夹不存在！");
            }
        }
    }
    
    [VerticalGroup("Common/Right")]
    [Button("构建AB资源包",ButtonSizes.Medium)]
    public void BuildAB()
    {
        CustomYooAssetBuild.BuildInternal();
    }

    private void SetKeystoreInfo()
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
    [Button("输出安装包",ButtonSizes.Medium)]
    public void PublishAPK()
    {
        SetKeystoreInfo();
        if (!Directory.Exists(TempAPKPath))
        {
            Directory.CreateDirectory(TempAPKPath);
        }

        var path = TempAPKPath + $"/{AssetVersion}.apk";
        if (File.Exists(path))  // 用 File.Exists 检查文件
        {
            File.Delete(path);
        }
        Directory.CreateDirectory(path);
        
        string[] scenes = SceneSelectionWindow.ShowWindow();
        if (scenes == null || scenes.Length == 0)
        {
            Debug.LogWarning("没有选择场景，已取消打包。");
            return;
        }
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = path,
            target = BuildTarget.Android,
            options = BuildOptions.CompressWithLz4
        };
        // COSUploader.UploadVersion(AssetVersion);
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false; 
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            EditorUtility.DisplayDialog("打包成功", "APK 已成功生成！", "确定");
            // if (IsUploadAPK) COSUploader.UploadAPK(path);
            string directoryPath = Path.GetDirectoryName(path); // 获取文件夹路径
            Process.Start("explorer.exe", directoryPath);
        }
        if (summary.result == BuildResult.Failed)
        {
            string errorMessage = "打包失败！\n错误信息: " + summary.totalErrors;
            Debug.LogError(errorMessage);
            EditorUtility.DisplayDialog("打包失败", errorMessage, "确定");
        }
    }
    
    [VerticalGroup("Common/Right")]
    [Button("导出Gradle工程",ButtonSizes.Medium)]
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
            Process.Start("explorer.exe", TempGradlePath);
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

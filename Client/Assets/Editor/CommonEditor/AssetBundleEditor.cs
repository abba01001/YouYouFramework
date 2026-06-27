using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

[CreateAssetMenu(menuName = "框架ScriptableObject/AssetBundleSettings")]
public class AssetBundleEditor : ScriptableObject
{
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
    private const string KeystoreRelativePath = "Tools/user.keystore";
    private const string KeystorePassword = "FrameWork";
    private const string KeyAlias = "key";
    private const string KeyPassword = "FrameWork";
    #endregion

    [PropertySpace(2f)]
    [HorizontalGroup("Common", LabelWidth = 75)]
    [VerticalGroup("Common/Left")]
    [LabelText("资源版本号")]
    public string AssetVersion;
    
    [PropertySpace(1f)]
    [VerticalGroup("Common/Left")]
    [LabelText("资源加载方式")]
    [OnValueChanged(nameof(OnAssetLoadTargetChanged))]
    public AssetLoadTarget CurrAssetLoadTarget;

    private void OnAssetLoadTargetChanged()
    {
        BuildTargetGroup currentGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        PlayerSettings.GetScriptingDefineSymbolsForGroup(currentGroup, out string[] currentDefines);

        HashSet<string> defineSet = new HashSet<string>(currentDefines);
        defineSet.Remove("EDITORLOAD");
        foreach (AssetLoadTarget target in Enum.GetValues(typeof(AssetLoadTarget)))
        {
            defineSet.Remove(target.ToString());
        }

        defineSet.Add(CurrAssetLoadTarget.ToString());
        string macroResult = string.Join(";", defineSet);

        PlayerSettings.SetScriptingDefineSymbolsForGroup(currentGroup, macroResult);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Macro] 保存宏成功: {macroResult}");
    }
    
    [PropertySpace(1f)]
    [VerticalGroup("Common/Left")]
    [Button("AB包资源预览", ButtonSizes.Medium)]
    public void PreviewAB() => YooAssetReportDiffTool.OpenWindow();

    [PropertySpace(1f)]
    [VerticalGroup("Common/Left")]
    [Button("启动本地服务器", ButtonSizes.Medium)]
    private void StartLocalMode()
    {
        StartLocalAb();
        StartLocalServer();
    }
    
    private void StartLocalAb()
    {
        string fullPath = Path.GetFullPath(TempServerBundlePath);
        EnsureDirectoryExists(fullPath);
        
        EditorProcessUtil.KillProcessOnPort(8000);
        EditorProcessUtil.StartNodeServer(8000, fullPath);
        Debug.Log($"[AB服务器] 启动成功: http://{GetLocalIPAddress()}:8000/");
    }
    
    private string GetLocalIPAddress()
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces();
        for (int i = 0; i < interfaces.Length; i++)
        {
            if (interfaces[i].OperationalStatus != OperationalStatus.Up) continue;

            var unicast = interfaces[i].GetIPProperties().UnicastAddresses;
            for (int j = 0; j < unicast.Count; j++)
            {
                var addr = unicast[j].Address;
                if (addr.AddressFamily == AddressFamily.InterNetwork && !addr.ToString().StartsWith("127"))
                {
                    return addr.ToString();
                }
            }
        }
        return "127.0.0.1";
    }

    private void StartLocalServer()
    {
        string exePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "Server","TCPServer", "Publish", "win-x64", "TCPServer.exe"));
        if (File.Exists(exePath)) EditorProcessUtil.StartExecutable(exePath, "local");
        else Debug.LogError($"未找到服务器可执行文件: {exePath}");
    }

    [VerticalGroup("Common/Right")] 
    [Button(ButtonSizes.Medium)]
    [OnValueChanged(nameof(OnBuildPackageTargetChanged))]
    [LabelText("构建平台")] 
    public PackageTarget BuildPackageTarget;

    private void OnBuildPackageTargetChanged()
    {
        EditorApplication.delayCall += () =>
        {
            (BuildTargetGroup group, BuildTarget target) = GetUnityBuildTarget(BuildPackageTarget);

            if (EditorUserBuildSettings.activeBuildTarget == target)
            {
                EditorUtility.DisplayDialog("提示", $"当前已是 {BuildPackageTarget} 平台", "确定");
                return;
            }

            if (!EditorUtility.DisplayDialog("切换平台", $"确定要切换到 {BuildPackageTarget} 平台吗？\n切换期间Unity会卡顿，请等待完成！", "确定",
                    "取消"))
            {
                BuildPackageTarget = GetCurrentPackageTarget();
                return;
            }

            EditorUtility.DisplayProgressBar("平台切换中", $"正在切换至 {BuildPackageTarget} 平台...", 0.5f);
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("完成", $"平台已切换为：{BuildPackageTarget}", "确定");


            EditorUtility.DisplayProgressBar("代码生成中", "正在执行 HybridCLR GenerateAll...", 0.8f);
            try
            {
                HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
                // 只有成功才弹出完成提示
                EditorUtility.DisplayDialog("成功", "HybridCLR 代码生成已完成。", "确定");
            }
            catch (System.Exception e)
            {
                // 捕获异常，将错误详细信息展示给用户
                Debug.LogError($"HybridCLR GenerateAll 失败: {e.Message}\n{e.StackTrace}");
                EditorUtility.DisplayDialog("失败", $"HybridCLR 代码生成失败，请检查控制台日志：\n{e.Message}", "确定");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        };
    }

    private (BuildTargetGroup group, BuildTarget target) GetUnityBuildTarget(PackageTarget target)
    {
        return target switch
        {
            PackageTarget.Android => (BuildTargetGroup.Android, BuildTarget.Android),
            PackageTarget.Windows => (BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64),
            PackageTarget.iOS => (BuildTargetGroup.iOS, BuildTarget.iOS),
            PackageTarget.WebGL => (BuildTargetGroup.WebGL, BuildTarget.WebGL),
            _ => throw new NotImplementedException("未支持的平台")
        };
    }

    private PackageTarget GetCurrentPackageTarget()
    {
        return EditorUserBuildSettings.activeBuildTarget switch
        {
            BuildTarget.Android => PackageTarget.Android,
            BuildTarget.StandaloneWindows64 or BuildTarget.StandaloneWindows => PackageTarget.Windows,
            BuildTarget.iOS => PackageTarget.iOS,
            BuildTarget.WebGL => PackageTarget.WebGL,
            _ => PackageTarget.Android
        };
    }
    
    [VerticalGroup("Common/Right")]
    [Button("构建资源包", ButtonSizes.Medium)]
    public void BuildAB()
    {
        PlayerSettings.bundleVersion = AssetVersion;
        CustomYooAssetBuild.BuildInternal(BuildPackageTarget);
    }

    public static void SynchronizedVersion(string version)
    {
        PlayerSettings.bundleVersion = version;
    }

    private static void SetKeystoreInfo()
    {
        PlayerSettings.Android.keystoreName = KeystoreRelativePath;
        PlayerSettings.Android.keystorePass = KeystorePassword;
        PlayerSettings.Android.keyaliasName = KeyAlias;
        PlayerSettings.Android.keyaliasPass = KeyPassword;
    }

    [VerticalGroup("Common/Right")]
    [Button("构建安装包", ButtonSizes.Medium)]
    public void PublishAPK()
    {
        PlayerSettings.bundleVersion = AssetVersion;
        SetKeystoreInfo();
        if (!GetBuildTargetAndPath(out BuildTarget buildTarget, out string outputFullPath, out string platformFolder))
        {
            Debug.LogError("未支持的打包平台！");
            return;
        }

        // 确保平台专属的文件夹存在
        EnsureDirectoryExists(platformFolder);

        // 清理旧的同名文件/文件夹，防止覆写失败
        if (Directory.Exists(outputFullPath)) Directory.Delete(outputFullPath, true);
        if (File.Exists(outputFullPath)) File.Delete(outputFullPath);

        string[] scenes = SceneSelectionWindow.ShowWindow();
        if (scenes == null || scenes.Length == 0) return;

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputFullPath,
            target = buildTarget,
            options = BuildOptions.CompressWithLz4
        };

        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        
        // 【优化】：执行完后，自动打开精准分类的平台子文件夹 (platformFolder)
        ExecuteBuild(options, $"{BuildPackageTarget} 安装包打包成功！", platformFolder);
    }

    /// <summary>
    /// 【优化】：获取构建目标、输出完整路径、以及平台专属文件夹路径
    /// </summary>
    private bool GetBuildTargetAndPath(out BuildTarget buildTarget, out string outputFullPath, out string platformFolder)
    {
        buildTarget = BuildTarget.NoTarget;
        outputFullPath = string.Empty;
        platformFolder = string.Empty;

        // 根据构建平台，动态追加子文件夹名字
        switch (BuildPackageTarget)
        {
            case PackageTarget.Android:
                buildTarget = BuildTarget.Android;
                platformFolder = Path.Combine(TempAPKPath, "Android");
                outputFullPath = Path.Combine(platformFolder, $"{Application.version}.apk");
                return true;
                
            case PackageTarget.Windows:
                buildTarget = BuildTarget.StandaloneWindows64;
                platformFolder = Path.Combine(TempAPKPath, "Windows");
                outputFullPath = Path.Combine(platformFolder, $"{Application.version}.exe");
                return true;
                
            case PackageTarget.WebGL:
                buildTarget = BuildTarget.WebGL;
                platformFolder = Path.Combine(TempAPKPath, "WebGL");
                // WebGL 是个文件夹目录，通常直接把版本号作为文件夹名丢在 WebGL 目录下
                outputFullPath = Path.Combine(platformFolder, Application.version); 
                return true;
                
            case PackageTarget.iOS:
                buildTarget = BuildTarget.iOS;
                platformFolder = Path.Combine(TempAPKPath, "iOS");
                outputFullPath = Path.Combine(platformFolder, Application.version);
                return true;
                
            default:
                return false;
        }
    }
    
    [VerticalGroup("Common/Right")]
    [Button("构建Gradle工程", ButtonSizes.Medium)]
    public void ExportGradleProject()
    {
        SetKeystoreInfo();
        EnsureDirectoryExists(TempGradlePath);
        
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Game/Scene_Launch.unity" },
            locationPathName = TempGradlePath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        ExecuteBuild(options, "Gradle 项目已成功导出！", TempGradlePath);
    }
    
    [VerticalGroup("Common/Right")]
    [Button("构建Xcode工程", ButtonSizes.Medium)]
    public void ExportXcodeProject()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
        {
            EditorUtility.DisplayDialog("提示", "当前不是 iOS 平台，请先切换 Build Target 为 iOS！", "确定");
            return;
        }
        
        EnsureDirectoryExists(TempXcodePath);
    
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Game/Scene_Launch.unity" }, 
            locationPathName = TempXcodePath, 
            target = BuildTarget.iOS,         
            options = BuildOptions.CompressWithLz4 
        };

        Debug.Log("<color=#00FFFF>开始构建 Xcode 工程...</color>");
        ExecuteBuild(options, "Xcode 项目已成功导出！\n请将该文件夹拷贝至 Mac 进行最终编译。", TempXcodePath);
    }

    #region 提炼出的通用核心底层方法
    /// <summary>
    /// 提取通用打包管线执行逻辑
    /// </summary>
    private void ExecuteBuild(BuildPlayerOptions options, string successMessage, string openFolderPath)
    {
        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            if (EditorUtility.DisplayDialog("构建成功", successMessage, "确定"))
            {
                if (!string.IsNullOrEmpty(openFolderPath))
                {
                    EditorProcessUtil.OpenFolderSmart(openFolderPath);
                }
            }
        }
        else
        {
            string errorMessage = $"构建失败！平台: {options.target}, 错误总数: {report.summary.totalErrors}";
            Debug.LogError(errorMessage);
            EditorUtility.DisplayDialog("构建失败", errorMessage, "确定");
        }
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
    #endregion

    [OnInspectorInit]
    private void Init()
    {
        AssetVersion = Application.version;
        BuildPackageTarget = GetCurrentPackageTarget();

        string macroStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (!string.IsNullOrEmpty(macroStr))
        {
            foreach (AssetLoadTarget target in Enum.GetValues(typeof(AssetLoadTarget)))
            {
                if (macroStr.Contains(target.ToString()))
                {
                    CurrAssetLoadTarget = target;
                    break; 
                }
            }
        }
    }
    
    [LabelText("导出Gradle工程路径")] [FolderPath] public string GradleSavePath;
    private string TempGradlePath => Path.Combine(Application.dataPath, "..", GradleSavePath);
    
    [LabelText("导出Xcode工程路径")] [FolderPath] public string XcodeSavePath;
    private string TempXcodePath => Path.Combine(Application.dataPath, "..", XcodeSavePath);
    
    [LabelText("输出APK包路径")] [FolderPath] public string PulishAPKPath;
    private string TempAPKPath => Path.Combine(Application.dataPath, "..", PulishAPKPath);
    
    [LabelText("本地AB包资源存储IP")] [FolderPath] public string ServerBundlePath;
    private string TempServerBundlePath => Path.Combine(Application.dataPath, "..", ServerBundlePath);
}
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;

public static class JenkinsBuildScript
{
    private const string KeystoreRelativePath = "Assets/PackageTool/user.keystore";
    private const string KeystorePassword = "FrameWork";
    private const string KeyAlias = "key";
    private const string KeyPassword = "FrameWork";

    public static void PerformBuild()
    {
        Debug.Log("[Jenkins] 开始解析命令行参数...");

        // 1. 解析来自 Jenkins 的命令行参数
        Dictionary<string, string> args = GetCommandLineArgs();
        
        string targetPlatform = args.GetValueOrDefault("-buildTarget", "Android");
        bool enableHybridCLR = args.GetValueOrDefault("-hybridCLR", "true").ToLower() == "true";
        string buildType = args.GetValueOrDefault("-buildType", "Release");
        
        // 【新增参数】：是否执行全量重构（即执行 HybridCLR 的 Generate All）
        bool isFullBuild = args.GetValueOrDefault("-fullBuild", "false").ToLower() == "true";

        Debug.Log($"[Jenkins] 平台: {targetPlatform}, 热更开启: {enableHybridCLR}, 模式: {buildType}, 全量重构: {isFullBuild}");

        // 2. 映射打包平台用于最终构建安装包
        BuildTarget buildTarget = targetPlatform switch
        {
            "Android" => BuildTarget.Android,
            "WebGL" => BuildTarget.WebGL,
            "iOS" => BuildTarget.iOS,
            "Windows" => BuildTarget.StandaloneWindows64,
            _ => BuildTarget.StandaloneWindows64
        };

        // 3. 【前置步骤 1】：如果是 Android 平台，配置签名证书
        if (buildTarget == BuildTarget.Android)
        {
            Debug.Log("[Jenkins] [前置步骤] 检测到 Android 平台，开始配置 Keystore 签名证书...");
            SetKeystoreInfo();
        }

        // 4. 【核心新增逻辑】：如果开启了 fullBuild，优先执行 HybridCLR 的 Generate All
        if (enableHybridCLR && isFullBuild)
        {
            Debug.Log("[Jenkins] [前置步骤] 检测到 -fullBuild 为 true，开始生成 HybridCLR 所有依赖项 (GenerateAll)...");
            try
            {
                // 切换到当前打包的目标平台上下文，确保生成的数据与平台对齐
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(buildTarget), buildTarget);
                
                // 直接调用 HybridCLR 官方的生成全部方法
                HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
                
                Debug.Log("[Jenkins] HybridCLR GenerateAll 执行成功！");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Jenkins] HybridCLR GenerateAll 执行失败: {e.Message}\n{e.StackTrace}");
                EditorApplication.Exit(1);
                return;
            }
        }

        // 5. 【前置步骤 3】：执行 YooAsset 资源包构建（内部包含编译并拷贝最新热更 DLL）
        if (enableHybridCLR)
        {
            Debug.Log("[Jenkins] [前置步骤] 开始构建 YooAsset 资源包并处理热更 DLL...");
            
            AssetBundleEditor.PackageTarget packageTarget = targetPlatform switch
            {
                "Android" => AssetBundleEditor.PackageTarget.Android,
                "WebGL" => AssetBundleEditor.PackageTarget.WebGL,
                "iOS" => AssetBundleEditor.PackageTarget.iOS,
                "Windows" => AssetBundleEditor.PackageTarget.Windows,
                _ => AssetBundleEditor.PackageTarget.Android
            };

            string streamingAssetsPath = Application.streamingAssetsPath;
            if (Directory.Exists(streamingAssetsPath))
            {
                Directory.Delete(streamingAssetsPath, true);
                Directory.CreateDirectory(streamingAssetsPath);
                AssetDatabase.Refresh();
            }
            bool assetBuildSuccess = CustomYooAssetBuild.BuildInternal(packageTarget);
            
            if (!assetBuildSuccess)
            {
                Debug.LogError("[Jenkins] YooAsset 资源包构建失败，中断安装包打包流程！");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log("[Jenkins] YooAsset 资源包构建并拷贝热更 DLL 成功，继续执行安装包构建...");
        }

        // 6. 映射安装包导出路径
        string platformFolder = Path.Combine(Environment.CurrentDirectory, $"../Builds/{targetPlatform}/{buildType}");
        string outputFullPath = buildTarget == BuildTarget.Android ? 
            Path.Combine(platformFolder, "MonsterSurvivors.apk") : platformFolder;

        if (!Directory.Exists(platformFolder)) Directory.CreateDirectory(platformFolder);
        if (Directory.Exists(outputFullPath)) Directory.Delete(outputFullPath, true);
        if (File.Exists(outputFullPath)) File.Delete(outputFullPath);

        // 7. 获取场景列表
        string[] scenes = GetProjectScenes();
        if (scenes == null || scenes.Length == 0)
        {
            Debug.LogError("[Jenkins] 未找到任何有效场景，打包中断！");
            EditorApplication.Exit(1);
            return;
        }

        // 8. 配置安装包打包参数
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputFullPath,
            target = buildTarget,
            options = BuildOptions.CompressWithLz4 | (buildType == "Debug" ? BuildOptions.Development : BuildOptions.None)
        };

        if (buildTarget == BuildTarget.Android)
        {
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        }

        // 9. 正式执行安装包生成管线
        Debug.Log($"[Jenkins] 开始构建主安装包，导出路径: {outputFullPath}");
        BuildReport report = BuildPipeline.BuildPlayer(options);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[Jenkins] {targetPlatform} 安装包打包成功！");
            EditorApplication.Exit(0);
        }
        else
        {
            string errorMessage = $"[Jenkins] 构建失败！平台: {options.target}, 错误总数: {report.summary.totalErrors}";
            Debug.LogError(errorMessage);
            EditorApplication.Exit(1);
        }
    }

    private static void SetKeystoreInfo()
    {
        PlayerSettings.Android.keystoreName = KeystoreRelativePath;
        PlayerSettings.Android.keystorePass = KeystorePassword;
        PlayerSettings.Android.keyaliasName = KeyAlias;
        PlayerSettings.Android.keyaliasPass = KeyPassword;
        Debug.Log($"[Jenkins] 签名配置成功: {PlayerSettings.Android.keystoreName}");
    }

    private static string[] GetProjectScenes()
    {
        string targetScene = "Assets/Game/Scene_Launch.unity"; 
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(targetScene);
        if (sceneAsset != null)
        {
            Debug.Log($"[Jenkins] 限制首包场景，仅打包指定场景: {targetScene}");
            return new string[] { targetScene };
        }
        Debug.LogError($"[Jenkins] 未找到指定的首场景文件: {targetScene}，降级使用默认激活列表！");
        List<string> scenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled) scenes.Add(scene.path);
        }
        return scenes.ToArray();
    }

    private static Dictionary<string, string> GetCommandLineArgs()
    {
        var argDict = new Dictionary<string, string>();
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("-") && i + 1 < args.Length && !args[i + 1].StartsWith("-"))
            {
                argDict[args[i]] = args[i + 1];
            }
        }
        return argDict;
    }
}
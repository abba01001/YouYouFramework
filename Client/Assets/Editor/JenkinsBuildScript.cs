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
        Debug.Log("[Jenkins] 开始执行自动化构建流程...");

        var args = GetCommandLineArgs();
        string targetPlatform = args.GetValueOrDefault("-buildTarget", "Android");
        bool enableHybridCLR = args.GetValueOrDefault("-hybridCLR", "true").ToLower() == "true";
        string buildType = args.GetValueOrDefault("-buildType", "Release");
        bool isFullBuild = args.GetValueOrDefault("-fullBuild", "false").ToLower() == "true";

        // 1. 平台校验
        BuildTarget buildTarget = GetBuildTarget(targetPlatform);
        Debug.Log($"[Jenkins] 配置检查: 平台={targetPlatform}, 模式={buildType}, 全量={isFullBuild}");

        // 2. Android 签名配置
        if (buildTarget == BuildTarget.Android)
        {
            string absKeystorePath = Path.Combine(Environment.CurrentDirectory, KeystoreRelativePath);
            if (!File.Exists(absKeystorePath))
            {
                Debug.LogError($"[Jenkins] 错误: 签名文件不存在 -> {absKeystorePath}");
                EditorApplication.Exit(1);
                return;
            }
            SetKeystoreInfo();
        }

        // 3. HybridCLR 生成
        if (enableHybridCLR && isFullBuild)
        {
            try {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(buildTarget), buildTarget);
                HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
                Debug.Log("[Jenkins] HybridCLR GenerateAll 完成");
            } catch (Exception e) {
                Debug.LogError($"[Jenkins] HybridCLR 严重错误: {e.Message}");
                EditorApplication.Exit(1);
                return;
            }
        }

        // 4. YooAsset 构建
        if (enableHybridCLR)
        {
            string saPath = Application.streamingAssetsPath;
            if (Directory.Exists(saPath)) {
                try { Directory.Delete(saPath, true); } catch { /* 忽略删除异常 */ }
            }
            Directory.CreateDirectory(saPath);

            var yooTarget = (targetPlatform == "Android") ? AssetBundleEditor.PackageTarget.Android : AssetBundleEditor.PackageTarget.Windows;
            if (!CustomYooAssetBuild.BuildInternal(yooTarget))
            {
                Debug.LogError("[Jenkins] YooAsset 构建内部函数返回失败，流程终止");
                EditorApplication.Exit(1);
                return;
            }
        }

        // 5. 准备输出目录
        string platformFolder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $"../Builds/{targetPlatform}/{buildType}"));
        string outputFullPath = (buildTarget == BuildTarget.Android) ? Path.Combine(platformFolder, "MonsterSurvivors.apk") : platformFolder;

        if (!Directory.Exists(platformFolder)) Directory.CreateDirectory(platformFolder);
        
        Debug.Log($"[Jenkins] 构建输出路径: {outputFullPath}");

        // 6. 执行构建
        BuildPlayerOptions options = new BuildPlayerOptions {
            scenes = GetProjectScenes(),
            locationPathName = outputFullPath,
            target = buildTarget,
            options = BuildOptions.CompressWithLz4 | (buildType == "Debug" ? BuildOptions.Development : BuildOptions.None)
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        
        // 7. 处理构建结果 (Jenkins 核心逻辑)
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[Jenkins] 构建成功! 耗时: {report.summary.totalTime.TotalSeconds:F0}秒");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[Jenkins] 构建失败! 错误数: {report.summary.totalErrors}");
            EditorApplication.Exit(1);
        }
    }

    private static BuildTarget GetBuildTarget(string platform) => platform switch
    {
        "Android" => BuildTarget.Android,
        "iOS" => BuildTarget.iOS,
        _ => BuildTarget.StandaloneWindows64
    };

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

    private static Dictionary<string, string> GetCommandLineArgs()
    {
        var argDict = new Dictionary<string, string>();
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("-") && i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                argDict[args[i]] = args[i + 1];
        }
        return argDict;
    }
}
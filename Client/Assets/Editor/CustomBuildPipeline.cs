using System;
using System.IO;
using System.Text;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

public class MyScriptableBuildParameters : ScriptableBuildParameters
{
    public override string GetPackageOutputDirectory()
    {
        var dir = $"{BuildOutputRoot}/{BuildTarget}/{PackageName}/{Application.version}";
        return dir;
    }
}

public class CustomYooAssetBuild
{
    private static string PackageName = "DefaultPackage";
    public static void CopyHofixDll(BuildTarget buildTarget)
    {
        CompileDllCommand.CompileDll(buildTarget);
        UnityEngine.Debug.Log($"脚本编译 完成!!!");
    
        string CodeDir = "Assets/Game/Download/Hotfix/";
        string gameScriptsDir = Application.dataPath + "/../" + "HybridCLRData/HotUpdateDlls/" + buildTarget.ToString() + "/GameScripts.dll";
        File.Copy(gameScriptsDir, Path.Combine(CodeDir, "GameScripts.dll.bytes"), true);
        string aotMetaAssemblyDir = Application.dataPath + "/../" + "HybridCLRData/AssembliesPostIl2CppStrip/" + buildTarget.ToString() + "/";
        foreach (var aotDllName in AOTGenericReferences.PatchedAOTAssemblyList)
        {
            File.Copy(aotMetaAssemblyDir + aotDllName, Path.Combine(CodeDir, aotDllName + ".bytes"), true);
        }
    
        AssetDatabase.Refresh();
        Debug.Log("热更dll和补充元数据dll, 复制到Game/Download/Hotfix完成");
    }
    
    public static bool BuildInternal(AssetBundleEditor.PackageTarget packageTarget)
    {
        BuildTarget buildTarget = default;
        switch (packageTarget)
        {
            case AssetBundleEditor.PackageTarget.WebGL:
                buildTarget = BuildTarget.WebGL;
                break;
            case AssetBundleEditor.PackageTarget.Android:
                buildTarget = BuildTarget.Android;
                break;
            case AssetBundleEditor.PackageTarget.Windows:
                buildTarget = BuildTarget.StandaloneWindows64;
                break;
        }
        CopyHofixDll(buildTarget);
        return BuildInternal(buildTarget);
    }

    private static bool BuildInternal(BuildTarget buildTarget)
    {
        UnityEngine.Debug.Log($"开始构建 : ");
        var buildoutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
        var streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();

        DateTime currentTime = DateTime.Now;
        string formattedTime = currentTime.ToString("yyyy.MM.dd.HH.mm.ss");

        // 构建参数
        MyScriptableBuildParameters buildParameters = new MyScriptableBuildParameters();
        // BuiltinBuildParameters buildParameters = new BuiltinBuildParameters();
        buildParameters.BuildOutputRoot = Path.Combine(buildoutputRoot, formattedTime);
        buildParameters.BuildinFileRoot = streamingAssetsRoot;
        buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString();
        // buildParameters.BuildPipeline = EBuildPipeline.BuiltinBuildPipeline.ToString();
        buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
        buildParameters.BuildTarget = buildTarget;
        buildParameters.PackageName = PackageName;
        buildParameters.PackageVersion = $"{Application.version}_{formattedTime}";
        buildParameters.VerifyBuildingResult = true;
        buildParameters.FileNameStyle = EFileNameStyle.BundleName_HashName;
        buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;
        buildParameters.BuildinFileCopyParams = string.Empty;
        // buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyByTags;
        // buildParameters.BuildinFileCopyParams = "first";

        buildParameters.BuiltinShadersBundleName = GetBuiltinShaderBundleName();
        // buildParameters.EncryptionServices = CreateEncryptionInstance();
        buildParameters.CompressOption = ECompressOption.LZ4;
        buildParameters.ClearBuildCacheFiles = false; //不清理构建缓存，启用增量构建，可以提高打包速度！
        buildParameters.UseAssetDependencyDB = true; //使用资源依赖关系数据库，可以提高打包速度！
        buildParameters.TrackSpriteAtlasDependencies = true; //自动建立资源对象对图集的依赖关系

        // 执行构建
        // BuiltinBuildPipeline pipeline = new BuiltinBuildPipeline();
        ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
        var buildResult = pipeline.Run(buildParameters, true);
        if (buildResult.Success)
        {
            UnityEngine.Debug.Log($"构建成功 : {buildResult.OutputPackageDirectory}");
            WriteVersionFile(buildParameters.PackageVersion,buildTarget);
            CopyAssetsToTarget(AssetBundleBuilderHelper.GetDefaultBuildOutputRoot(), buildTarget, buildResult);
            CopyAssetsToTarget(AssetBundleBuilderHelper.GetDefaultBuildOutputRoot2(), buildTarget, buildResult);
            //服务器用 end
            return true;
        }
        else
        {
            UnityEngine.Debug.LogError($"构建失败 : {buildResult.ErrorInfo}");
        }
        return false;
    }

    private static void WriteVersionFile(string packageVersion,BuildTarget buildTarget)
    {
        // 1. 获取输出根目录
        // 注意：确保该目录在构建开始后已经存在
        string outputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot2();
        // 2. 拼接文件完整路径
        string filePath = $"{outputRoot}/{buildTarget}/Version.txt";
        try
        {
            // 3. 写入版本字符串 (例如: 1.3.0_2026.04.16.16.36.20)
            // 使用 UTF8 无 BOM 格式，确保移动端读取兼容性
            File.WriteAllText(filePath, packageVersion, new UTF8Encoding(false));
            
            Debug.Log($"<color=#00FF00>Version文件构建成功!</color> 路径: {filePath} 内容: {packageVersion}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"构建Version文件失败: {e.Message}");
        }
    }
    
    private static void CopyAssetsToTarget(string serveDirectory,BuildTarget buildTarget,BuildResult buildResult)
    {
        if (!System.IO.Directory.Exists(serveDirectory))
        {
            System.IO.Directory.CreateDirectory(serveDirectory);
            Debug.Log($"已创建目录: {serveDirectory}");
        }

        //服务器用 start
        string sourceDirectory = buildResult.OutputPackageDirectory;
        string targetDirectory = $"{serveDirectory}/{buildTarget}/{Application.version}";
        if (buildTarget == BuildTarget.Android)
        {
            targetDirectory = $"{serveDirectory}/Android/{Application.version}";
        }
        else if (buildTarget == BuildTarget.StandaloneWindows64)
        {
            targetDirectory = $"{serveDirectory}/WindowsPlayer/{Application.version}";
        }

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }
        else
        {
            foreach (string file in Directory.GetFiles(targetDirectory))
            {
                File.Delete(file);
            }
        }
        foreach (string file in Directory.GetFiles(sourceDirectory))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(targetDirectory, fileName);
            File.Copy(file, destFile, true);
        }
        UnityEngine.Debug.Log($"资源已复制到 : {targetDirectory}");
    }
    
    private static string GetBuiltinShaderBundleName()
    {
        var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
        var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
        return packRuleResult.GetBundleName(PackageName, uniqueBundleName);
    }
}

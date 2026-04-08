
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HybridCLR.Editor;
using HybridCLR.Editor.AOT;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Settings;
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

    // public static void CompileDllActiveBuildTargetCopy()
    // {
    //     HybridCLR.Editor.Commands.CompileDllCommand.CompileDll(EditorUserBuildSettings.activeBuildTarget);
    //     CopyDllToAssets();
    // }

    public static void CopyHofixDll()
    {
        CompileDllCommand.CompileDll(EditorUserBuildSettings.activeBuildTarget);
        UnityEngine.Debug.Log($"脚本编译 完成!!!");
    
        string CodeDir = "Assets/Game/Download/Hotfix/";
        string ScriptAssembliesDir = Application.dataPath + "/../" + "HybridCLRData/HotUpdateDlls/" + EditorUserBuildSettings.activeBuildTarget.ToString() + "/Assembly-CSharp.dll";
        File.Copy(ScriptAssembliesDir, Path.Combine(CodeDir, "Assembly-CSharp.dll.bytes"), true);
        string aotMetaAssemblyDir = Application.dataPath + "/../" + "HybridCLRData/AssembliesPostIl2CppStrip/" + EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
        foreach (var aotDllName in AOTGenericReferences.PatchedAOTAssemblyList)
        {
            File.Copy(aotMetaAssemblyDir + aotDllName, Path.Combine(CodeDir, aotDllName + ".bytes"), true);
        }
    
        AssetDatabase.Refresh();
        Debug.Log("热更dll和补充元数据dll, 复制到Game/Download/Hotfix完成");
    }
    
    public static void CopyDllToAssets()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        string buildDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        // 项目配置的热更dll
        for (int i = 0; i < HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions.Length; i++)
        {
            string fileName = HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions[i].name + ".dll";
            string sourcePath = Directory.GetFiles(buildDir).ToList().Find(hotPath => hotPath.Contains(fileName));
            if (string.IsNullOrEmpty(sourcePath))
            {
                UnityEngine.Debug.LogError($"热更程序集不存在: {buildDir} / {fileName}");
                continue;
            }

            // 将程序集添加后缀 .bytes 并复制到AB包路径下
            string newFileName = fileName + ".bytes";
            string targetDirectory = Application.dataPath + "/Res/AOTAssembly";

            UnityEngine.Debug.Log($"目标目录路径:{targetDirectory} ");
            // 检查源文件是否存在
            if (File.Exists(sourcePath))
            {
                // 构建目标文件的完整路径
                string destinationPath = Path.Combine(targetDirectory, newFileName);
                // 检查目标目录是否存在，如果不存在则创建
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                // 如果目标文件已经存在，则删除
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                // 将源文件复制到目标目录下，并修改名称
                File.Copy(sourcePath, destinationPath);
                // AOTAssemblyMetadataStripper.Strip(sourcePath, destinationPath);
                // 刷新资源，使其在 Unity 编辑器中可见
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log("File copied successfully!");
            }
            else
            {
                UnityEngine.Debug.LogError("Source file does not exist!");
            }
        }

        //补充元数据
        buildDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
        for (int i = 0; i < AOTGenericReferences.PatchedAOTAssemblyList.Count; i++)
        {
            string fileName = AOTGenericReferences.PatchedAOTAssemblyList[i];
            string sourcePath = Directory.GetFiles(buildDir).ToList().Find(hotPath => hotPath.Contains(fileName));
            if (string.IsNullOrEmpty(sourcePath))
            {
                UnityEngine.Debug.LogError($"热更程序集不存在: {buildDir}/{fileName}");
                continue;
            }

            // 将程序集添加后缀 .bytes 并复制到AB包路径下
            string newFileName = fileName + ".bytes";
            string targetDirectory = Application.dataPath + "/Res/AOTAssembly/Others";

            UnityEngine.Debug.Log($"目标目录路径:{targetDirectory} ");
            // 检查源文件是否存在
            if (File.Exists(sourcePath))
            {
                // 构建目标文件的完整路径
                string destinationPath = Path.Combine(targetDirectory, newFileName);
                // 检查目标目录是否存在，如果不存在则创建
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                // 如果目标文件已经存在，则删除
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                // 将源文件复制到目标目录下，并修改名称
                // File.Copy(sourcePath, destinationPath);
                // 进一步剔除AOT dll中非泛型函数元数据
                AOTAssemblyMetadataStripper.Strip(sourcePath, destinationPath);
                // 刷新资源，使其在 Unity 编辑器中可见
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log("File copied successfully!");
            }
            else
            {
                UnityEngine.Debug.LogError("Source file does not exist!");
            }
        }

        UnityEngine.Debug.Log("复制热更的DLL到资源目录 完成!!!");
    }

    private static Dictionary<string, string> FilterRuleDict;
    //过滤不需要打入包的资源
    private static void FilterPackRes()
    {
        FilterRuleDict = new Dictionary<string, string>();
        foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
        {
            if (package.PackageName == PackageName)
            {
                foreach (var group in package.Groups)
                {
                    List<AssetBundleCollector> collectors = group.Collectors;
                    foreach(AssetBundleCollector collector in group.Collectors)
                    {
                        if (collector.CollectPath == "Assets/Res/Prefabs/Scene")
                        {
                            FilterRuleDict.Add(collector.CollectPath, collector.FilterRuleName);
                            collector.FilterRuleName = nameof(CollectScene);//修改Collect规则，把CollectPrefab修改成CollectScene就不会收集预制体了
                        }
                    }
                }
            }
        }
        // 保存设置
        //EditorUtility.SetDirty(AssetBundleCollectorSettingData.Setting);
        //AssetDatabase.SaveAssets();
    }

    //还原默认的FilterRuleName
    private static void ResetFilterRule()
    {
        foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
        {
            if (package.PackageName == PackageName)
            {
                foreach (var group in package.Groups)
                {
                    List<AssetBundleCollector> collectors = group.Collectors;
                    foreach (AssetBundleCollector collector in group.Collectors)
                    {
                        if (FilterRuleDict.ContainsKey(collector.CollectPath))
                        {
                            collector.FilterRuleName = FilterRuleDict[collector.CollectPath];
                        }
                    }
                }
            }
        }
        // 保存设置
        //EditorUtility.SetDirty(AssetBundleCollectorSettingData.Setting);
        //AssetDatabase.SaveAssets();
    }




    public static bool BuildInternal()
    {
        CopyHofixDll();
#if UNITY_WEBGL
        return BuildInternal(BuildTarget.WebGL);
#elif UNITY_ANDROID
        return BuildInternal(BuildTarget.Android);
#elif UNITY_STANDALONE
        return BuildInternal(BuildTarget.StandaloneWindows64);
#endif
    }

    private static bool BuildInternal(BuildTarget buildTarget)
    {
        string serveDirectory = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
        if (!System.IO.Directory.Exists(serveDirectory))
        {
            System.IO.Directory.CreateDirectory(serveDirectory);
            Debug.Log($"已创建目录: {serveDirectory}");
        }
        
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
            //服务器用 end
            return true;
        }
        else
        {
            UnityEngine.Debug.LogError($"构建失败 : {buildResult.ErrorInfo}");
        }
        return false;
    }

    private static string GetBuiltinShaderBundleName()
    {
        var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
        var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
        return packRuleResult.GetBundleName(PackageName, uniqueBundleName);
    }
}

public class BuildInternalWindow : EditorWindow
{
    private bool removePackRes;
    private Action<bool> callBack;
    public static void Open(Action<bool> callBack)
    {
        BuildInternalWindow window = GetWindow<BuildInternalWindow>("构建资源包");
        window.callBack = callBack;
    }

    void OnGUI()
    {
        GUILayout.Space(10);
        removePackRes = EditorGUILayout.Toggle("是否剔除首包资源", removePackRes);

        GUILayout.Space(5);

        if (GUILayout.Button("开始构建", GUILayout.Height(30)))
        {
            callBack?.Invoke(removePackRes);
        }
    }



    
   



}

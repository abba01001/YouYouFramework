using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

using Debug = UnityEngine.Debug;

[CreateAssetMenu(menuName = "YouYouAsset/AssetBundleSettings")]
public class AssetBundleSettings : ScriptableObject
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

    [PropertySpace(2)]
    [HorizontalGroup("Common", LabelWidth = 75)]
    [VerticalGroup("Common/Left")]
    [LabelText("资源版本号")]
    public string AssetVersion = "1.0.0";

    [PropertySpace(5)]
    [VerticalGroup("Common/Left")]
    [LabelText("目标平台")]
    public CusBuildTarget CurrBuildTarget;

    public BuildTarget GetBuildTarget()
    {
        switch (CurrBuildTarget)
        {
            default:
            case CusBuildTarget.Windows:
                return BuildTarget.StandaloneWindows64;
            case CusBuildTarget.Android:
                return BuildTarget.Android;
            case CusBuildTarget.IOS:
                return BuildTarget.iOS;
            case CusBuildTarget.WebGL:
                return BuildTarget.WebGL;
        }
    }

    [PropertySpace(3)]
    [VerticalGroup("Common/Left")]
    [LabelText("参数")]
    public BuildAssetBundleOptions Options;

    [PropertySpace(6)]
    [VerticalGroup("Common/Left")]
    [LabelText("输出路径")]
    [ReadOnly] // 如果你有定义 ReadOnly 特性，可以用这个来显示为只读
    [SerializeField]
    private string PublishPath = "";//$"{Application.persistentDataPath}/输出路径";
    
    [VerticalGroup("Common/Left")]
    [Button("AB包资源预览",ButtonSizes.Medium)]
    public void Test()
    {
        GameUtil.LogError(EditorUserBuildSettings.activeBuildTarget,"===",GetBuildTarget());
    }

    [VerticalGroup("Common/Left")]
    [Button("启动本地AB包资源存储", ButtonSizes.Medium)]
    void StartLocalServer()
    {
        string projectPath = Application.dataPath.Replace("/Assets", "");
        string serveDirectory = projectPath + "/ServerBundles";
        if (!System.IO.Directory.Exists(serveDirectory))
        {
            System.IO.Directory.CreateDirectory(serveDirectory);
            Debug.Log($"已创建目录: {serveDirectory}");
        }

        // 转换路径格式
        serveDirectory = serveDirectory.Replace("\\", "/");
        serveDirectory = "\"" + serveDirectory + "\"";

        // 👉 获取本地IP
        string localIP = GetLocalIPAddress();

        // 杀掉占用 8000 端口的进程
        KillProcessOnPort(8000);

        // 启动 Python 服务器
        StartPythonServer(serveDirectory);

        Debug.Log($"服务器已启动: http://{localIP}:8000/");
        Debug.Log($"服务目录: {serveDirectory}");
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
    
    [VerticalGroup("Common/Right")]
    [Button("清空本地AB包资源",ButtonSizes.Medium)]
    public void ClearAssetBundle()
    {
        if (Directory.Exists(TempPath))
        {
            Directory.Delete(TempPath, true);
        }
        EditorUtility.DisplayDialog("", "清空完毕", "确定");
    }

    private void SetKeystoreInfo()
    {
        PlayerSettings.Android.keystoreName = keystoreRelativePath;
        PlayerSettings.Android.keystorePass = keystorePassword;
        PlayerSettings.Android.keyaliasName = keyAlias;
        PlayerSettings.Android.keyaliasPass = keyPassword;
    }
    
    /// <summary>
    /// 要收集的资源包
    /// </summary>
    List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

    [VerticalGroup("Common/Right")]
    [Button("输出AB包资源到本地",ButtonSizes.Medium)]
    public void BuildAssetBundle()
    {
        MacroSettings MacroSettings = GetMacroSettings();
        if (CurrBuildTarget == CusBuildTarget.Android && MacroSettings.CurrAssetLoadTarget != MacroSettings.AssetLoadTarget.ASSETBUNDLE)
        {
            EditorUtility.DisplayDialog(
                "提示",
                $"安卓平台请先切换到{MacroSettings.AssetLoadTarget.ASSETBUNDLE}模式",
                "确定"
            );
            return;
        }
        // if (IsUploadStreamingAsset)
        // {
        //     CopyAssetBundlesToStreamingAssets(AssetVersion);
        // }
    }

    private async Task UploadAssetsAsync()
    {
        // if (IsUploadCloudAsset || IsForceUploadCloudAsset)
        // {
        //     COSUploader.ShowUploadWindow();
        //     await UniTask.Delay(3000);  // 延迟 1 秒
        //     // 等待文件存在
        //     COSUploader.UploadVersion(AssetVersion);
        //     COSUploader.UploadAssetBundle(AssetVersion, GetUploadPath(),IsForceUploadCloudAsset); 
        // }
    }
    
    //复制到steamingassets资源
    public static void CopyAssetBundlesToStreamingAssets(string version)
    {
        string baseFolder = Path.Combine(SettingsUtil.ProjectDir, "AssetBundles", version);
        string targetFolder = Path.Combine(Application.streamingAssetsPath, "AssetBundles", version);
        CopyDirectory(baseFolder, targetFolder);
   
        // 👇 写到目标目录
        string filePath = Path.Combine(Application.streamingAssetsPath, "AssetBundles","version.txt");
        string[] versionParts = version.Split('.');
        string apkVersion = versionParts[0] + ".0.0";
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.WriteLine(apkVersion);
            writer.WriteLine(version);
        }
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
    
    public static MacroSettings GetMacroSettings()
    {
        return AssetDatabase.LoadAssetAtPath<MacroSettings>("Assets/Game/YouYouFramework/YouYouAssets/MacroSettings.asset");
    }

    private string GetUploadPath()
    {
        string path = "";
        switch (CurrBuildTarget)
        {
            default:
            case CusBuildTarget.Windows:
                path = "/Unity/AssetBundle/"+ AssetVersion+"/"+ "Windows"+ "/";
                break;
            case CusBuildTarget.Android:
                path = "/Unity/AssetBundle/" + AssetVersion+"/" + "Android" + "/";
                break;
            case CusBuildTarget.IOS:
                break;
        }
        return path;
    }


    [VerticalGroup("Common/Right")]
    [Button("出包",ButtonSizes.Medium)]
    public void PublishAPK()
    {
        SetKeystoreInfo();
        var path = PublishPath + $"/{AssetVersion}.apk";
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
            GameEntry.LogError(errorMessage);
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

    [VerticalGroup("Common/Right")]
    [Button("清理StreamingAssets资源", ButtonSizes.Medium)]
    public void ClarStreamingAssets()
    {
        string assetBundlesPath = Path.Combine(Application.streamingAssetsPath, "AssetBundles");

        // 确保 AssetBundles 文件夹存在
        if (Directory.Exists(assetBundlesPath))
        {
            try
            {
                Directory.Delete(assetBundlesPath, true);  // 递归删除
                Debug.Log("StreamingAssets/AssetBundles资源清理完成！");
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
    
    #region TempPath OutPath
    /// <summary>
    /// 临时目录
    /// </summary>
    public string TempPath
    {
        get
        {
            return Application.dataPath + "/../" + AssetBundleSavePath + "/" + AssetVersion + "_Temp/" + CurrBuildTarget;
        }
    }
    
    /// <summary>
    /// 临时目录
    /// </summary>
    public string TempGradlePath
    {
        get
        {
            return Application.dataPath + "/../" + GradleSavePath;
        }
    }

    /// <summary>
    /// 输出目录
    /// </summary>
    public string OutPath
    {
        get
        {
            return TempPath.Replace("_Temp", "");
        }
    }
    #endregion

    #region CopyFile 拷贝文件到正式目录
    /// <summary>
    /// 拷贝文件到正式目录
    /// </summary>
    /// <param name="oldPath"></param>
    private void CopyFile(string oldPath)
    {
        if (Directory.Exists(OutPath))
        {
            Directory.Delete(OutPath, true);
        }

        IOUtil.CopyDirectory(oldPath, OutPath);
        DirectoryInfo directory = new DirectoryInfo(OutPath);

        //拿到文件夹下所有文件
        FileInfo[] arrFiles = directory.GetFiles("*.y", SearchOption.AllDirectories);
        int len = arrFiles.Length;
        for (int i = 0; i < len; i++)
        {
            FileInfo file = arrFiles[i];
            File.Move(file.FullName, file.FullName.Replace(".ab.y", ".assetbundle"));
        }
    }
    #endregion

    #region AssetBundleEncrypt 资源包加密
    /// <summary>
    /// 资源包加密
    /// </summary>
    /// <param name="path"></param>
    private void AssetBundleEncrypt()
    {
        int len = Datas.Length;
        for (int i = 0; i < len; i++)
        {
            AssetBundleData assetBundleData = Datas[i];
            if (assetBundleData.IsEncrypt && assetBundleData.IsBuild)
            {
                //如果需要加密
                for (int j = 0; j < assetBundleData.Path.Length; j++)
                {
                    string path = OutPath + "/" + assetBundleData.Path[j];

                    if (assetBundleData.Overall)
                    {
                        //不是遍历文件夹打包 说明这个路径就是一个包
                        path = path + ".assetbundle";
                        AssetBundleEncryptFile(path);
                    }
                    else
                    {
                        AssetBundleEncryptFolder(path);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 加密文件夹下所有文件
    /// </summary>
    /// <param name="folderPath"></param>
    private void AssetBundleEncryptFolder(string folderPath, bool isDelete = false)
    {
        DirectoryInfo directory = new DirectoryInfo(folderPath);

        //拿到文件夹下所有文件
        FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);

        foreach (FileInfo file in arrFiles)
        {
            AssetBundleEncryptFile(file.FullName, isDelete);
        }
    }

    /// <summary>
    /// 加密文件
    /// </summary>
    /// <param name="filePath"></param>
    private void AssetBundleEncryptFile(string filePath, bool isDelete = false)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        byte[] buffer = null;

        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
        }

        buffer = SecurityUtil.Xor(buffer);

        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            fs.Write(buffer, 0, buffer.Length);
            fs.Flush();
        }
    }
    #endregion

    #region CreateVersionFile 生成版本文件
    /// <summary>
    /// 生成版本文件
    /// </summary>
    private void CreateVersionFile()
    {
        string path = OutPath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string strVersionFilePath = path + "/VersionFile.txt"; //版本文件路径
        //如果版本文件存在 则删除
        IOUtil.DeleteFile(strVersionFilePath);
        StringBuilder sbContent = new StringBuilder();
        DirectoryInfo directory = new DirectoryInfo(path);

        //拿到文件夹下所有文件
        FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);
        sbContent.AppendLine(AssetVersion);
        for (int i = 0; i < arrFiles.Length; i++)
        {
            FileInfo file = arrFiles[i];

            if (file.Extension == ".manifest")
            {
                continue;
            }
            string fullName = file.FullName; //全名 包含路径扩展名

            //相对路径
            string name = fullName.Substring(fullName.IndexOf(CurrBuildTarget.ToString()) + CurrBuildTarget.ToString().Length + 1);

            string md5 = EncryptUtil.GetFileMD5(fullName); //文件的MD5
            if (md5 == null) continue;

            string size = file.Length.ToString(); //文件大小

            bool isFirstData = false; //是否初始数据
            bool isEncrypt = false;
            bool isBreak = false;

            for (int j = 0; j < Datas.Length; j++)
            {
                foreach (string mPath in Datas[j].Path)
                {
                    string tempPath = mPath;

                    name = name.Replace("\\", "/");
                    if (name.IndexOf(tempPath, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        isFirstData = Datas[j].IsFirstData;
                        isEncrypt = Datas[j].IsEncrypt;
                        isBreak = true;
                        break;
                    }
                }
                if (isBreak) break;
            }

            string strLine = string.Format("{0}|{1}|{2}|{3}|{4}", name, md5, size, isFirstData ? 1 : 0, isEncrypt ? 1 : 0);
            sbContent.AppendLine(strLine);
        }
        IOUtil.CreateTextFile(strVersionFilePath, sbContent.ToString());
        MMO_MemoryStream ms = new MMO_MemoryStream();
        string str = sbContent.ToString().Trim();
        string[] arr = str.Split('\n');
        int len = arr.Length;
        ms.WriteInt(len);
        for (int i = 0; i < len; i++)
        {
            if (i == 0)
            {
                ms.WriteUTF8String(arr[i]);
            }
            else
            {
                string[] arrInner = arr[i].Split('|');
                ms.WriteUTF8String(arrInner[0]);
                ms.WriteUTF8String(arrInner[1]);
                ms.WriteULong(ulong.Parse(arrInner[2]));
                ms.WriteByte(byte.Parse(arrInner[3]));
                ms.WriteByte(byte.Parse(arrInner[4]));
            }
        }

        string filePath = path + "/VersionFile.bytes"; //版本文件路径
        byte[] buffer = ms.ToArray();
        buffer = ZlibHelper.CompressBytes(buffer);
        FileStream fs = new FileStream(filePath, FileMode.Create);
        fs.Write(buffer, 0, buffer.Length);
        fs.Close();
        fs.Dispose();
    }

    #endregion

    #region GetAssetBundleName 获取资源包的名称
    /// <summary>
    /// 获取资源包的名称
    /// </summary>
    /// <param name="assetFullName"></param>
    /// <returns></returns>
    private string GetAssetBundleName(string assetFullName)
    {
        int len = Datas.Length;
        //循环设置文件夹包括子文件里边的项
        for (int i = 0; i < len; i++)
        {
            AssetBundleData assetBundleData = Datas[i];
            for (int j = 0; j < assetBundleData.Path.Length; j++)
            {
                if (assetFullName.IndexOf(assetBundleData.Path[j], StringComparison.CurrentCultureIgnoreCase) > -1)
                {
                    if (assetBundleData.Overall)
                    {
                        //文件夹是个整包 则返回这个特文件夹名字
                        return assetBundleData.Path[j].ToLower();
                    }
                    else
                    {
                        //零散资源
                        //return assetFullName.Substring(0, assetFullName.LastIndexOf('.')).ToLower().Replace("assets/", "");
                        return assetFullName.ToLower().Replace("assets/", "");
                    }
                }
            }
        }
        return null;
    }
    #endregion

    #region CopyHofixDll 热更Dll复制到Hotfix目录
    /// <summary>
    /// 热更Dll复制到Hotfix目录
    /// </summary>
    public void CopyHofixDll()
    {
        CompileDllCommand.CompileDll(GetBuildTarget());

        string CodeDir = "Assets/Game/Download/Hotfix/";

        string ScriptAssembliesDir = Application.dataPath + "/../" + "HybridCLRData/HotUpdateDlls/" + GetBuildTarget().ToString() + "/Assembly-CSharp.dll";
        File.Copy(ScriptAssembliesDir, Path.Combine(CodeDir, "Assembly-CSharp.dll.bytes"), true);
        SupplementAOTDll t = new SupplementAOTDll();
        string aotMetaAssemblyDir = Application.dataPath + "/../" + "HybridCLRData/AssembliesPostIl2CppStrip/" + GetBuildTarget().ToString() + "/";
        foreach (var aotDllName in t.aotMetaAssemblyFiles)
        {
            File.Copy(aotMetaAssemblyDir + aotDllName, Path.Combine(CodeDir, aotDllName + ".bytes"), true);
        }

        AssetDatabase.Refresh();
        Debug.Log("热更dll和补充元数据dll, 复制到Game/Download/Hotfix完成");
    }
    #endregion

    /// <summary>
    /// 根据路径打包资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="overall">打成一个资源包</param>
    private void BuildAssetBundleForPath(string path, bool overall)
    {
        string fullPath = Application.dataPath + "/" + path;
        //Debug.LogError("fullPath=" + fullPath);

        //1.拿到文件夹下所有文件
        DirectoryInfo directory = new DirectoryInfo(fullPath);
        FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);
        HandleNormalPack(overall, arrFiles, path);
    }
    
    //处理正常的包
    private void HandleNormalPack(bool overall,FileInfo[] arrFiles,string path)
    {
        if (overall)
        {
            //打成一个资源包
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = path + ".ab";
            build.assetBundleVariant = "y";
            string[] arr = GetValidateFiles(arrFiles);
            build.assetNames = arr;
            builds.Add(build);
        }
        else
        {
            //每个文件打成一个包
            string[] arr = GetValidateFiles(arrFiles);
            for (int i = 0; i < arr.Length; i++)
            {
                AssetBundleBuild build = new AssetBundleBuild();
                //build.assetBundleName = arr[i].Substring(0, arr[i].LastIndexOf('.')).Replace("Assets/", "") + ".ab";
                build.assetBundleName = arr[i].Replace("Assets/", "") + ".ab";
                build.assetBundleVariant = "y";
                build.assetNames = new string[] { arr[i] };

                //Debug.LogError("assetBundleName==" + build.assetBundleName);
                builds.Add(build);
            }
        }
    }

    private void OnEnable()
    {
        PublishPath = $"{Application.persistentDataPath}/OutPut";
        PlayerSettings.bundleVersion = AssetVersion;
        IsForceUploadCloudAsset = false;
        IsUploadAPK = false;
    }
    
    private void OnValidate()
    {
        PlayerSettings.bundleVersion = AssetVersion;
    }

    
    private string[] GetValidateFiles(FileInfo[] arrFiles)
    {
        List<string> lst = new List<string>();

        int len = arrFiles.Length;
        for (int i = 0; i < len; i++)
        {
            FileInfo file = arrFiles[i];
            if (!file.Extension.Equals(".meta", StringComparison.CurrentCultureIgnoreCase))
            {
                lst.Add("Assets" + file.FullName.Replace("\\", "/").Replace(Application.dataPath, ""));
            }
        }

        return lst.ToArray();
    }

    [LabelText("勾选输出资源时上传到云端(增量上传)")]
    public bool IsUploadCloudAsset;
    
    [LabelText("勾选输出资源时上传到云端(全部上传)")]
    public bool IsForceUploadCloudAsset;
    
    [LabelText("勾选输出包时上传到云端")]
    public bool IsUploadAPK;
    
    [LabelText("勾选输出资源时上传到包内")]
    public bool IsUploadStreamingAsset;
    
    [LabelText("资源包保存路径")]
    [FolderPath]
    /// <summary>
    /// 资源包保存路径
    /// </summary>
    public string AssetBundleSavePath;

    [LabelText("导出Gradle工程路径")]
    [FolderPath]
    /// <summary>
    /// 资源包保存路径
    /// </summary>
    public string GradleSavePath;
    
    [LabelText("勾选进行编辑")]
    public bool IsCanEditor;

    [EnableIf("IsCanEditor")]
    [BoxGroup("AssetBundleSettings")]
    public AssetBundleData[] Datas;

    //必须加上可序列化标记
    [Serializable]
    public class AssetBundleData
    {
        [LabelText("名称")]
        /// <summary>
        /// 名称
        /// </summary>
        public string Name;

        [LabelText("是否要打包")]
        public bool IsBuild = true;
        
        [LabelText("文件夹为一个资源包")]
        /// <summary>
        /// 文件夹为一个资源包
        /// </summary>
        public bool Overall;

        [LabelText("是否初始资源")]
        /// <summary>
        /// 是否初始资源
        /// </summary>
        public bool IsFirstData = true;

        [LabelText("是否加密")]
        /// <summary>
        /// 是否加密
        /// </summary>
        public bool IsEncrypt;

        [FolderPath(ParentFolder = "Assets")]
        /// <summary>
        /// 路径
        /// </summary>
        public string[] Path;
    }
    

}

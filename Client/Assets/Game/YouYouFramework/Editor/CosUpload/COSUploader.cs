using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using COSXML.CosException;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using COSXML.Utils;
using Cysharp.Threading.Tasks;
using HybridCLR.Editor;
using Main;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace YouYou
{
    public static class COSUploader
    {
        private static CosConfig cosConfig;
        private static StringBuilder successLog = new StringBuilder(); // 用于记录成功的上传状态
        private static StringBuilder failureLog = new StringBuilder(); // 用于记录失败的上传状态
        private static int totalFileCount = 0;
        private static int successCount = 0;
        private static int failCount = 0;
        private static string rootPath = @$"{System.IO.Directory.GetParent(Application.dataPath).FullName}\AssetBundles\{Application.version}\Android\";
        private static string localVersionFilePath = $"{Application.persistentDataPath}/{YFConstDefine.VersionFileName}";
        private static string cloudVersionFilePath = $"{SystemModel.Instance.CurrChannelConfig.EditorRealSourceUrl}{YFConstDefine.VersionFileName}";
        private static UploadResultWindow uploadWindow;
        private static Stopwatch stopwatch;
        
        private static Dictionary<string, VersionFileEntity> m_CDNVersionDic = new Dictionary<string, VersionFileEntity>();
        private static Dictionary<string, VersionFileEntity> m_LocalAssetsVersionDic = new Dictionary<string, VersionFileEntity>();
        
        public static async Task UploadVersion(string sourceVersion)
        {
            CosXml cosXml = CreateCosXml();
            string localFilePath = Path.Combine(Application.persistentDataPath, "APKVersion.txt");
            string[] versionParts = sourceVersion.Split('.');
            string apkVersion = versionParts[0] + ".0.0"; 
            using (StreamWriter writer = new StreamWriter(localFilePath, false))
            {
                await writer.WriteLineAsync(apkVersion);
                await writer.WriteLineAsync(sourceVersion);
            }

            
            // 将版本信息写入 Resources 资源
            string resourcesVersionFilePath = Path.Combine(Application.dataPath, "Resources", "version.txt");
            using (StreamWriter writer = new StreamWriter(resourcesVersionFilePath, false))
            {
                await writer.WriteLineAsync(apkVersion);
                await writer.WriteLineAsync(sourceVersion);
            }
            
            string relativePath = Path.GetFileName(localFilePath);
            using (FileStream fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var dic = SecurityUtil.GetSecretKeyDic();
                PutObjectRequest request = new PutObjectRequest(dic["bucket"], "APK/" + relativePath, fileStream);
                request.SetSign(TimeUtils.GetCurrentTime(TimeUnit.Seconds), 600);
                try
                {
                    await Task.Run(() => cosXml.PutObject(request));
                }
                catch (Exception ex)
                {
                    GameEntry.LogError($"{relativePath} 上传状态：<color=red>失败</color>，错误：{ex.Message}");
                }
            }
        }

        public static async Task UploadAPK(string apkFilePath)
        {
            // 获取APK文件名
            CosXml cosXml = CreateCosXml();
            string apkFileName = Path.GetFileName(apkFilePath);
            using (FileStream fileStream = new FileStream(apkFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var dic = SecurityUtil.GetSecretKeyDic();
                PutObjectRequest request = new PutObjectRequest(dic["bucket"], "APK/" + apkFileName, fileStream);
                request.SetSign(TimeUtils.GetCurrentTime(TimeUnit.Seconds), 600);
        
                try
                {
                    await Task.Run(() => cosXml.PutObject(request));
                    EditorUtility.DisplayDialog("上传至云端", "APK上传成功！", "确定");
                }
                catch (Exception ex)
                {
                    GameEntry.LogError($"{apkFileName} 上传状态：<color=red>失败</color>，错误：{ex.Message}");
                }
            }
        }

        public static async Task ShowUploadWindow()
        {
            if (uploadWindow == null) uploadWindow = new UploadResultWindow(); // 创建上传结果窗口
            uploadWindow.Show(); // 显示窗口
            successLog.AppendLine($"倒计时3秒后开启上传至云端");
            uploadWindow.UpdateLog(successLog.ToString(), failureLog.ToString(), "");
            await UniTask.Delay(1000); // 延迟 1 秒
            successLog.AppendLine($"倒计时2秒后开启上传至云端");
            uploadWindow.UpdateLog(successLog.ToString(), failureLog.ToString(), "");
            await UniTask.Delay(1000); // 延迟 1 秒
            successLog.AppendLine($"倒计时1秒后开启上传至云端");
            uploadWindow.UpdateLog(successLog.ToString(), failureLog.ToString(), "");
            await UniTask.Delay(1000); // 延迟 1 秒
            successLog.AppendLine($"开始上传");
            uploadWindow.UpdateLog(successLog.ToString(), failureLog.ToString(), "");
        }
        
        public static async void UploadAssetBundle(string version,string uploadPath)
        {
            cosConfig = Resources.Load<CosConfig>("CosConfig");
            CosXml cosXml = CreateCosXml();
            stopwatch = new Stopwatch(); // 用于记录耗时
            if(uploadWindow == null) uploadWindow = new UploadResultWindow(); // 创建上传结果窗口
            uploadWindow.Show(); // 显示窗口
            
            await GetCloudAssetFiles();
            try
            {
                stopwatch.Start(); // 开始计时
                // 获取当前的构建平台
                BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
                // 设置基础文件夹
                string baseFolder = Path.Combine(SettingsUtil.ProjectDir, "AssetBundles", version, GetPlatformOption(activeBuildTarget));
                var dic = SecurityUtil.GetSecretKeyDic();
                // 递归上传整个文件夹
                await UploadFolderAsync(cosXml, dic["bucket"], uploadPath, baseFolder);
                string totalTime = $"所有文件上传完成，总耗时: {stopwatch.Elapsed.TotalSeconds:0.00} 秒";
                uploadWindow.UpdateLog(successLog.ToString(), failureLog.ToString(), totalTime);
            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                GameEntry.LogError("CosClientException: " + clientEx);
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                GameEntry.LogError("CosServerException: " + serverEx.GetInfo());
            }
            finally
            {
                // 上传完成后关闭窗口并显示结果提示
                //uploadWindow.Close();
                ShowUploadResult();
            }
        }

        
        public static async Task GetCloudAssetFiles()
        {
            m_CDNVersionDic.Clear();
            m_LocalAssetsVersionDic.Clear();
            if (File.Exists(localVersionFilePath))
            {
                m_LocalAssetsVersionDic = GetAssetBundleVersionList();
            }
            
            StringBuilder sbr = StringHelper.PoolNew();
            string url = sbr.AppendFormatNoGC(cloudVersionFilePath).ToString();
            try
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.timeout = 2;
                    await request.SendWebRequest();
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        m_CDNVersionDic = GetAssetBundleVersionList(request.downloadHandler.data);

                    }
                }
            }
            catch (Exception ex)
            {
                // 捕获异常并打印错误信息
                GameUtil.LogError($"获取云端版本文件时发生异常: {ex.Message}");
            }
            
        }


        private static Dictionary<string, VersionFileEntity> GetAssetBundleVersionList(byte[] buffer)
        {
            buffer = ZlibHelper.DeCompressBytes(buffer);
            Dictionary<string, VersionFileEntity> dic = new Dictionary<string, VersionFileEntity>();
            MMO_MemoryStream ms = new MMO_MemoryStream(buffer);
            int len = ms.ReadInt();
            for (int i = 0; i < len; i++)
            {
                if (i == 0)
                {
                    ms.ReadUTF8String().Trim();
                }
                else
                {
                    VersionFileEntity entity = new VersionFileEntity();
                    entity.AssetBundleName = ms.ReadUTF8String();
                    entity.MD5 = ms.ReadUTF8String();
                    entity.Size = ms.ReadULong();
                    entity.IsFirstData = ms.ReadByte() == 1;
                    entity.IsEncrypt = ms.ReadByte() == 1;
                    dic[entity.AssetBundleName] = entity;
                }
            }
            return dic;
        }
        
        private static Dictionary<string, VersionFileEntity> GetAssetBundleVersionList()
        {
            string json = IOUtil.GetFileText(localVersionFilePath);
            return json.ToObject<Dictionary<string, VersionFileEntity>>();
        }
        
        // 根据构建平台获取对应的 PlatformOption 值
        private static string GetPlatformOption(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.Android:
                    return "Android";
                default:
                    return "Windows"; // 默认返回 Windows
            }
        }

        // 异步递归上传整个文件夹的方法
        static async Task UploadFolderAsync(CosXml cosXml, string bucket, string cosPath, string localFolderPath)
        {
            // 上传文件
            string[] files = Directory.GetFiles(localFolderPath);
            foreach (string filePath in files)
            {
                
                string tempPath = Path.GetRelativePath(rootPath, filePath);
                tempPath = tempPath.Replace("\\", "/");
                if (!CheckNeedUpload(tempPath)) continue;
                
                string relativePath = filePath.Substring(localFolderPath.Length + 1);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    PutObjectRequest request = new PutObjectRequest(bucket, cosPath + "/" + relativePath, fileStream);
                    request.SetSign(TimeUtils.GetCurrentTime(TimeUnit.Seconds), 600);
                    try
                    {
                        await Task.Run(() =>
                        {
                            PutObjectResult result = cosXml.PutObject(request);
                            if (result.IsSuccessful())
                            {
                                successCount += 1;
                                successLog.AppendLine($"{Path.GetFileName(filePath)}      上传成功");
                            }
                            else
                            {
                                failCount += 1;
                                failureLog.AppendLine($"{Path.GetFileName(filePath)}      上传失败");
                            }

                            totalFileCount += 1;
                        });
                    }
                    catch (Exception ex)
                    {
                        failureLog.AppendLine($"{Path.GetFileName(filePath)}      上传状态：<color=red>失败</color>，错误：{ex.Message}");
                    }

                    // 实时更新窗口
                    string totalTime = $"当前耗时: {stopwatch.Elapsed.TotalSeconds:0.00} 秒";
                    uploadWindow.UpdateLog(successLog.ToString(), failureLog.ToString(), totalTime);
                    uploadWindow.Repaint();  // 实时刷新窗口
                }
            }

            // 递归上传子文件夹
            string[] subFolders = Directory.GetDirectories(localFolderPath);
            foreach (string subFolder in subFolders)
            {
                string relativePath = subFolder.Substring(localFolderPath.Length + 1);
                await UploadFolderAsync(cosXml, bucket, cosPath + "/" + relativePath, subFolder);
            }
        }

        private static bool CheckNeedUpload(string abName)
        {
            if (m_LocalAssetsVersionDic.Count == 0 || m_CDNVersionDic.Count == 0) return true;
            if (abName == "Android" || abName == "AssetInfo.bytes" || abName == "AssetInfo.json") return true;
            if (m_CDNVersionDic.ContainsKey(abName))
            {
                if (m_LocalAssetsVersionDic.ContainsKey(abName))
                {
                    bool needUpload = m_LocalAssetsVersionDic[abName].MD5 != m_CDNVersionDic[abName].MD5 ||
                                      m_LocalAssetsVersionDic[abName].Size != m_CDNVersionDic[abName].Size ||
                                      m_LocalAssetsVersionDic[abName].IsEncrypt != m_CDNVersionDic[abName].IsEncrypt ||
                                      m_LocalAssetsVersionDic[abName].IsFirstData != m_CDNVersionDic[abName].IsFirstData;
                    if (needUpload)
                    {
                        MainEntry.Log(MainEntry.LogCategory.Assets,$"上传资源{abName},云端md5{m_CDNVersionDic[abName].MD5},本地md5{m_LocalAssetsVersionDic[abName].MD5}");
                    }
                    return needUpload;
                }
                else
                {
                    MainEntry.LogWarning(MainEntry.LogCategory.Assets,$"资源出现问题==本地没有这个资源,但是云端有这个资源{abName}");
                    return false;
                }
                return true;
            }
            return true;
        }
        
        static CosXml CreateCosXml()
        {
            var dic = SecurityUtil.GetSecretKeyDic();
            CosXmlConfig config = new CosXmlConfig.Builder()
                .SetConnectionTimeoutMs(60000)  //设置连接超时时间，单位毫秒，默认45000ms
                .SetReadWriteTimeoutMs(40000)  //设置读写超时时间，单位毫秒，默认45000ms
                .IsHttps(true)  //设置默认 HTTPS 请求
                .SetAppid("1318826377")
                .SetRegion(dic["region"])
                .Build();

            long durationSecond = 600; //每次请求签名有效时长，单位为秒
            QCloudCredentialProvider qCloudCredentialProvider = new DefaultQCloudCredentialProvider(dic["secretId"], dic["secretKey"], durationSecond);
            return new CosXmlServer(config, qCloudCredentialProvider);
        }

        // 显示上传结果提示窗口
        static void ShowUploadResult()
        {
            string message = $"需上传文件数: {totalFileCount}\n成功上传: {successCount}\n失败上传: {failCount}";
            EditorUtility.DisplayDialog("上传结果", message, "确定");
        }
        


    }
    
    // 自定义上传结果窗口
    public class UploadResultWindow : EditorWindow
    {
        private Vector2 scrollPosition; // 用于记录滚动位置
        private string successLog;
        private string failureLog;
        private string totalTime;

        public void UpdateLog(string successes, string failures, string time)
        {
            successLog = successes;
            failureLog = failures;
            totalTime = time;

            // 强制滚动到底部
            scrollPosition = new Vector2(0, Mathf.Infinity);

            Repaint(); // 更新窗口
        }

        private void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

            // 显示成功上传的文件
            GUILayout.Label("成功上传的文件:", EditorStyles.boldLabel);
            GUILayout.TextArea(successLog, GUILayout.ExpandHeight(false)); // 显示成功日志

            // 显示失败上传的文件
            GUILayout.Label("失败上传的文件:", EditorStyles.boldLabel);
            GUILayout.TextArea(failureLog, GUILayout.ExpandHeight(false)); // 显示失败日志

            // 显示总耗时
            GUILayout.Label(totalTime, EditorStyles.miniLabel);

            GUILayout.EndScrollView(); // 结束滚动视图
        }
    }

    
}

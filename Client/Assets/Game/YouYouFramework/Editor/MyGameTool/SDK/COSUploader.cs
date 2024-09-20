using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using COSXML.CosException;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using COSXML.Utils;
using HybridCLR.Editor;
using Main;
using UnityEngine;
using UnityEditor;
using YouYou;
using Debug = UnityEngine.Debug;

namespace StarForce.Editor
{
    public class COSUploader
    {
        private static CosConfig cosConfig;
        private static StringBuilder successLog = new StringBuilder(); // 用于记录成功的上传状态
        private static StringBuilder failureLog = new StringBuilder(); // 用于记录失败的上传状态
        [MenuItem("YouYouTools/将AB包资源上传到云端", false, 53)]
        static async void UploadAB()
        {
            cosConfig = Resources.Load<CosConfig>("CosConfig");
            CosXml cosXml = CreateCosXml();
            Stopwatch stopwatch = new Stopwatch(); // 用于记录耗时
            try
            {
                stopwatch.Start(); // 开始计时
                // 获取当前的构建平台
                BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;

                // 设置基础文件夹
                string baseFolder = Path.Combine(SettingsUtil.ProjectDir, "AssetBundles", PlayerPrefs.GetString(YFConstDefine.AssetVersion), GetPlatformOption(activeBuildTarget));

                // 递归上传整个文件夹
                Debug.LogError(cosConfig.cosABRoot);
                Debug.LogError(baseFolder);
                await UploadFolderAsync(cosXml, cosConfig.bucket, cosConfig.cosABRoot, baseFolder);
                
                string totalTime = $"所有文件上传完成，总耗时: {stopwatch.Elapsed.TotalSeconds:0.00} 秒";
                UploadResultWindow.ShowWindow(successLog.ToString(), failureLog.ToString(), totalTime);
                // resultLog.AppendLine($"所有文件上传完成，总耗时: {stopwatch.Elapsed.TotalSeconds:0.00} 秒");
                // Debug.LogError(resultLog.ToString());
            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                Debug.LogError("CosClientException: " + clientEx);
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                Debug.LogError("CosServerException: " + serverEx.GetInfo());
            }
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
                string relativePath = filePath.Substring(localFolderPath.Length + 1);
                string uploadStatus;

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                {
                    PutObjectRequest request = new PutObjectRequest(bucket, cosPath + "/" + relativePath, fileStream);
                    request.SetSign(TimeUtils.GetCurrentTime(TimeUnit.Seconds), 600);
                    request.SetCosProgressCallback(delegate (long completed, long total)
                    {
                        Debug.Log($"Progress uploading {relativePath}: {completed * 100.0 / total:0.00}%");
                    });

                    // 使用 Task.Run 来异步执行上传
                    await Task.Run(() =>
                    {
                        PutObjectResult result = cosXml.PutObject(request);
                        if (result.IsSuccessful())
                        {
                            successLog.AppendLine($"{Path.GetFileName(filePath)}      上传状态：成功");
                        }
                        else
                        {
                            failureLog.AppendLine($"{Path.GetFileName(filePath)}      上传状态：<color=red>失败</color>");
                        }
                    });
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

        static CosXml CreateCosXml()
        {
            CosXmlConfig config = new CosXmlConfig.Builder()
            .SetConnectionTimeoutMs(60000)  //设置连接超时时间，单位毫秒，默认45000ms
            .SetReadWriteTimeoutMs(40000)  //设置读写超时时间，单位毫秒，默认45000ms
            .IsHttps(true)  //设置默认 HTTPS 请求
            .SetAppid(cosConfig.appid)
            .SetRegion(cosConfig.region)
            .Build();

            long durationSecond = 600; //每次请求签名有效时长，单位为秒
            QCloudCredentialProvider qCloudCredentialProvider = new DefaultQCloudCredentialProvider(cosConfig.secretId,
              cosConfig.secretKey, durationSecond);

            CosXml cosXml = new CosXmlServer(config, qCloudCredentialProvider);
            return cosXml;
        }
    }
    
    // 自定义上传结果窗口
    public class UploadResultWindow : EditorWindow
    {
        private static string successLog;
        private static string failureLog;
        private static string totalTime;

        public static void ShowWindow(string successes, string failures, string time)
        {
            successLog = successes;
            failureLog = failures;
            totalTime = time;
            UploadResultWindow window = GetWindow<UploadResultWindow>("上传结果");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("成功上传的文件:", EditorStyles.boldLabel);
            GUILayout.TextArea(successLog, GUILayout.Height(200));

            GUILayout.Label("失败上传的文件:", EditorStyles.boldLabel);
            GUILayout.TextArea(failureLog, GUILayout.Height(200));

            GUILayout.Label(totalTime, EditorStyles.miniLabel);
        }
    }
}

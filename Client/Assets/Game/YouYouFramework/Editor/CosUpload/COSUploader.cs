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
using UnityEditor;
using UnityEngine;

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
        private static UploadResultWindow uploadWindow;
        private static Stopwatch stopwatch;
        
        
        public static async void UploadAB()
        {
            cosConfig = Resources.Load<CosConfig>("CosConfig");
            CosXml cosXml = CreateCosXml();
            stopwatch = new Stopwatch(); // 用于记录耗时
            uploadWindow = new UploadResultWindow(); // 创建上传结果窗口
            uploadWindow.Show(); // 显示窗口
            
            try
            {
                stopwatch.Start(); // 开始计时
                // 获取当前的构建平台
                BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;

                // 设置基础文件夹
                string baseFolder = Path.Combine(SettingsUtil.ProjectDir, "AssetBundles", PlayerPrefs.GetString(YFConstDefine.AssetVersion), GetPlatformOption(activeBuildTarget));
                var dic = SecurityUtil.GetSecretKeyDic("editor");
                // 递归上传整个文件夹
                await UploadFolderAsync(cosXml, dic["bucket"], cosConfig.cosABRoot, baseFolder);
                
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
                uploadWindow.Close();
                ShowUploadResult();
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

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    PutObjectRequest request = new PutObjectRequest(bucket, cosPath + "/" + relativePath, fileStream);
                    request.SetSign(TimeUtils.GetCurrentTime(TimeUnit.Seconds), 600);
                    request.SetCosProgressCallback(delegate (long completed, long total)
                    {
                        GameEntry.LogError($"Progress uploading {relativePath}: {completed * 100.0 / total:0.00}%");
                    });

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

        static CosXml CreateCosXml()
        {
            var dic = SecurityUtil.GetSecretKeyDic("editor");
            CosXmlConfig config = new CosXmlConfig.Builder()
                .SetConnectionTimeoutMs(60000)  //设置连接超时时间，单位毫秒，默认45000ms
                .SetReadWriteTimeoutMs(40000)  //设置读写超时时间，单位毫秒，默认45000ms
                .IsHttps(true)  //设置默认 HTTPS 请求
                .SetAppid(cosConfig.appid)
                .SetRegion(dic["region"])
                .Build();

            long durationSecond = 600; //每次请求签名有效时长，单位为秒
            QCloudCredentialProvider qCloudCredentialProvider = new DefaultQCloudCredentialProvider(dic["SecretId"], dic["SecretKey"], durationSecond);
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

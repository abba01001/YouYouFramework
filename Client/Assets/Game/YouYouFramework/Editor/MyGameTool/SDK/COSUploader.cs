using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using COSXML.CosException;
using System;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using COSXML.Utils;
using UnityEditor;
using System.Text;
using HybridCLR.Editor;
using Main;

namespace StarForce.Editor
{
    public class COSUploader
    {
        private static CosConfig cosConfig;
        //[MenuItem("YouYouTools/将打包生成的Version.txt上传到云端", false, 50)]
        //static void UploadVersionInfo ()
        //{
        //    // 通过Resources.Load加载CosConfig实例
        //    cosConfig = Resources.Load<CosConfig>("CosConfig");
        //    // 确保实例非空
        //    if (cosConfig == null)
        //    {
        //        Debug.LogError("CosConfig not found! Make sure it's placed in a Resources folder.");
        //        return;
        //    }
        //    CosXml cosXml = CreateCosXml();
        //    try
        //    {
        //        // 构建 txtFilePath，相对于项目的路径
        //        string txtFileName = "Assets/PackageTool/Version.txt";
        //        string srcPath = System.IO.Path.Combine(Application.dataPath, "..", txtFileName);

        //        if (!File.Exists(srcPath))
        //        {
        //            Debug.Log("Version.txt不存在");
        //            return;
        //        }

        //        PutObjectRequest request = new PutObjectRequest(cosConfig.bucket, cosConfig.cosVersionPath, srcPath);
        //        //设置签名有效时长
        //        request.SetSign(TimeUtils.GetCurrentTime(TimeUnit.Seconds), 600);
        //        //设置进度回调
        //        request.SetCosProgressCallback(delegate (long completed, long total)
        //        {
        //            Debug.Log(String.Format("progress = {0:##.##}%", completed * 100.0 / total));
        //        });
        //        //执行请求
        //        PutObjectResult result = cosXml.PutObject(request);
        //        //对象的 eTag
        //        string eTag = result.eTag;

        //    }
        //    catch (COSXML.CosException.CosClientException clientEx)
        //    {
        //        Debug.LogError("CosClientException: " + clientEx);
        //    }
        //    catch (COSXML.CosException.CosServerException serverEx)
        //    {
        //        //请求失败
        //        Debug.LogError("CosServerException: " + serverEx.GetInfo());
        //    }
        //}

        [MenuItem("YouYouTools/将AB包资源上传到云端", false, 53)]
        static void UploadAB()
        {
            cosConfig = Resources.Load<CosConfig>("CosConfig");
            CosXml cosXml = CreateCosXml();
            try
            {
                // 获取当前的构建平台
                BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
                // 获取项目根目录

                // 设置基础文件夹
                string baseFolder = Path.Combine(SettingsUtil.ProjectDir, "AssetBundles", PlayerPrefs.GetString(YFConstDefine.AssetVersion), GetPlatformOption(activeBuildTarget));
                // 递归上传整个文件夹
                Debug.LogError(cosConfig.cosABRoot);
                Debug.LogError(baseFolder);
                UploadFolder(cosXml, cosConfig.bucket, cosConfig.cosABRoot, baseFolder);
            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                Debug.LogError("CosClientException: " + clientEx);
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                // 请求失败
                Debug.LogError("CosServerException: " + serverEx.GetInfo());
            }
        }

        // 根据构建平台获取对应的 PlatformOption 值
        private static string GetPlatformOption(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                    return "Windows";
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.Android:
                    return "Android";
                // 添加其他平台的映射
                default:
                    return "Windows"; // 默认返回 Windows
            }
        }

        // 递归上传整个文件夹的方法
        static void UploadFolder(CosXml cosXml, string bucket, string cosPath, string localFolderPath)
        {
            // 上传文件
            string[] files = Directory.GetFiles(localFolderPath);
            foreach (string filePath in files)
            {
                string relativePath = filePath.Substring(localFolderPath.Length + 1);
                PutObjectRequest request = new PutObjectRequest(bucket, cosPath + "/" + relativePath, filePath);
                request.SetSign(TimeUtils.GetCurrentTime(TimeUnit.Seconds), 600);
                request.SetCosProgressCallback(delegate (long completed, long total)
                {
                    Debug.Log(String.Format("progress = {0:##.##}%", completed * 100.0 / total));
                });
                PutObjectResult result = cosXml.PutObject(request);
                string eTag = result.eTag;
            }

            // 递归上传子文件夹
            string[] subFolders = Directory.GetDirectories(localFolderPath);
            foreach (string subFolder in subFolders)
            {
                string relativePath = subFolder.Substring(localFolderPath.Length + 1);
                UploadFolder(cosXml, bucket, cosPath + "/" + relativePath, subFolder);
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

            long durationSecond = 600;          //每次请求签名有效时长，单位为秒
            QCloudCredentialProvider qCloudCredentialProvider = new DefaultQCloudCredentialProvider(cosConfig.secretId,
              cosConfig.secretKey, durationSecond);

            CosXml cosXml = new CosXmlServer(config, qCloudCredentialProvider);
            return cosXml;
        }

    }
}

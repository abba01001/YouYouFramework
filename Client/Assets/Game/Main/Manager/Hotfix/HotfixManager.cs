using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Main
{
    public class HotfixManager
    {
        private static AssetBundle hotfixAb;

        public HotfixManager()
        {
            //这里防止热更工程找不到AOT工程的类
            System.Data.AcceptRejectRule acceptRejectRule = System.Data.AcceptRejectRule.None;
            System.Net.WebSockets.WebSocketReceiveResult webSocketReceiveResult = null;
        }
        public async Task Init()
        {
#if EDITORLOAD
            GameObject gameEntry = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Download/Hotfix/GameEntry.prefab");
            UnityEngine.Object.Instantiate(gameEntry);
            return;
#endif
            
            MainEntry.Download.GetAPKVersion(SystemModel.Instance.CurrChannelConfig.APKVersionUrl, null, (result) =>
            {
                // 获取第一行的版本号
                string version = result.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)[0];
                SystemModel.Instance.CurrChannelConfig.SourceVersion = version;
                InitDefaultVersion();
                CheckVersion();
            });

        }

        private void CheckVersion()
        {
            //初始化CDN的VersionFile信息
            MainEntry.Assets.VersionFile.InitCDNVersionFile((string cloudVersion) =>
            {
                MainEntry.LogError(MainEntry.LogCategory.Assets,
                    $"======本地版本{PlayerPrefs.GetString("localVersion")}======云端版本{cloudVersion}");
                //先校验游戏版本

#if UNITY_EDITOR
                PlayerPrefs.SetString("localVersion", cloudVersion); // 替换为实际版本号
                PlayerPrefs.Save();
#else
                DownLoadApk dl = new DownLoadApk();
                if (dl.CheckDownLoadApk(cloudVersion))
                {
                    return;
                }
#endif

                //下载并加载热更程序集
                CheckAndDownload(YFConstDefine.HotfixAssetBundlePath, (string fileUrl) =>
                {
#if !UNITY_EDITOR
                    hotfixAb =
 AssetBundle.LoadFromFile(string.Format("{0}/{1}", Application.persistentDataPath, fileUrl));
                    LoadMetadataForAOTAssemblies();
                    System.Reflection.Assembly.Load(hotfixAb.LoadAsset<TextAsset>("Assembly-CSharp.dll.bytes").bytes);
                    MainEntry.Log(MainEntry.LogCategory.Assets, "Assembly-CSharp.dll加载完毕");
#else
                    hotfixAb = AssetBundle.LoadFromFile(string.Format("{0}/{1}", Application.persistentDataPath,
                        fileUrl));
#endif
                    UnityEngine.Object.Instantiate(hotfixAb.LoadAsset<GameObject>("formcheckversion.prefab"));
                    UnityEngine.Object.Instantiate(hotfixAb.LoadAsset<GameObject>("gameentry.prefab"));
                });
            });
        }
        
        private void InitDefaultVersion()
        {
            if (string.IsNullOrEmpty(PlayerPrefs.GetString("localVersion")))
            {
                PlayerPrefs.SetString("localVersion",SystemModel.Instance.CurrChannelConfig.DefaultVersion);//第一次安装没有版本号，给个默认值
            }
        }
        
        private void CheckAndDownload(string url, Action<string> onComplete)
        {
            bool isEquals = MainEntry.Assets.CheckVersionChangeSingle(url);
            if (isEquals)
            {
                MainEntry.Log(MainEntry.LogCategory.Assets, "资源没变化, 不用重新下载, url==" + url);
                onComplete?.Invoke(url);
            }
            else
            {
                MainEntry.Log(MainEntry.LogCategory.Assets, "资源有更新, 重新下载, url==" + url);
                MainEntry.Download.BeginDownloadSingle(url, onComplete: onComplete);
            }
        }

        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        private static void LoadMetadataForAOTAssemblies()
        {
            //这里补充完泛型，同时也要在AssetBundleSettings里CopyHofixDll里补充进去
            List<string> aotMetaAssemblyFiles = new List<string>()
            {
                "mscorlib.dll",
                "System.dll",
                "System.Core.dll",
                "UniTask.dll",
                "UnityEngine.AndroidJNIModule.dll"
            };
            /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            /// 
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in aotMetaAssemblyFiles)
            {
                byte[] dllBytes = hotfixAb.LoadAsset<TextAsset>(aotDllName + ".bytes").bytes;
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            }
            MainEntry.Log(MainEntry.LogCategory.Assets, "补充元数据Dll加载完毕==" + aotMetaAssemblyFiles.ToJson());
        }
    }

    public class DownLoadApk
    {
        private string apkFileName = "Demo.apk"; // APK 文件名，放在 StreamingAssets 中
        public bool CheckDownLoadApk(string cloudVersion)
        {
            string localVersion = PlayerPrefs.GetString("localVersion", string.Empty);
            if (!string.IsNullOrEmpty(localVersion) && !string.IsNullOrEmpty(cloudVersion))
            {
                string[] part1 = localVersion.Split('.');
                string localApkVersion = part1[0];
                
                string[] part2 = cloudVersion.Split('.');
                string cloudApkVersion = part2[0];

                if (localApkVersion != cloudApkVersion)
                {
                    //直接下载整包
                    MainEntry.Instance.StartCoroutine(DownloadAndInstall(SystemModel.Instance.CurrChannelConfig.ApkUrl,cloudVersion));
                    return true;
                }
            }
            return false;
        }
        
        public IEnumerator DownloadAndInstall(string url,string cloudVersion)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                string tempPath = Path.Combine(Application.persistentDataPath, apkFileName);
                if (File.Exists(tempPath))
                {
                    MainEntry.LogError(MainEntry.LogCategory.Assets, "找到本地安装包,直接安装。");
                    InstallAPKInternal(tempPath);
                }
                
                www.SendWebRequest();
                // 循环检查进度
                while (!www.isDone)
                {
                    //MainEntry.LogError(MainEntry.LogCategory.Assets,$"下载进度: {www.downloadProgress * 100}%");
                    yield return null; // 等待下一帧
                }

                if (www.result == UnityWebRequest.Result.ConnectionError ||
                    www.result == UnityWebRequest.Result.ProtocolError)
                {
                    MainEntry.LogError(MainEntry.LogCategory.Assets,www.error);
                }
                else
                {
                    // 将 APK 数据保存到临时文件
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                    File.WriteAllBytes(tempPath, www.downloadHandler.data);
                    // 调用内部安装方法
                    if (InstallAPKInternal(tempPath))
                    {
                        PlayerPrefs.SetString("localVersion", cloudVersion); // 替换为实际版本号
                        PlayerPrefs.Save();
                    }
                    else
                    {
                        Application.Quit();
                    }
                }
            }
        }

        private bool InstallAPKInternal(string apkPath)
        {
            AndroidJavaClass javaClass = new AndroidJavaClass("com.example.mylibrary.Install");
            return javaClass.CallStatic<bool>("安装apk", apkPath);
        }
    }
}
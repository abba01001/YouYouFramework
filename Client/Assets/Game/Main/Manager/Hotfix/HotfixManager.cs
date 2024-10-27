using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public void Init()
        {
#if EDITORLOAD
            GameObject gameEntry = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Download/Hotfix/GameEntry.prefab");
            UnityEngine.Object.Instantiate(gameEntry);
            return;
#endif
            //初始化CDN的VersionFile信息
            MainEntry.Assets.VersionFile.InitCDNVersionFile(() =>
            {
                //下载并加载热更程序集
                CheckAndDownload(YFConstDefine.HotfixAssetBundlePath, (string fileUrl) =>
                {
#if !UNITY_EDITOR
                    hotfixAb = AssetBundle.LoadFromFile(string.Format("{0}/{1}", Application.persistentDataPath, fileUrl));
                    LoadMetadataForAOTAssemblies();
                    System.Reflection.Assembly.Load(hotfixAb.LoadAsset<TextAsset>("Assembly-CSharp.dll.bytes").bytes);
                    MainEntry.Log(MainEntry.LogCategory.Assets, "Assembly-CSharp.dll加载完毕");
#else
                    hotfixAb = AssetBundle.LoadFromFile(string.Format("{0}/{1}", Application.persistentDataPath, fileUrl));
#endif
                    UnityEngine.Object.Instantiate(hotfixAb.LoadAsset<GameObject>("formcheckversion.prefab"));
                    UnityEngine.Object.Instantiate(hotfixAb.LoadAsset<GameObject>("gameentry.prefab"));
                });
            });

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
}
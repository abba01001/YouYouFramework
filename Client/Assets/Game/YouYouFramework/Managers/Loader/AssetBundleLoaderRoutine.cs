using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// 资源包加载器
    /// </summary>
    public class AssetBundleLoaderRoutine
    {
        /// <summary>
        /// 当前的资源包信息
        /// </summary>
        public VersionFileEntity CurrAssetBundleInfo { get; private set; }

        /// <summary>
        /// 资源包创建请求
        /// </summary>
        public AssetBundleCreateRequest CurrAssetBundleCreateRequest { get; private set; }

        /// <summary>
        /// 加载资源包完毕
        /// </summary>
        public Action<AssetBundle> OnLoadAssetBundleComplete;

        /// <summary>
        /// 资源包创建请求更新
        /// </summary>
        public Action<float> OnAssetBundleCreateUpdate;

        /// <summary>
        /// 资源包下载请求更新（边玩边下载才会触发）
        /// </summary>
        public Action<float> OnAssetBundleDownloadUpdate;

        #region LoadAssetBundle 加载资源包
        public void LoadAssetBundleAsync(string assetBundlePath)
        {
            CurrAssetBundleInfo = MainEntry.Assets.VersionFile.GetVersionFileEntity(assetBundlePath);

            //检查文件在可写区是否存在
            bool isExistsInLocal = File.Exists(string.Format("{0}/{1}", Application.persistentDataPath, assetBundlePath));
            if (isExistsInLocal)
            {
                if (CurrAssetBundleInfo.IsEncrypt)
                {
                    //可写区加载, 需要解密
                    byte[] buffer = IOUtil.GetFileBuffer(string.Format("{0}/{1}", Application.persistentDataPath, assetBundlePath));
                    if (buffer != null)
                    {
                        buffer = SecurityUtil.Xor(buffer);
                        CurrAssetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(buffer);
                        return;
                    }
                }
                else
                {
                    //可写区加载, 不用解密
                    CurrAssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(string.Format("{0}/{1}", Application.persistentDataPath, assetBundlePath));
                }
            }
            else
            {
                //如果可写区没有 那么就从CDN下载
                MainEntry.Download.BeginDownloadSingle(assetBundlePath, (url, currSize, progress) =>
                {
                    //YouYou.GameEntry.LogError(progress);
                    OnAssetBundleDownloadUpdate?.Invoke(progress);
                }, (string fileUrl) =>
                {
                    //下载完毕，从可写区加载
                    LoadAssetBundleAsync(assetBundlePath);
                });
            }

        }
        public static AssetBundle LoadAssetBundle(string assetBundlePath)
        {
            VersionFileEntity assetBundleInfo = MainEntry.Assets.VersionFile.GetVersionFileEntity(assetBundlePath);

            //检查文件在可写区是否存在
            bool isExistsInLocal = File.Exists(string.Format("{0}/{1}", Application.persistentDataPath, assetBundlePath));

            if (isExistsInLocal)
            {
                if (assetBundleInfo.IsEncrypt)
                {
                    //可写区加载, 需要解密
                    byte[] buffer = IOUtil.GetFileBuffer(string.Format("{0}/{1}", Application.persistentDataPath, assetBundlePath));
                    if (buffer != null)
                    {
                        buffer = SecurityUtil.Xor(buffer);
                        return AssetBundle.LoadFromMemory(buffer);
                    }
                }
                else
                {
                    //可写区加载, 不用解密
                    return AssetBundle.LoadFromFile(string.Format("{0}/{1}", Application.persistentDataPath, assetBundlePath));
                }
            }
            GameEntry.LogError(LogCategory.Loader, "本地没有该资源, 或许要去服务端下载==" + assetBundlePath);
            return null;
        }

        #endregion

        public static AssetBundleLoaderRoutine Create()
        {
            AssetBundleLoaderRoutine assetBundleLoaderRoutine = MainEntry.ClassObjectPool.Dequeue<AssetBundleLoaderRoutine>();
            return assetBundleLoaderRoutine;
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            CurrAssetBundleCreateRequest = null;
        }

        /// <summary>
        /// 更新
        /// </summary>
        internal void OnUpdate()
        {
            UpdateAssetBundleCreateRequest();
        }

        #region UpdateAssetBundleCreateRequest 更新资源包请求
        /// <summary>
        /// 更新资源包请求
        /// </summary>
        private void UpdateAssetBundleCreateRequest()
        {
            if (CurrAssetBundleCreateRequest == null) return;
            if (CurrAssetBundleCreateRequest.isDone)
            {
                AssetBundle assetBundle = CurrAssetBundleCreateRequest.assetBundle;
                if (assetBundle != null)
                {
                    //GameEntry.Log(LogCategory.Loader, "资源包=>{0} 加载完毕", m_CurrAssetBundleInfo.AssetBundleName);
                }
                else
                {
                    GameEntry.LogError(LogCategory.Loader, "资源包=>{0} 加载失败", CurrAssetBundleInfo.AssetBundleName);
                }
                Reset();//一定要早点Reset
                MainEntry.ClassObjectPool.Enqueue(this);
                OnLoadAssetBundleComplete?.Invoke(assetBundle);
            }
            else
            {
                //加载进度
                OnAssetBundleCreateUpdate?.Invoke(CurrAssetBundleCreateRequest.progress);
            }
        }
        #endregion
    }
}
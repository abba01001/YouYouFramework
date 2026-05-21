using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Main;
using UnityEngine;
using UnityEngine.U2D;
using YooAsset;

namespace GameScripts
{
    using Object = UnityEngine.Object;


    using YooAsset;
    
    /// <summary>
    /// 资源加载 管理器
    /// </summary>
    public class LoaderManager
    {
        public ResourcePackage DefaultPackage { get; private set; }
    
        public LoaderManager()
        {
            DefaultPackage = YooAssets.GetPackage("DefaultPackage");
    
            //加载时间最短, 但加载时帧率下降最严重
            Application.backgroundLoadingPriority = ThreadPriority.High;
        }
    
        /// <summary>
        /// 异步加载主资源，自动加载依赖
        /// 注意: 如果这个资源没有打AB包, 则无法加载
        /// </summary>
        /// <param name="target">依赖的游戏物体， 它销毁时会触发引用计数减少</param>
        public async UniTask<T> LoadMainAssetAsync<T>(string assetFullPath, GameObject target) where T : Object
        {
            if (target == null)
            {
                Debugger.LogError(LogCategory.Loader, "依赖的游戏物体不可为空");
                return null;
            }
    
            var op = DefaultPackage.LoadAssetAsync(assetFullPath);
            await op;
            AssetReleaseHandle.Add(op, target);
            return op.AssetObject as T;
        }
        
        public async UniTask<T> LoadMainAssetAsync<T>(string assetFullPath) where T : Object
        {
            var op = DefaultPackage.LoadAssetAsync(assetFullPath);
            await op;
            return op.AssetObject as T;
        }
    
        /// <summary>
        /// 同步加载主资源, 自动加载依赖
        /// 注意: 如果这个资源没有打AB包, 则无法加载
        /// 注意: 微信小游戏不支持同步加载
        /// </summary>
        /// <param name="target">依赖的游戏物体， 它销毁时会触发引用计数减少</param>
        public T LoadMainAsset<T>(string assetFullPath, GameObject target) where T : Object
        {
            if (target == null)
            {
                Debugger.LogError(LogCategory.Loader, "依赖的游戏物体不可为空");
                return null;
            }
            
            var op = DefaultPackage.LoadAssetSync(assetFullPath);
            AssetReleaseHandle.Add(op, target);
            return op.AssetObject as T;
        }
    
        public T LoadMainAsset<T>(string assetFullPath) where T : Object
        {
            var op = DefaultPackage.LoadAssetSync(assetFullPath);
            return op.AssetObject as T;
        }
        
        // 缓存字典
        private readonly Dictionary<string, AssetHandle> _atlasCache = new Dictionary<string, AssetHandle>();
        public async UniTask<SpriteAtlas> LoadAtlasAsync(string assetFullPath)
        {
            if (_atlasCache.TryGetValue(assetFullPath, out var handle))
            {
                return handle.AssetObject as SpriteAtlas;
            }
            AssetHandle op = DefaultPackage.LoadAssetAsync<SpriteAtlas>(assetFullPath);
            await op.ToUniTask();
            if (op.Status == EOperationStatus.Succeed)
            {
                _atlasCache[assetFullPath] = op;
                return op.AssetObject as SpriteAtlas;
            }
            Debugger.LogError($"图集加载失败: {assetFullPath}");
            return null;
        }
    }
}
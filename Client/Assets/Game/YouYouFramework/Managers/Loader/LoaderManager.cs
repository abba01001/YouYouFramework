using Main;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;

namespace YouYou
{
    /// <summary>
    /// 资源加载 管理器
    /// </summary>
    public class LoaderManager
    {
        /// <summary>
        /// 资源依赖信息 管理器
        /// </summary>
        public AssetInfoManager AssetInfo { get; private set; }

        /// <summary>
        /// 资源包加载器链表
        /// </summary>
        private LinkedList<AssetBundleLoaderRoutine> m_AssetBundleLoaderList;

        /// <summary>
        /// 资源加载器链表
        /// </summary>
        private LinkedList<AssetLoaderRoutine> m_AssetLoaderList;

        public LoaderManager()
        {
            AssetInfo = new AssetInfoManager();
            m_AssetBundleLoaderList = new LinkedList<AssetBundleLoaderRoutine>();
            m_AssetLoaderList = new LinkedList<AssetLoaderRoutine>();
        }
        internal void Init()
        {
            Application.backgroundLoadingPriority = ThreadPriority.High;
        }
        internal void OnUpdate()
        {
            for (LinkedListNode<AssetBundleLoaderRoutine> curr = m_AssetBundleLoaderList.First; curr != null; curr = curr.Next)
            {
                curr.Value.OnUpdate();
            }

            for (LinkedListNode<AssetLoaderRoutine> curr = m_AssetLoaderList.First; curr != null; curr = curr.Next)
            {
                curr.Value.OnUpdate();
            }
        }


        #region LoadAssetBundle 加载资源包
        /// <summary>
        /// 加载中的Bundle
        /// </summary>
        private TaskGroup AssetBundleTaskGroup = new TaskGroup();
        /// <summary>
        /// 加载资源包
        /// </summary>
        public void LoadAssetBundleAction(string assetbundlePath, Action<AssetBundle> onComplete = null, Action<float> onUpdate = null, Action<float> onDownloadUpdate = null)
        {
            //使用TaskGroup, 加入异步加载队列, 防止高并发导致的重复加载
            AssetBundleTaskGroup.AddTask((taskRoutine) =>
            {
                //判断资源包是否存在于AssetBundlePool
                AssetBundleReferenceEntity assetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetbundlePath);
                if (assetBundleEntity != null)
                {
                    //GameEntry.Log("资源包在资源池中存在 从资源池中加载AssetBundle");
                    onComplete?.Invoke(assetBundleEntity.Target);
                    taskRoutine.Leave();
                    return;
                }

                AssetBundleLoaderRoutine loadRoutine = AssetBundleLoaderRoutine.Create();

                //加入链表开始Update()
                m_AssetBundleLoaderList.AddLast(loadRoutine);
                //资源包加载 监听回调
                loadRoutine.OnAssetBundleCreateUpdate = onUpdate;
                loadRoutine.OnAssetBundleDownloadUpdate = onDownloadUpdate;
                loadRoutine.OnLoadAssetBundleComplete = (AssetBundle assetbundle) =>
                {
                    //资源包注册到资源池
                    AssetBundleReferenceEntity.Create(assetbundlePath, assetbundle);

                    taskRoutine.Leave();
                    onComplete?.Invoke(assetbundle);

                    m_AssetBundleLoaderList.Remove(loadRoutine);
                };
                //加载资源包
                loadRoutine.LoadAssetBundleAsync(assetbundlePath);
            });
            AssetBundleTaskGroup.Run();
        }

        public UniTask<AssetBundle> LoadAssetBundleAsync(string assetbundlePath, Action<float> onUpdate = null, Action<float> onDownloadUpdate = null)
        {
            var task = new UniTaskCompletionSource<AssetBundle>();
            LoadAssetBundleAction(assetbundlePath, (AssetBundle bundle) =>
            {
                task.TrySetResult(bundle);
            }, onUpdate, onDownloadUpdate);
            return task.Task;
        }
        /// <summary>
        /// 加载主资源包和依赖资源包
        /// </summary>
        public async UniTask<AssetBundle> LoadAssetBundleMainAndDependAsync(string assetFullName, Action<float> onUpdate = null, Action<float> onDownloadUpdate = null)
        {
            AssetInfoEntity assetEntity = GameEntry.Loader.AssetInfo.GetAssetEntity(assetFullName);
            if (assetEntity == null) return null;

            //加载这个资源所依赖的资源包
            List<string> dependsAssetList = assetEntity.DependsAssetBundleList;
            for (int i = 0; i < dependsAssetList.Count; i++)
            {
                await GameEntry.Loader.LoadAssetBundleAsync(dependsAssetList[i]);
            }

            //加载主资源包
            AssetBundle m_MainAssetBundle = await GameEntry.Loader.LoadAssetBundleAsync(assetEntity.AssetBundleFullPath, onUpdate, onDownloadUpdate);
            if (m_MainAssetBundle == null)
            {
                GameEntry.LogError(LogCategory.Loader, "MainAssetBundle not exists " + assetEntity.AssetFullPath);
                return null;
            }
            return m_MainAssetBundle;
        }

        public AssetBundle LoadAssetBundleMainAndDepend(string assetFullName)
        {
            AssetInfoEntity assetEntity = GameEntry.Loader.AssetInfo.GetAssetEntity(assetFullName);
            if (assetEntity == null) return null;

            //加载这个资源所依赖的资源包
            List<string> dependsAssetList = assetEntity.DependsAssetBundleList;
            for (int i = 0; i < dependsAssetList.Count; i++)
            {
                GameUtil.LogError($"资源路径========={dependsAssetList[i]}");
                GameEntry.Loader.LoadAssetBundle(dependsAssetList[i]);
            }

            //加载主资源包
            AssetBundle m_MainAssetBundle = GameEntry.Loader.LoadAssetBundle(assetEntity.AssetBundleFullPath);
            if (m_MainAssetBundle == null)
            {
                GameEntry.LogError(LogCategory.Loader, "MainAssetBundle not exists " + assetEntity.AssetFullPath);
                return null;
            }
            return m_MainAssetBundle;
        }
        public AssetBundle LoadAssetBundle(string assetbundlePath)
        {
            //判断资源包是否存在于AssetBundlePool
            AssetBundleReferenceEntity assetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetbundlePath);
            if (assetBundleEntity != null)
            {
                //GameEntry.Log("资源包在资源池中存在 从资源池中加载AssetBundle");
                return assetBundleEntity.Target;
            }

            //如果这个AssetBundle在异步加载中，则直接堵塞主线程，返回Request.assetBundle
            for (LinkedListNode<AssetBundleLoaderRoutine> curr = m_AssetBundleLoaderList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.CurrAssetBundleInfo.AssetBundleName == assetbundlePath)
                {
                    return curr.Value.CurrAssetBundleCreateRequest.assetBundle;
                }
            }

            //加载资源包
            AssetBundle assetbundle = AssetBundleLoaderRoutine.LoadAssetBundle(assetbundlePath);

            //资源包注册到资源池
            AssetBundleReferenceEntity.Create(assetbundlePath, assetbundle);
            return assetbundle;
        }
        #endregion

        #region LoadAsset 从资源包中加载资源
        /// <summary>
        /// 加载中的Asset
        /// </summary>
        private TaskGroup AssetTaskGroup = new TaskGroup();
        /// <summary>
        /// 异步加载
        /// </summary>
        public void LoadAssetAction(string assetFullPath, AssetBundle assetBundle, Action<float> onUpdate = null, Action<AssetReferenceEntity> onComplete = null)
        {
            //使用TaskGroup, 加入异步加载队列, 防止高并发导致的重复加载
            AssetTaskGroup.AddTask((taskRoutine) =>
            {
                //从分类资源池(AssetPool)中查找主资源
                AssetReferenceEntity referenceEntity = GameEntry.Pool.AssetPool.Spawn(assetFullPath);
                if (referenceEntity != null)
                {
                    onComplete?.Invoke(referenceEntity);
                    //YouYou.GameEntry.LogError("从分类资源池加载" + assetEntity.ResourceName);
                    return;
                }

                AssetLoaderRoutine routine = AssetLoaderRoutine.Create();

                //加入链表开始循环
                m_AssetLoaderList.AddLast(routine);

                //资源加载 进行中 回调
                routine.OnAssetUpdate = onUpdate;
                //资源加载 结果 回调
                routine.OnLoadAssetComplete = (Object obj) =>
                {
                    taskRoutine.Leave();
                    referenceEntity = AssetReferenceEntity.Create(assetFullPath, obj);
                    onComplete?.Invoke(referenceEntity);

                    //结束循环 回池
                    m_AssetLoaderList.Remove(routine);
                };
                //加载资源
                routine.LoadAssetAsync(assetFullPath, assetBundle);
            });
            AssetTaskGroup.Run();
        }
        public UniTask<AssetReferenceEntity> LoadAssetAsync(string assetFullPath, AssetBundle assetBundle, Action<float> onUpdate = null)
        {
            var task = new UniTaskCompletionSource<AssetReferenceEntity>();
            LoadAssetAction(assetFullPath, assetBundle, onUpdate, (AssetReferenceEntity assetReferenceEntity) =>
            {
                task.TrySetResult(assetReferenceEntity);
            });
            return task.Task;
        }

        public AssetReferenceEntity LoadAsset(string assetFullPath, AssetBundle assetBundle)
        {
            //如果这个Asset在异步加载中，则直接堵塞主线程，返回Request.asset
            for (LinkedListNode<AssetLoaderRoutine> curr = m_AssetLoaderList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.CurrAssetName == assetFullPath)
                {
                    return AssetReferenceEntity.Create(assetFullPath, curr.Value.CurrAssetBundleRequest.asset);
                }
            }
            return AssetReferenceEntity.Create(assetFullPath, assetBundle.LoadAsset(assetFullPath));
        }
        #endregion

        #region LoadMainAsset 加载主资源(自动加载依赖)
        /// <summary>
        /// 异步加载主资源，自动加载依赖
        /// </summary>
        /// <param name="target">依赖的游戏物体， 它销毁时会触发引用计数减少</param>
        public async UniTask<T> LoadMainAssetAsync<T>(string assetFullPath, GameObject target, Action<float> onUpdate = null, Action<float> onDownloadUpdate = null) where T : Object
        {
            if (target == null)
            {
                GameEntry.LogError(LogCategory.Loader, "依赖的游戏物体不可为空");
                return null;
            }
            AssetReferenceEntity referenceEntity = await LoadMainAssetAsync(assetFullPath, onUpdate, onDownloadUpdate);
            AutoReleaseHandle.Add(referenceEntity, target);
            return referenceEntity.Target as T;
        }
        /// <summary>
        /// 异步加载主资源，自动加载依赖， 注意：这个方法需要自己调用AssetReferenceEntity.ReferenceAdd去管理引用计数
        /// </summary>
        public async UniTask<AssetReferenceEntity> LoadMainAssetAsync(string assetFullPath, Action<float> onUpdate = null, Action<float> onDownloadUpdate = null)
        {
            //从分类资源池(AssetPool)中查找主资源
            AssetReferenceEntity referenceEntity = GameEntry.Pool.AssetPool.Spawn(assetFullPath);
            if (referenceEntity != null)
            {
                //GameEntry.Log(LogCategory.Loader, "从分类资源池加载" + referenceEntity.AssetFullPath);
                return referenceEntity;
            }

#if EDITORLOAD && UNITY_EDITOR
            referenceEntity = AssetReferenceEntity.Create(assetFullPath, UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetFullPath));
#else
            //加载主资源包和依赖资源包
            AssetBundle mainAssetBundle = await GameEntry.Loader.LoadAssetBundleMainAndDependAsync(assetFullPath, onUpdate, onDownloadUpdate);

            //加载主资源
            referenceEntity = await GameEntry.Loader.LoadAssetAsync(assetFullPath, mainAssetBundle);
#endif

            if (referenceEntity == null) GameEntry.LogError(LogCategory.Loader, "资源加载失败,assetFullPath=={0}", assetFullPath);
            return referenceEntity;
        }

        /// <summary>
        /// 同步加载主资源, 自动加载依赖
        /// </summary>
        /// <param name="target">依赖的游戏物体， 它销毁时会触发引用计数减少</param>
        public T LoadMainAsset<T>(string assetFullPath, GameObject target) where T : Object
        {
            if (target == null)
            {
                GameEntry.LogError(LogCategory.Loader, "依赖的游戏物体不可为空");
                return null;
            }
            AssetReferenceEntity referenceEntity = LoadMainAsset(assetFullPath);
            AutoReleaseHandle.Add(referenceEntity, target);
            return referenceEntity.Target as T;
        }
        /// <summary>
        /// 同步加载主资源, 自动加载依赖， 注意：这个方法需要自己调用AssetReferenceEntity.ReferenceAdd去管理引用计数
        /// </summary>
        public AssetReferenceEntity LoadMainAsset(string assetFullPath)
        {
            //从分类资源池(AssetPool)中查找主资源
            AssetReferenceEntity referenceEntity = GameEntry.Pool.AssetPool.Spawn(assetFullPath);
            if (referenceEntity != null)
            {
                //GameEntry.Log(LogCategory.Loader, "从分类资源池加载" + referenceEntity.AssetFullPath);
                return referenceEntity;
            }

#if EDITORLOAD && UNITY_EDITOR
            referenceEntity = AssetReferenceEntity.Create(assetFullPath, UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(assetFullPath));
#else
            //加载主资源包和依赖资源包
            AssetBundle mainAssetBundle = GameEntry.Loader.LoadAssetBundleMainAndDepend(assetFullPath);

            //加载主资源
            referenceEntity = GameEntry.Loader.LoadAsset(assetFullPath, mainAssetBundle);
#endif

            if (referenceEntity == null) GameEntry.LogError(LogCategory.Loader, "资源加载失败,assetFullPath=={0}", assetFullPath);
            return referenceEntity;
        }
        #endregion
    }
}
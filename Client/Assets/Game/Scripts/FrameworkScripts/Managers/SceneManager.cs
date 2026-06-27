using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Main;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using YooSceneHandle = YooAsset.SceneHandle;

namespace GameScripts
{
    public class SceneManager
    {
        public Action<float> LoadingUpdateAction;
        private readonly List<SceneLoaderRoutine> m_SceneLoaders = new();
        private bool m_IsLoading;

        /// <summary>
        /// 加载场景组
        /// </summary>
        public async UniTask LoadSceneAsync(string sceneName, int sceneLoadCount = -1)
        {
            if (m_IsLoading)
            {
                Debugger.LogError(LogCategory.Framework, $"场景正在加载中: {sceneName}");
                return;
            }
            LoadingUpdateAction?.Invoke(0f);
            m_IsLoading = true;
            try
            {
                // 1. 卸载旧场景
                if (m_SceneLoaders.Count > 0)
                {
                    Debugger.BeginProfile("LoadSceneAsync",$"开始卸载旧场景{m_SceneLoaders[0].Scene.name}==>>");
                    await UniTask.WhenAll(m_SceneLoaders.Select(r => r.UnloadSceneAsync()));
                    Debugger.EndProfile("LoadSceneAsync",$"卸载旧场景{m_SceneLoaders[0].Scene.name}完成==>>");
                    m_SceneLoaders.Clear();
                }

                // 2. 清理资源
                Debugger.BeginProfile("LoadSceneAsync","开始卸载旧场景资源==>>");
                await GameEntry.Loader.DefaultPackage.UnloadUnusedAssetsAsync();
                Debugger.EndProfile("LoadSceneAsync","卸载旧场景资源完成==>>");

                // 3. 并行加载新场景
                var entities = GameEntry.Config.Sys_SceneDBModel.GetListByGroupName(sceneName, sceneLoadCount);
                var progressDict = new Dictionary<string, float>();
                
                // 构建任务流
                var loadTasks = entities.Select(entity => 
                {
                    var routine = new SceneLoaderRoutine();
                    m_SceneLoaders.Add(routine);
                    
                    // 使用 IProgress 实时报告进度
                    var progress = new Progress<float>(p => 
                    {
                        progressDict[entity.AssetFullPath] = p;
                        float total = progressDict.Values.Sum() / entities.Count;
                        LoadingUpdateAction?.Invoke(GameUtil.ConvertPercent(total,0,0.95f));
                    });
                    return routine.LoadSceneAsync(entity.AssetFullPath, progress);
                });
                Debugger.BeginProfile("LoadSceneAsync",$"开始加载新场景==>>");
                await UniTask.WhenAll(loadTasks);
                Debugger.EndProfile("LoadSceneAsync",$"加载新场景{m_SceneLoaders[0].Scene.name}完成==>>");

                // 4. 完成后激活主场景
                if (m_SceneLoaders.Count > 0)
                {
                    UnityEngine.SceneManagement.SceneManager.SetActiveScene(m_SceneLoaders[0].Scene);
                    GameEntry.Pool.GameObjectPool.InitScenePool();
                }
                HandleFinalProgress().Forget();
            }
            finally
            {
                m_IsLoading = false;
            }
        }
        
        async UniTask HandleFinalProgress()
        {
            try
            {
                for (int i = 0; i <= 4; i++)
                {
                    int index = i;
                    float percent = 0.95f + (index * 0.01f);
                    LoadingUpdateAction?.Invoke(percent);
                    await UniTask.Delay(TimeSpan.FromMilliseconds(200), DelayType.Realtime);
                }
            }
            finally
            {
                // 确保无论发生什么，最后一步必须强制设为 1f
                LoadingUpdateAction?.Invoke(1f);
            }   
        }
    }

    public class SceneLoaderRoutine
    {   
        private YooSceneHandle m_Handle;
        public string SceneFullPath { get; private set; } // 记录路径
        public Scene Scene 
        {
            get 
            {
                // 通过路径在当前已加载场景中查找
                return UnityEngine.SceneManagement.SceneManager.GetSceneByPath(SceneFullPath);
            }
        }
        public async UniTask LoadSceneAsync(string path, IProgress<float> progress)
        {
            SceneFullPath = path;
            m_Handle = GameEntry.Loader.DefaultPackage.LoadSceneAsync(path, LoadSceneMode.Additive);
            
            // 实时轮询 YooAsset 的进度
            while (!m_Handle.IsDone)
            {
                progress?.Report(m_Handle.Progress);
                await UniTask.Yield();
            }
            progress?.Report(1.0f);
        }
        
        // public async UniTask LoadSceneAsync(string path, IProgress<float> progress)
        // {
        //     SceneFullPath = path;
        //
        //     // 注意：YooAsset 默认可能直接激活。
        //     // 确保你的 YooAsset 配置（例如 ResourcePackage.LoadSceneAsync）
        //     // 有相应的参数设置 SuspendLoad = true
        //     var handle = GameEntry.Loader.DefaultPackage.LoadSceneAsync(path, LoadSceneMode.Additive);
        //
        //     // 1. 等待直到加载到 90% (即加载完成，但未激活)
        //     await UniTask.WaitUntil(() => handle.Status == EOperationStatus.Succeed || handle.Progress >= 0.9f);
        //
        //     // 2. 此时场景已在内存中，但 Awake 未执行。
        //     // 设置为 ActiveScene (至关重要)
        //     handle.ActivateScene(); 
        //
        //     // 3. 真正解除挂起，触发 Awake
        //     handle.UnSuspend(); 
        //
        //     await UniTask.WaitUntil(() => handle.IsDone);
        //     progress?.Report(1.0f);
        // }

        public async UniTask UnloadSceneAsync()
        {
            if (m_Handle != null && m_Handle.IsValid)
            {
                await m_Handle.UnloadAsync();
            }
        }
    }
}
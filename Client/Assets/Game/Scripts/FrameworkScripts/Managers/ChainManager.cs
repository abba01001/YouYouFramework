using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Main;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameScripts
{
    #region 任务链接口
    //链式任务接口
    public interface IChainTask
    {
        UniTask ExecuteAsync();
    }
    
    
    public class ResourceTaskChain : IChainTask
    {
        public ResourceTaskChain(){}
        public async UniTask ExecuteAsync()
        {
            Debugger.LogError("开始执行111ResourceTaskChain");
            await GameEntry.UI.OpenUIForm<FormSetting>();
            Debugger.LogError("开始执行222ResourceTaskChain");
            await GameEntry.UI.GetCloseTask<FormSetting>();
            Debugger.LogError("结束执行333ResourceTaskChain");
            // await 一些操作
        }
    }
    
    public class ResourceTaskChain222 : IChainTask
    {
        public ResourceTaskChain222(){}
        public async UniTask ExecuteAsync()
        {
            Debugger.LogError("开始执行111ResourceTaskChain222");
            await GameEntry.UI.OpenUIForm<FormSetting>();
            Debugger.LogError("开始执行222ResourceTaskChain222");
            await GameEntry.UI.GetCloseTask<FormSetting>();
            Debugger.LogError("开始执行333ResourceTaskChain222");
            // await 一些操作
        }
    }
    #endregion

    [MonoSingletonPath("[Singleton]/ChainManager")]
    public class ChainManager : MonoBehaviour, ISingleton
    {
        #region 内部数据定义
        [Serializable]
        public class TaskNode
        {
            public int Priority;
            [ReadOnly] public string TaskName; // 新增：显示任务名
            [HideInInspector] public IChainTask ChainNode; // 存放任务链对象
            [ReadOnly] public bool IsProcessing = false;
        }
        #endregion

        [ShowInInspector, Title("Pending Task Chain"), ReadOnly] private List<TaskNode> _tasks = new List<TaskNode>();
        [ShowInInspector, Title("Active Task"), ReadOnly] private TaskNode _currentTask;
        private bool _isDirty = false;

        public static ChainManager Instance => MonoSingletonProperty<ChainManager>.Instance;

        public void OnSingletonInit()
        {
            Init();
        }
        public void OnSingletonDispose() { }

        public void Init()
        {
            Debugger.Log("ChainManager (Merged) Initialized.");
        }

        private void LateUpdate()
        {
            if (_isDirty)
            {
                _isDirty = false;
                _tasks.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }
        }

        #region 外部调用接口 (API)
        public void CheckEnableChains()
        {
            if (_currentTask == null)
            {
                ExecuteNext();
            }
        }
        
        public void AddTask(IChainTask node, int priority = 999)
        {
            string taskName = name ?? node.GetType().Name;
    
            ScheduleTask(new TaskNode 
            { 
                ChainNode = node, 
                Priority = priority, 
                TaskName = taskName 
            });
        }
        #endregion

        #region 队列核心逻辑

        private void ScheduleTask(TaskNode taskNode)
        {
            if (taskNode == null) return;
            _tasks.Add(taskNode);
            _isDirty = true;
        }

        private void ExecuteNext()
        {
            if (_tasks.Count == 0)
            {
                _currentTask = null;
                return;
            }

            _currentTask = _tasks[0];
            _tasks.RemoveAt(0);
            _currentTask.IsProcessing = true;

            ExecuteTaskAsync(_currentTask.ChainNode).Forget();
        }
        
        private async UniTaskVoid ExecuteTaskAsync(IChainTask node)
        {
            try
            {
                await node.ExecuteAsync(); // 此时代码会在这里“挂起”，直到任务完成
            // 解决竞态条件：确保任务链在彻底结束（包含底层的UI列表移除、对象池回收）之后，
            // 再进入下一轮任务检查，防止前一个任务的清理动作还未完成就被下一个任务抢占。
                await UniTask.NextFrame();
            }
            catch (Exception ex)
            {
                Debugger.LogError($"Interaction Task Execution Failed: {ex.Message}");
            }
            finally
            {
                CompleteCurrentAndContinue(); // 无论成功与否，都要推进队列，防止卡死
            }
        }

        private void CompleteCurrentAndContinue()
        {
            _currentTask = null;
            ExecuteNext();
        }
        #endregion
    }
}
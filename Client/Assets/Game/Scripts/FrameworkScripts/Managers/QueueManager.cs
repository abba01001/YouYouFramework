using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Main;
using Sirenix.OdinInspector;
using UniRx;

namespace GameScripts
{
    public enum UIActionType
    {
        ShowUI,
        HideUI,
    }

    [MonoSingletonPath("[Singleton]/QueueManager")]
    public class QueueManager : MonoBehaviour, ISingleton
    {
        #region 内部数据定义
        public enum TaskType
        {
            WaitTime,       // 等待固定时间
            WaitEvent,      // 等待特定事件名触发
            WaitPopup,      // 等待弹窗关闭
            SequenceTween,  // 执行 DOTween 并等待结束
            CommonAction    // 仅执行普通逻辑（不阻塞）
        }

        [Serializable]
        public class QueueTask
        {
            [GUIColor(0.8f, 1f, 0.8f)] // 绿色标出类型
            public TaskType Type;
            public int Priority;
            
            [ShowIf("@Type != TaskType.SequenceTween")]
            public string Name;          
            
            [ShowIf("@Type == TaskType.WaitEvent")]
            public string CloseEvent;    
            
            [ShowIf("@Type == TaskType.WaitTime")]
            public float Duration;       

            // Action 和 Func 无法在 Inspector 序列化显示，必须隐藏，否则会导致列表渲染出错
            [HideInInspector] public Action Action;        
            [HideInInspector] public Func<Tween> TweenCall;
            
            public bool CanTouch;
            
            [ReadOnly] public bool IsProcessing = false;
        }
        #endregion

        // 使用 ShowInInspector 确保 Odin 能显示私有列表
        [ShowInInspector, Title("Wait Queue"), ReadOnly] private List<QueueTask> _queueList = new List<QueueTask>();
        // 显示当前正在跑的任务
        [ShowInInspector, Title("Active Task"), ReadOnly]
        private QueueTask _currentTask;
        private int _taskIdCounter = 0;

        // 标记队列是否变动，用于 LateUpdate 统一处理
        private bool _isDirty = false;

        public static QueueManager Instance => MonoSingletonProperty<QueueManager>.Instance;

        public void OnSingletonInit()
        {
            Init();
        }
        public void OnSingletonDispose() { }

        public void Init()
        {
            Debugger.Log("QueueManager (Merged) Initialized.");
            // 监听全局弹窗关闭事件（假设你已有此事件系统）
            GameEntry.Event.AddEventListener(Constants.EventName.PopupAction, OnPopupAction);
            GameEntry.Event.AddEventListener(Constants.EventName.EventMessage, OnEventMessage);
        }

        private void LateUpdate()
        {
            // 在每一帧末尾统一排序并处理任务，解决同一帧多次入队的问题
            if (_isDirty)
            {
                _isDirty = false;
                _queueList.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                
                if (_currentTask == null)
                {
                    ProcessNext();
                }
            }
            
            // 增加心跳监控：如果当前任务为空，但队列里有任务，说明卡死了
            if (_currentTask == null && _queueList.Count > 0)
            {
                Debugger.LogWarning("QueueManager stalled, self-healing...");
                ProcessNext();
            }
        }

        #region 外部调用接口 (API)

        // 1. 添加 Tween 任务
        public void AddTweenTask(Func<Tween> tweenFunc, int priority = 999, bool canTouch = true)
        {
            Enqueue(new QueueTask { Type = TaskType.SequenceTween, TweenCall = tweenFunc, Priority = priority, CanTouch = canTouch });
        }

        // 2. 添加弹窗任务
        public void AddPopupTask(string popupName, Action openAction, int priority = 999, bool canTouch = true)
        {
            Enqueue(new QueueTask { Type = TaskType.WaitPopup, Name = popupName, Action = openAction, Priority = priority, CanTouch = canTouch });
        }

        // 3. 添加时间任务
        public void AddTimeTask(float duration, Action startAction = null, Action endAction = null, int priority = 999, bool canTouch = true)
        {
            Enqueue(new QueueTask 
            { 
                Type = TaskType.WaitTime, 
                Duration = duration, 
                Action = startAction, 
                Priority = priority, 
                CanTouch = canTouch,
                CloseEvent = "TIME_END_CALLBACK" // 内部标记
            });
        }

        // 4. 添加事件任务 (分发一个事件，等待另一个事件返回)
        public void AddEventTask(string sendEvent, string waitEvent, int priority = 999, bool canTouch = true)
        {
            Enqueue(new QueueTask 
            { 
                Type = TaskType.WaitEvent, 
                Name = sendEvent, 
                CloseEvent = waitEvent, 
                Priority = priority, 
                CanTouch = canTouch 
            });
        }

        #endregion

        #region 队列核心逻辑

        private void Enqueue(QueueTask task)
        {
            if (task == null) return;
            
            // // 防重入：禁止在队列中存在完全相同类型的任务（根据业务需求定）
            // if (_queueList.Exists(t => t.Type == task.Type && t.Name == task.Name))
            // {
            //     Debugger.LogWarning($"Task {task.Name} already in queue, skipped.");
            //     return;
            // }
            
            // 如果是弹窗，检查是否已经存在同名弹窗在队列中（防重复）
            if (task.Type == TaskType.WaitPopup && _queueList.Exists(t => t.Name == task.Name))
            {
                return; 
            }

            _queueList.Add(task);
            
            // 标记队列变动，由 LateUpdate 在帧末统一排序，防止重复触发
            _isDirty = true;
        }

        private void ProcessNext()
        {
            if (_queueList.Count == 0)
            {
                _currentTask = null;
                SetGlobalTouch(true);
                return;
            }

            _currentTask = _queueList[0];
            _queueList.RemoveAt(0);
            _currentTask.IsProcessing = true;

            SetGlobalTouch(_currentTask.CanTouch);
            ExecuteTask(_currentTask);
        }
        
        private async UniTaskVoid ExecuteInteractionAsync(IChainTask node)
        {
            try
            {
                await node.ExecuteAsync(); // 此时代码会在这里“挂起”，直到任务完成
            }
            catch (Exception ex)
            {
                Debugger.LogError($"Interaction Task Execution Failed: {ex.Message}");
            }
            finally
            {
                TurnToNext(); // 无论成功与否，都要推进队列，防止卡死
            }
        }

        private void ExecuteTask(QueueTask task)
        {
            switch (task.Type)
            {
                case TaskType.WaitTime:
                    DoTimeTaskAsync(task).Forget();
                    break;

                case TaskType.SequenceTween:
                    ExecuteTweenTask(task);
                    break;

                case TaskType.WaitPopup:
                    task.Action?.Invoke();
                    // 等待 OnPopupAction 回调触发 TurnToNext
                    break;

                case TaskType.WaitEvent:
                    // 延迟一帧分发事件，防止同步调用导致逻辑混乱
                    Observable.NextFrame().Subscribe(_ => GameEntry.Event.Dispatch(task.Name));
                    // 等待 OnEventMessage 回调触发 TurnToNext
                    break;
            }
        }

        private void TurnToNext()
        {
            _currentTask = null;
            ProcessNext();
        }

        public void ClearQueue(bool immediate = true)
        {
            _queueList.Clear();
            _isDirty = false;
            if (immediate)
            {
                StopAllCoroutines();
                _currentTask = null;
                SetGlobalTouch(true);
            }
        }

        #endregion

        #region 任务执行细节

        private async UniTaskVoid DoTimeTaskAsync(QueueTask task)
        {
            try
            {
                task.Action?.Invoke();
                // 这里的 Delay 完美替代了 WaitForSeconds
                await UniTask.Delay(TimeSpan.FromSeconds(task.Duration), cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            catch (Exception ex)
            {
                Debugger.LogError($"[QueueManager] Time Task Error: {ex.Message}");
            }
            finally
            {
                TurnToNext();
            }
        }

        private void ExecuteTweenTask(QueueTask task)
        {
            try
            {
                Tween t = task.TweenCall?.Invoke();
                if (t != null && t.IsActive())
                {
                    t.OnComplete(TurnToNext);
                }
                else
                {
                    TurnToNext();
                }
            }
            catch (Exception ex)
            {
                Debugger.LogError($"Tween Task Error: {ex.Message}");
                TurnToNext();
            }
        }

        private void OnPopupAction(object userData)
        {
            var evt = userData as PopupActionEvent;
            if (evt != null && evt.UIActionType == UIActionType.HideUI)
            {
                if (_currentTask != null && _currentTask.Type == TaskType.WaitPopup && _currentTask.Name == evt.Name)
                {
                    TurnToNext();
                }
            }
        }

        private void OnEventMessage(object userData)
        {
            var evt = userData as EventMessage;
            if (evt != null && _currentTask != null && _currentTask.Type == TaskType.WaitEvent)
            {
                if (_currentTask.CloseEvent == evt.EventName)
                {
                    TurnToNext();
                }
            }
        }

        private void SetGlobalTouch(bool canTouch)
        {
            // 屏蔽/开启 UI 点击层
            // FxMaskView.Instance.CanTouch = canTouch;
        }
        
        public int GetQueueCount() => _queueList.Count + (_currentTask != null ? 1 : 0);

        #endregion
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Main;
using UnityEngine;

namespace GameScripts
{
    /// <summary>
    /// 任务管理器
    /// </summary>
    public class TaskManager : IDisposable
    {
        /// <summary>
        /// 任务组列表
        /// </summary>
        private LinkedList<TaskGroup> m_TaskGroupList;
    
        private TaskGroup CommonGroup;
    
        public TaskManager()
        {
            m_TaskGroupList = new LinkedList<TaskGroup>();
            CommonGroup = new TaskGroup();
            CommonGroup.OnComplete = () => { GameEntry.UI.CloseUIForm<FormCircle>(); };
        }

        public void OnUpdate()
        {
    #if DEBUG_MODEL
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.E))
            {
                CommonGroup.LogTask();
                LinkedListNode<TaskGroup> taskGroup = m_TaskGroupList.First;
                while (taskGroup != null)
                {
                    Debugger.LogError(LogCategory.Framework, "======================");
                    taskGroup.Value.LogTask();
                    taskGroup = taskGroup.Next;
                }
            }
    #endif
            UpdateItem();
        }
    
        private void UpdateItem()
        {
            CommonGroup.OnUpdate();
            LinkedListNode<TaskGroup> taskGroup = m_TaskGroupList.First;
            while (taskGroup != null)
            {
                taskGroup.Value.OnUpdate();
                taskGroup = taskGroup.Next;
            }
        }
    
        /// <summary>
        /// 添加异步任务 (等待异步时会有 转圈等待UI遮罩)
        /// </summary>
        public void AddTaskCommon(Action<TaskRoutine> task, bool isTask = true)
        {
            CommonGroup.AddTask(task, isTask);
            CommonGroup.Run(false, () => GameEntry.UI.OpenUIForm<FormCircle>());
        }
    
        /// <summary>
        /// 创建一个任务组
        /// </summary>
        public TaskGroup CreateTaskGroup()
        {
            TaskGroup taskGroup = new TaskGroup();
            return taskGroup;
        }
    
        /// <summary>
        /// 注册任务组
        /// </summary>
        internal void RegisterTaskGroup(TaskGroup taskGroup)
        {
            m_TaskGroupList.AddLast(taskGroup);
        }
    
        /// <summary>
        /// 移除任务组
        /// </summary>
        public void RemoveTaskGroup(TaskGroup taskGroup)
        {
            m_TaskGroupList.Remove(taskGroup);
        }
    
        public void Dispose()
        {
    
        }
    }
    
    
        /// <summary>
    /// 任务组
    /// </summary>
    public class TaskGroup : IDisposable
    {
        /// <summary>
        /// 任务列表
        /// </summary>
        private LinkedList<TaskRoutine> m_TaskRoutineList;
    
        /// <summary>
        /// 任务组完成
        /// </summary>
        public Action OnComplete;
    
        /// <summary>
        /// 单个任务完成
        /// </summary>
        public Action OnCompleteOne;
    
        /// <summary>
        /// 是否并发执行
        /// </summary>
        private bool m_IsConcurrency = false;
    
        /// <summary>
        /// 是否正在执行
        /// </summary>
        public bool InTask { get; private set; }
    
        public int TotalCount { get; private set; }
        public int CurrCount { get; private set; }
    
        public TaskGroup()
        {
            m_TaskRoutineList = new LinkedList<TaskRoutine>();
        }
    
        public void Dispose()
        {
            InTask = false;
            OnComplete?.Invoke();
            CurrCount = 0;
            TotalCount = 0;
            m_TaskRoutineList.Clear();
            GameEntry.Task.RemoveTaskGroup(this);
        }
    
        public virtual void AddTask(Action<TaskRoutine> task, bool isAddGroup = true)
        {
            if (task == null) return;
            TaskRoutine taskRoutine = new TaskRoutine();
            taskRoutine.CurrTask = task;
            if (isAddGroup)
            {
                m_TaskRoutineList.AddLast(taskRoutine);
                TotalCount++;
            }
            else
            {
                taskRoutine.Enter();
            }
        }
    
        public void LeaveCurrTask()
        {
            LinkedListNode<TaskRoutine> curr = m_TaskRoutineList.First;
            if (curr != null && InTask)
            {
                curr.Value.Leave();
            }
        }
    
        /// <summary>
        /// 清空所有任务
        /// </summary>
        public void ClearAllTask()
        {
            LinkedListNode<TaskRoutine> routine = m_TaskRoutineList.First;
            while (routine != null)
            {
                var next = routine.Next;
                routine.Value.StopTask?.Invoke();
                m_TaskRoutineList.Remove(routine);
                routine = next;
            }
        }
    
        /// <summary>
        /// 执行任务
        /// </summary>
        public void Run(bool isConcurrency = false, Action onStart = null)
        {
            if (m_TaskRoutineList.Count == 0) return;
    
            if (InTask) return;
            InTask = true;
    
            GameEntry.Task.RegisterTaskGroup(this);
            onStart?.Invoke();
    
            //是否并行
            m_IsConcurrency = isConcurrency;
            if (m_IsConcurrency)
            {
                ConcurrencyTask();
            }
            else
            {
                CheckTask();
            }
        }
    
        public void OnUpdate()
        {
            LinkedListNode<TaskRoutine> taskRotine = m_TaskRoutineList.First;
            while (taskRotine != null)
            {
                taskRotine.Value.OnUpdate();
                taskRotine = taskRotine.Next;
            }
        }
    
        /// <summary>
        /// 按照AddTask顺序执行任务
        /// </summary>
        private void CheckTask()
        {
            LinkedListNode<TaskRoutine> curr = m_TaskRoutineList.First;
            if (curr != null)
            {
                curr.Value.OnCompleteStack.Push(() =>
                {
                    CurrCount++;
                    OnCompleteOne?.Invoke();
                    m_TaskRoutineList.Remove(curr);
                    CheckTask();
                });
                curr.Value.Enter();
            }
            else
            {
                Dispose();
            }
        }
    
        /// <summary>
        /// 并发执行任务
        /// </summary>
        private void ConcurrencyTask()
        {
            LinkedListNode<TaskRoutine> routine = m_TaskRoutineList.First;
            while (routine != null)
            {
                LinkedListNode<TaskRoutine> next = routine.Next;
                routine.Value.OnCompleteStack.Push(() =>
                {
                    CurrCount++;
                    OnCompleteOne?.Invoke();
                    if (CurrCount == TotalCount) Dispose();
                });
                routine.Value.Enter();
                routine = next;
            }
        }
    
        public void LogTask()
        {
            Debugger.LogError(LogCategory.Framework, "InTask={0}", InTask);
            LinkedListNode<TaskRoutine> routine = m_TaskRoutineList.First;
            while (routine != null)
            {
                Debugger.LogError(LogCategory.Framework, routine.Value);
                Debugger.LogError(LogCategory.Framework, routine.Value.CurrTask);
                Debugger.LogError(LogCategory.Framework, "{0}=========={1}", routine.Value.CurrTask.Target,
                    routine.Value.CurrTask.Method);
                routine = routine.Next;
            }
        }
    }
        
    /// <summary>
    /// 任务执行器
    /// </summary>
    public class TaskRoutine
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int TaskRoutineId;
    
        /// <summary>
        /// 具体的任务
        /// </summary>
        public Action<TaskRoutine> CurrTask;
    
        /// <summary>
        /// 任务完成
        /// </summary>
        public readonly Stack<Action> OnCompleteStack = new Stack<Action>();
    
        /// <summary>
        /// 停止任务
        /// </summary>
        public Action StopTask;
    
        /// <summary>
        /// 任务数据
        /// </summary>
        public object TaskData;
    
        /// <summary>
        /// 进入任务
        /// </summary>
        public void Enter()
        {
            if (CurrTask != null) CurrTask(this);
            else Leave();
        }
    
        public void OnUpdate()
        {
            if (CurrTask != null && CurrTask.Target == null) Leave();
        }
    
        /// <summary>
        /// 离开任务
        /// </summary>
        public void Leave()
        {
            if (OnCompleteStack.Count > 0)
            {
                while (OnCompleteStack.Count > 0)
                {
                    OnCompleteStack.Pop()();
                }
    
                CurrTask = null;
            }
        }
    }
}
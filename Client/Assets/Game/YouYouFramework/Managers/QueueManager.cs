using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TaskData
{
    public enum TaskType
    {
        Popup,
        Time,
        Tween,
        Event
    }
    public TaskType Type { get; }
    public Action Action { get; }
    public int Priority { get; set; }

    public TaskData(TaskType type, Action action, int inputPriority = 0)
    {
        Type = type;
        Action = action;
        Priority = inputPriority;
    }
}

public class QueueManager : MonoBehaviour
{
    private float checkInterval = 0.04f; // 检测间隔秒数
    private float elapsedTime = 0f;
    private List<TaskData> taskList = new List<TaskData>();
    private List<TaskData> readyAddTaskList = new List<TaskData>();
    private bool isHandlingQueue = false;
    private readonly object _lock = new object();
    
    private static QueueManager _instace;

    public static QueueManager Instance
    {
        get
        {
            if (_instace == null)
            {
                GameObject singletonObject = new GameObject("QueueManager");
                _instace = singletonObject.AddComponent<QueueManager>();
            }

            return _instace;
        }
    }
    
    private void Update()
    {
        if (taskList.Count == 0 && readyAddTaskList.Count == 0) return;
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= checkInterval)
        {
            HandleReadyAddTaskList(); // 检查准备添加的任务
            HandleTaskQueue(); // 处理任务队列
            elapsedTime = 0f; // 重置计时器
        }
    }

    //添加弹窗任务
    public void AddPopupTask(string popupName, Action action, int inputPriority = 999, bool isInsert = false,bool isCanTouch = true)
    {
        //IConfigService configService = MainContainer.Container.Resolve<IConfigService>();
        //if (popupName == Constants.DoozyView.RewardPopup) isInsert = true;
        var task = new TaskData(TaskData.TaskType.Popup, () => ViewQueueManager.Instance.AddWaitPopupClose(popupName, action,isInsert,isCanTouch), inputPriority);
        // // 检查优先级
        // if (configService.PopupPriorityConfig.TryGetValue(popupName, out PopupPriorityModel model))
        // {
        //     task.Priority = model.priority;
        // }
        readyAddTaskList.Add(task);
    }

    //添加Time任务
    public void AddTimeTask(float time,Action action = null,Action endAction = null,int priority = 999, bool isInsert = false,bool isCanTouch = true)
    {
        var task = new TaskData(TaskData.TaskType.Time, () => ViewQueueManager.Instance.AddWaitTime(time,action,endAction,isInsert,isCanTouch), priority);
        readyAddTaskList.Add(task);
    }

    //添加Tween任务
    public void AddTweenTask(Func<Tween> tweenFunc, int priority = 999,bool isInsert = false,bool isCanTouch = true)
    {
        var task = new TaskData(TaskData.TaskType.Tween, () => ViewQueueManager.Instance.AddWaitTween(tweenFunc,isInsert,isCanTouch), priority);
        readyAddTaskList.Add(task);
    }

    //添加Event任务
    public void AddEventTask(string eventName, string closeEventName, int priority = 999, bool isInsert = false,bool isCanTouch = true)
    {
        var task = new TaskData(TaskData.TaskType.Event, () => ViewQueueManager.Instance.AddEventNameClose(eventName,closeEventName,isInsert,isCanTouch), priority);
        readyAddTaskList.Add(task);
    }

    public int GetTaskCount()
    {
        return ViewQueueManager.Instance.GetQueueCount();
    }
    
    
    
    private void HandleReadyAddTaskList()
    {
        lock (_lock)
        {
            foreach (var data in readyAddTaskList)
            {
                taskList.Add(data);
            }

            readyAddTaskList.Clear(); // 清空已处理的列表
        }
    }

    private void HandleTaskQueue()
    {
        lock (_lock)
        {
            if (isHandlingQueue || taskList.Count == 0)
                return;
            isHandlingQueue = true;
            taskList.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            foreach (var taskData in taskList)
            {
                taskData.Action.Invoke();
            }

            taskList.Clear();
            isHandlingQueue = false; // 处理完成
        }
    }
}

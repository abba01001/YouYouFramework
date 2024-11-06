using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using YouYou;

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

public class ViewQueueManager : MonoBehaviour
{
    [Serializable]
    public enum ViewParamType
    {
        WaitTime,
        WaitViewClose,
        WaitEventName,
        WaitPopupClose,
        SequenceEnd,
        Generic,
        NoneSet
    }
    
    [Serializable]
    private class ViewQueueParamBase
    {
        [HideIf("@IsWatting()")]
        public bool isWaitting = false;

        [HideIf("@IsCanTouch()")]
        public bool canTouch = true;

        [HideIf("@IsNameEmpty()")]
        public string name = "";

        [HideIf("@IsEventNameEmpty()")]
        public string eventName = "";

        [HideIf("@IsCloseEventNameEmpty()")]
        public string closeEventName = "";

        public int paramId = -1;
        public ViewParamType paramType = ViewParamType.NoneSet;

        public bool IsWatting() => !isWaitting;
        public bool IsCanTouch() => !canTouch;
        public bool IsNameEmpty() => string.IsNullOrEmpty(name);
        public bool IsEventNameEmpty() => string.IsNullOrEmpty(eventName);
        public bool IsCloseEventNameEmpty() => string.IsNullOrEmpty(closeEventName);
    }
    
    private int ParamID = -1;

    private class WaitTime : ViewQueueParamBase
    {
        public float time;
        public Action startAction;
        public Action endAction;
    }

    private class WaitEventName : ViewQueueParamBase
    {
    }
    
    private class WaitViewClose : ViewQueueParamBase
    {
    }

    private class WaitPopupClose : ViewQueueParamBase
    {
        public Action openAction;
    }

    private class SequenceEnd : ViewQueueParamBase
    {
        public Func<Tween> tweenCall;
    }

    private static ViewQueueManager _instace;

    public static ViewQueueManager Instance
    {
        get
        {
            if (_instace == null)
            {
                GameObject singletonObject = new GameObject("ViewQueueManager");
                _instace = singletonObject.AddComponent<ViewQueueManager>();
            }

            return _instace;
        }
    }

    [SerializeField] private List<ViewQueueParamBase> queueList = new List<ViewQueueParamBase>();
    private ViewQueueParamBase nowParamBase;

    public void RegisterEvents()
    {
        GameEntry.Event.AddEventListener(Constants.EventName.PopupAction, OnPopupAction);
        GameEntry.Event.AddEventListener(Constants.EventName.EventMessage, OnEventMessage);
        
    }

    private void UnregisterEvents()
    {
        GameEntry.Event.RemoveEventListener(Constants.EventName.PopupAction, OnPopupAction);
        GameEntry.Event.RemoveEventListener(Constants.EventName.EventMessage, OnEventMessage);
    }

    private void OnEventMessage(object userData)
    {
        EventMessage t = userData as EventMessage;
        GameUtil.LogError(t.EventName);
        if (nowParamBase != null && nowParamBase is WaitEventName waitEventParam)
        {
            if (waitEventParam.closeEventName == t.EventName)
                TurnToNextParam();
        }
    }

    private void OnPopupAction(object userData)
    {
        PopupActionEvent t = userData as PopupActionEvent;
        if (t.UIActionType == UIActionType.HideUI)
        {
            if (nowParamBase is WaitPopupClose popupCloseParam && popupCloseParam.name == t.Name)
            {
                TurnToNextParam();
            }
        }
    }

    private IEnumerator WaitTimeParam()
    {
        if (nowParamBase == null) yield break;
        if (!(nowParamBase is WaitTime waitTimeParam))
        {
            TurnToNextParam();
            yield break;
        }

        waitTimeParam.startAction?.Invoke();
        yield return new WaitForSeconds(waitTimeParam.time);
        waitTimeParam.endAction?.Invoke();
        TurnToNextParam();
    }

    public void AddEventNameClose(string eventName, string closeEventName, bool isInsert = false,
        bool isCanTouch = true)
    {
        ViewQueueParamBase paramBase = new WaitEventName()
        {
            eventName = eventName,
            closeEventName = closeEventName,
            canTouch = isCanTouch,
            paramType = ViewParamType.WaitEventName
        };
        AddViewQueueParam(paramBase, isInsert);
    }

    public void AddWaitViewClose(string eventName, string closeEventName, bool isInsert = false, bool isCanTouch = true)
    {
        ViewQueueParamBase paramBase = new WaitViewClose()
        {
            eventName = eventName,
            closeEventName = closeEventName,
            canTouch = isCanTouch,
            paramType = ViewParamType.WaitViewClose
        };
        AddViewQueueParam(paramBase, isInsert);
    }

    
    public void AddWaitPopupClose(string popupName, Action openAction, bool isInsert = false, bool isCanTouch = true)
    {
        ViewQueueParamBase paramBase = new WaitPopupClose()
        {
            openAction = openAction,
            canTouch = isCanTouch,
            name = popupName,
            paramType = ViewParamType.WaitPopupClose
        };
        AddViewQueueParam(paramBase, isInsert);
    }

    public void AddWaitTime(float time, Action action, Action endAction = null, bool isInsert = false,bool isCanTouch = true)
    {
        ViewQueueParamBase paramBase = new WaitTime()
        {
            time = time,
            startAction = action,
            endAction = endAction,
            canTouch = isCanTouch,
            paramType = ViewParamType.WaitTime
        };
        AddViewQueueParam(paramBase, isInsert);
    }

    public void AddWaitTween(Func<Tween> tweenCall, bool isInsert = false, bool isCanTouch = true)
    {
        ViewQueueParamBase paramBase = new SequenceEnd()
        {
            tweenCall = tweenCall,
            canTouch = isCanTouch,
            paramType = ViewParamType.SequenceEnd
        };
        AddViewQueueParam(paramBase, isInsert);
    }

    public void ClearQueue()
    {
        nowParamBase = null;
        queueList.Clear();
    }

    private void AddViewQueueParam(ViewQueueParamBase param, bool isInsert = false)
    {
        if (param == null || CheckPopupHasAdd(param)) return;

        param.paramId = ++ParamID;
        if (isInsert) queueList.Insert(nowParamBase == null ? 0 : 1, param);
        else queueList.Add(param);

        if (nowParamBase == null) TurnToNextParam();
    }

    //Popup是否加入队列
    bool CheckPopupHasAdd(ViewQueueParamBase paramBase)
    {
        bool isAdd = false;
        if (paramBase.paramType == ViewParamType.WaitPopupClose)
        {
            WaitPopupClose t = paramBase as WaitPopupClose;
            foreach (var pair in queueList)
            {
                if (t.name == pair.name)
                {
                    if (CheckMultiplyAdd(pair.name)) return false;
                    return true;
                }
            }
        }
        return false;
    }

    //Popup参数能否叠加
    bool CheckMultiplyAdd(string popupName)
    {
        //if (popupName == Constants.DoozyView.RewardPopup) return true;
        return false;
    }

    private void TurnToNextParam()
    {
        SetCanTouch(true);
        if (nowParamBase != null) queueList.Remove(nowParamBase);
        nowParamBase = queueList.Count > 0 ? queueList[0] : null;
        RunNowParam();
    }

    public int GetQueueCount()
    {
        return queueList.Count;
    }

    private void SetCanTouch(bool state)
    {
        //是否可点击屏幕
        //FxMaskView.Instance.CanTouch = state;
    }

    private void RunNowParam()
    {
        if (nowParamBase?.isWaitting != false) return;
        nowParamBase.isWaitting = true;
        SetCanTouch(nowParamBase.canTouch);
        switch (nowParamBase)
        {
            case WaitTime param:
                StartCoroutine(WaitTimeParam());
                break;
            case WaitEventName param:
                GameEntry.Time.Yield(() => { GameEntry.Event.Dispatch(param.eventName); });
                break;
            case WaitPopupClose param:
                param.openAction();
                break;
            case SequenceEnd param:
                try
                {
                    Tween tween = param.tweenCall?.Invoke();
                    if (tween != null && tween.Duration() != 0) tween.onComplete += TurnToNextParam;
                    else TurnToNextParam();
                }
                catch (Exception ex)
                {
                    GameUtil.LogError($"${ex.Message}");
                    TurnToNextParam();
                }
                break;
            default:
                break;
        }
    }
}
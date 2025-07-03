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
        var task = new TaskData(TaskData.TaskType.Popup, () => ViewQueueManager.Instance.EnqueuePopup(popupName, action,isInsert,isCanTouch), inputPriority);
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
        var task = new TaskData(TaskData.TaskType.Time, () => ViewQueueManager.Instance.EnqueueTime(time,action,endAction,isInsert,isCanTouch), priority);
        readyAddTaskList.Add(task);
    }

    //添加Tween任务
    public void AddTweenTask(Func<Tween> tweenFunc, int priority = 999,bool isInsert = false,bool isCanTouch = true)
    {
        var task = new TaskData(TaskData.TaskType.Tween, () => ViewQueueManager.Instance.EnqueueTween(tweenFunc,isInsert,isCanTouch), priority);
        readyAddTaskList.Add(task);
    }

    //添加Event任务
    public void AddEventTask(string eventName, string closeEventName, int priority = 999, bool isInsert = false,bool isCanTouch = true)
    {
        var task = new TaskData(TaskData.TaskType.Event, () => ViewQueueManager.Instance.EnqueueEvent(eventName,closeEventName,isInsert,isCanTouch), priority);
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
    private class QueueParamBase
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

    private class TimeTaskParam : QueueParamBase
    {
        public float time;
        public Action startAction;
        public Action endAction;
    }

    private class EventTaskParam : QueueParamBase
    {
    }
    
    private class ViewTaskParam : QueueParamBase
    {
    }

    private class PopupTaskParam : QueueParamBase
    {
        public Action openAction;
    }

    private class SequenceTaskParam : QueueParamBase
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

    [SerializeField] private List<QueueParamBase> queueList = new List<QueueParamBase>();
    private QueueParamBase currentParam;

    public void RegisterEvents()
    {
        GameEntry.Event.AddEventListener(Constants.EventName.FinishLoadDataTable, OnInitFormMask);
        GameEntry.Event.AddEventListener(Constants.EventName.PopupAction, OnPopupAction);
        GameEntry.Event.AddEventListener(Constants.EventName.EventMessage, OnEventMessage);
        
    }

    public void OnInitFormMask(object userData)
    {
        GameEntry.UI.OpenUIForm<FormMask>();
    }
    
    private void UnregisterEvents()
    {
        GameEntry.Event.RemoveEventListener(Constants.EventName.FinishLoadDataTable, OnInitFormMask);
        GameEntry.Event.RemoveEventListener(Constants.EventName.PopupAction, OnPopupAction);
        GameEntry.Event.RemoveEventListener(Constants.EventName.EventMessage, OnEventMessage);
    }

    private void OnEventMessage(object userData)
    {
        EventMessage t = userData as EventMessage;
        GameUtil.LogError(t.EventName);
        if (currentParam != null && currentParam is EventTaskParam waitEventParam)
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
            if (currentParam is PopupTaskParam popupCloseParam && popupCloseParam.name == t.Name)
            {
                TurnToNextParam();
            }
        }
    }

    private IEnumerator DoTimeTaskAction()
    {
        if (currentParam == null) yield break;
        if (!(currentParam is TimeTaskParam timeParam))
        {
            TurnToNextParam();
            yield break;
        }

        timeParam.startAction?.Invoke();
        yield return new WaitForSeconds(timeParam.time);
        timeParam.endAction?.Invoke();
        TurnToNextParam();
    }

    public void EnqueueEvent(string eventName, string closeEventName, bool isInsert = false,
        bool isCanTouch = true)
    {
        QueueParamBase paramBase = new EventTaskParam()
        {
            eventName = eventName,
            closeEventName = closeEventName,
            canTouch = isCanTouch,
            paramType = ViewParamType.WaitEventName
        };
        EnqueueParam(paramBase, isInsert);
    }

    public void EnqueueView(string eventName, string closeEventName, bool isInsert = false, bool isCanTouch = true)
    {
        QueueParamBase paramBase = new ViewTaskParam()
        {
            eventName = eventName,
            closeEventName = closeEventName,
            canTouch = isCanTouch,
            paramType = ViewParamType.WaitViewClose
        };
        EnqueueParam(paramBase, isInsert);
    }

    
    public void EnqueuePopup(string popupName, Action openAction, bool isInsert = false, bool isCanTouch = true)
    {
        QueueParamBase paramBase = new PopupTaskParam()
        {
            openAction = openAction,
            canTouch = isCanTouch,
            name = popupName,
            paramType = ViewParamType.WaitPopupClose
        };
        EnqueueParam(paramBase, isInsert);
    }

    public void EnqueueTime(float time, Action action, Action endAction = null, bool isInsert = false,bool isCanTouch = true)
    {
        QueueParamBase paramBase = new TimeTaskParam()
        {
            time = time,
            startAction = action,
            endAction = endAction,
            canTouch = isCanTouch,
            paramType = ViewParamType.WaitTime
        };
        EnqueueParam(paramBase, isInsert);
    }

    public void EnqueueTween(Func<Tween> tweenCall, bool isInsert = false, bool isCanTouch = true)
    {
        QueueParamBase paramBase = new SequenceTaskParam()
        {
            tweenCall = tweenCall,
            canTouch = isCanTouch,
            paramType = ViewParamType.SequenceEnd
        };
        EnqueueParam(paramBase, isInsert);
    }

    public void ClearQueue()
    {
        currentParam = null;
        queueList.Clear();
    }

    private void EnqueueParam(QueueParamBase param, bool isInsert = false)
    {
        if (param == null || CheckPopupHasAdd(param)) return;

        param.paramId = ++ParamID;
        if (isInsert) queueList.Insert(currentParam == null ? 0 : 1, param);
        else queueList.Add(param);

        if (currentParam == null) TurnToNextParam();
    }

    //Popup是否加入队列
    bool CheckPopupHasAdd(QueueParamBase paramBase)
    {
        bool isAdd = false;
        if (paramBase.paramType == ViewParamType.WaitPopupClose)
        {
            PopupTaskParam t = paramBase as PopupTaskParam;
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
        if (currentParam != null) queueList.Remove(currentParam);
        currentParam = queueList.Count > 0 ? queueList[0] : null;
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
        if (currentParam?.isWaitting != false) return;
        currentParam.isWaitting = true;
        SetCanTouch(currentParam.canTouch);
        switch (currentParam)
        {
            case TimeTaskParam param:
                StartCoroutine(DoTimeTaskAction());
                break;
            case EventTaskParam param:
                GameEntry.Time.Yield(() => { GameEntry.Event.Dispatch(param.eventName); });
                break;
            case PopupTaskParam param:
                param.openAction();
                break;
            case SequenceTaskParam param:
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
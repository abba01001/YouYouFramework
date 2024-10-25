using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;
using YouYou;

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
        public bool isWaitting = false;
        public bool canTouch = true;
        public string name = "";
        public int paramId = -1;
        public ViewParamType paramType = ViewParamType.NoneSet;
    }
    private int ParamID = -1;

    private class WaitTime : ViewQueueParamBase
    {
        public float time;
        public Action action;
        public Action endAction;
    }

    private class WaitEventName : ViewQueueParamBase
    {
        public string eventName;
        public string closeEventName;
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
                _instace = new ViewQueueManager();
                _instace.RegisterEvents();
            }

            return _instace;
        }
    }


    [SerializeField] private List<ViewQueueParamBase> queueList = new List<ViewQueueParamBase>();
    private ViewQueueParamBase nowParamBase;

    private void RegisterEvents()
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

        waitTimeParam.action?.Invoke();
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

    public void AddWaitPopupClose(string popupName, Action openAction, bool isInsert = false,
        bool isCanTouch = true)
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
            action = action,
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
        //FxMaskView.Instance.CanTouch = true;
        if (nowParamBase != null) queueList.Remove(nowParamBase);
        nowParamBase = queueList.Count > 0 ? queueList[0] : null;
        RunNowParam();
    }

    public int GetQueueCount()
    {
        return queueList.Count;
    }

    private void RunNowParam()
    {
        if (nowParamBase?.isWaitting != false) return;
        nowParamBase.isWaitting = true;
        //FxMaskView.Instance.CanTouch = nowParamBase.canTouch;
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
                Tween tween = param.tweenCall?.Invoke();
                if (tween != null && tween.Duration() != 0)
                    tween.onComplete += TurnToNextParam;
                else
                    TurnToNextParam();
                break;
            default:
                break;
        }
    }
}
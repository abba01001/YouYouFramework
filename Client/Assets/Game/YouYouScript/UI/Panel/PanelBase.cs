using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YouYou;


public class PanelBase : MonoBehaviour
{
    protected string CurPanelName = string.Empty;
    private void Awake()
    {
        OnAwake();
    }

    private void Start()
    {
        OnStart();
    }

    private void OnEnable()
    {
        GameEntry.Time.Yield(OnShow);
    }

    private void OnDisable()
    {
        GameEntry.Time.Yield(OnHide);
    }

    protected virtual void OnAwake()
    {
    }

    protected virtual void OnStart()
    {
    }

    protected virtual void OnShow()
    {
        if (CurPanelName != string.Empty)
        {
            // Dictionary<string,object> dic = new Dictionary<string,object>();
            // dic.Add("Panel",CurPanelName);
            // TalkingDataSDK.OnEvent("显示Panel",dic,null);
            TalkingDataSDK.OnPageBegin($"{CurPanelName}");
        }
        GameEntry.Event.AddEventListener(Constants.EventName.UpdateBtnUnlockStatus,OnUpdateBtnStatus);
    }

    protected virtual void OnHide()
    {
        if (CurPanelName != string.Empty)
        {
            // Dictionary<string,object> dic = new Dictionary<string,object>();
            // dic.Add("Panel",CurPanelName);
            // TalkingDataSDK.OnEvent("关闭Panel",dic,null);
            TalkingDataSDK.OnPageEnd($"{CurPanelName}");
        }
        GameEntry.Event.RemoveEventListener(Constants.EventName.UpdateBtnUnlockStatus,OnUpdateBtnStatus);
    }

    protected virtual void OnUpdateBtnStatus(object user_data)
    {
        
    }
}
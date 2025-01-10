using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Main;
using MessagePack;
using Protocols;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YouYou;


// "登录"界面
public class FormYouLi : UIFormBase
{
    [SerializeField] private Button loginBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button getBtn;
    [SerializeField] private Button quickGetBtn;

    protected override void Awake()
    {
        base.Awake();
        closeBtn.SetButtonClick(() =>
        {
            GameEntry.UI.CloseUIForm<FormYouLi>();
        });
        getBtn.SetButtonClick(() =>
        {
            
        });
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEntry.Event.AddEventListener(Constants.EventName.GetSuspendReward,HandleGetReward);
        if (GameEntry.Data.SuspendStartTime == 0)
        {
            GameEntry.Net.Requset.c2s_request_get_suspend_reward(0);
        }
    }

    private void HandleGetReward(object data)
    {
        SuspendTimeMsg msg = data as SuspendTimeMsg;
        if (msg.Type == 0)
        {
            GameEntry.Data.SuspendStartTime = msg.Timestamp;
            GameUtil.LogError(GameEntry.Time.ConvertSecondsToTimeFormat(msg.Timestamp));
        }
        else
        {
            
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEntry.Event.RemoveEventListener(Constants.EventName.GetSuspendReward,HandleGetReward);
    }
}

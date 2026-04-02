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



// "登录"界面
public class FormYouLi : UIFormBase
{
    [SerializeField] private Button loginBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button getBtn;
    [SerializeField] private Button quickGetBtn;

    protected override async UniTask Awake()
    {
        await base.Awake();
        closeBtn.SetButtonClick(() =>
        {
            GameEntry.UI.CloseUIForm<FormYouLi>();
        });
        getBtn.SetButtonClick(() =>
        {
            GameEntry.Net.Requset.c2s_request_get_suspend_reward(1);
        });
        quickGetBtn.SetButtonClick(() =>
        {
            if (GameEntry.Data.SuspendQuickGetRewardIndex < GameEntry.Data.SuspendQuickGetRewardLimit)
            {
                GameEntry.Net.Requset.c2s_request_get_suspend_reward(2);
            }
        });
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEntry.Event.AddEventListener(Constants.EventName.GetSuspendReward,HandleGetReward);
        if (GameEntry.Data.SuspendStartTime == -1)
        {
            GameEntry.Net.Requset.c2s_request_get_suspend_reward(0);
        }
    }

    private void HandleGetReward(object data)
    {
        SuspendTimeMsg msg = data as SuspendTimeMsg;
        switch (msg.Type)
        {
            case 0:
                GameEntry.Data.SuspendStartTime = msg.Timestamp;
                GameEntry.Data.SuspendQuickGetRewardIndex = msg.QuickGetSuspendRewardIndex;
                GameEntry.Data.SuspendQuickGetRewardLimit = msg.QuickGetSuspendRewardLimit;
                break;
            case 1:
                if (msg.CanGetReward && msg.Hour > 0)
                {
                    GameUtil.LogError($"挂机时间{msg.Hour}");
                    GameEntry.Net.Requset.c2s_request_get_suspend_reward(0);
                }
                break;
            case 2:
                if (msg.CanGetReward)
                {
                    GameEntry.Net.Requset.c2s_request_get_suspend_reward(0);
                }
                break;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEntry.Event.RemoveEventListener(Constants.EventName.GetSuspendReward,HandleGetReward);
    }
}

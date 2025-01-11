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

public enum ChatChannelType
{
    World = 1,//世界
    Private = 2//私聊
}

// "登录"界面
public class FormChat : UIFormBase
{
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button sendBtn;
    [SerializeField] private TMP_InputField input;
    protected override void Awake()
    {
        base.Awake();
        closeBtn.SetButtonClick(() =>
        {
            GameEntry.UI.CloseUIForm<FormYouLi>();
        });
        sendBtn.SetButtonClick(SendChat);
    }

    private void SendChat()
    {
        if (String.IsNullOrEmpty(input.text)) return;
        GameEntry.Net.Requset.c2s_request_chat(1,input.text);
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        //GameEntry.Event.AddEventListener(Constants.EventName.GetSuspendReward,HandleGetReward);

    }

    protected override void OnDisable()
    {
        base.OnDisable();
        //GameEntry.Event.RemoveEventListener(Constants.EventName.GetSuspendReward,HandleGetReward);
    }
}

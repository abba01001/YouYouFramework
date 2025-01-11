using System;
using System.Collections;
using System.Collections.Generic;
using Protocols;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YouYou;

public class MainPanel : MonoBehaviour
{
    [SerializeField] private Button YouLiBtn;
    [SerializeField] private Button MoreBtn;
    [SerializeField] private Button ChatBtn;
    [SerializeField] private GameObject MoreDetail;
    [SerializeField] private Text ChatText;
    private void Awake()
    {
        YouLiBtn.SetButtonClick(() =>
        {
            GameEntry.UI.OpenUIForm<FormYouLi>();
        });
        MoreBtn.SetButtonClick(() =>
        {
            MoreDetail.gameObject.MSetActive(!MoreDetail.gameObject.activeSelf);
        });
        ChatBtn.SetButtonClick(() =>
        {
            GameEntry.UI.OpenUIForm<FormChat>();
        });
                
        
        if (!GameEntry.Data.RequestPublicChat)
        {
            GameEntry.Data.RequestPublicChat = true;
            GameEntry.Net.Requset.c2s_request_chat(0,"",GameEntry.Data.UserId,true);
        }
        ChatText.text = "";
    }

    private void OnUpdateChatText(object data)
    {
        ChatMsg msg = data as ChatMsg;
        ChatText.text = GameUtil.TruncateText(ChatText,$"<color=yellow>【{Constants.ChatChannel[msg.ChannelType]}】</color>{msg.Message}");
    }
    
    private void OnDisable()
    {
        GameEntry.Event.RemoveEventListener(Constants.EventName.UpdateChatText,OnUpdateChatText);
    }

    private void OnEnable()
    {
        GameEntry.Event.AddEventListener(Constants.EventName.UpdateChatText,OnUpdateChatText);
    }
}
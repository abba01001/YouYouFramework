using System;
using System.Collections;
using System.Collections.Generic;
using Protocols;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YouYou;

public class MainPanel : PanelBase
{
    [SerializeField] private Button YouLiBtn;
    [SerializeField] private Button MoreBtn;
    [SerializeField] private Button ChatBtn;
    [SerializeField] private GameObject MoreDetail;
    [SerializeField] private Text ChatText;
    [SerializeField] private Button QuickFightBtn;
    protected override void OnAwake()
    {
        base.OnAwake();
        CurPanelName = "MainPanel";
        YouLiBtn.SetButtonClick(() =>
        {
            GameEntry.Data.PlayerRoleData.dialogueIds.Clear();
            GameEntry.Event.Dispatch(Constants.EventName.TriggerDialogue,new DialogueModel()
            {
                dialogueId = (int)DialogueConfigId.FirstEntryGame,
                finishAction = () =>
                {
                    GameEntry.Data.PlayerRoleData.curGuide = 0;
                    Guide_NewUser1.Instance.FirstEntryMain(QuickFightBtn,MoreBtn);
                }
            });
            return;
            
            GameEntry.UI.CloseUIForm<FormMain>();
            GameEntry.Scene.UnLoadCurrScene();
            GameEntry.Procedure.ChangeState(ProcedureState.Preload);
            //GameEntry.UI.OpenUIForm<FormYouLi>();
        });
        MoreBtn.SetButtonClick(() =>
        {
            MoreDetail.gameObject.MSetActive(!MoreDetail.gameObject.activeSelf);
        });
        ChatBtn.SetButtonClick(() =>
        {
            GameEntry.UI.OpenUIForm<FormChat>();
        });
        QuickFightBtn.SetButtonClick(() =>
        {
            return;
            GameEntry.Procedure.ChangeState(ProcedureState.Battle);
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

    protected override void OnShow()
    {
        base.OnShow();
        GameEntry.Event.RemoveEventListener(Constants.EventName.UpdateChatText,OnUpdateChatText);
        GameObject obj = GameUtil.FindObjectByPath(GameEntry.Instance.transform,"UI/UIRoot/UIDefaultGroup/FormMain(Clone)/MainPanel(Clone)002/LianHeZuoZhan");
        GameUtil.LogError($"物体{obj == null}");
    }

    protected override void OnHide()
    {
        base.OnHide();
        GameEntry.Event.AddEventListener(Constants.EventName.UpdateChatText,OnUpdateChatText);
    }
}
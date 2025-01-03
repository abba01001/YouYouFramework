using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Main;
using MessagePack;
using MessagePack.Resolvers;
using Protocols.Player;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using YouYou;


// "主"界面
public class FormMain : UIFormBase
{
    [SerializeField] private TMP_InputField account;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private Button loginBtn;
    [SerializeField] private Button guildBtn;

    protected override void Awake()
    {
        base.Awake();
        PlayerData data = new PlayerData();
        loginBtn.SetButtonClick(() =>
        {
            // GameEntry.Net.Requset.c2s_request_update_role_info(new Dictionary<string, string>()
            // {
            //     {nameof(data.UserPassword),"99999"}
            // });
            GameEntry.Data.SaveData(true,true,true,true);
            return;
            GameUtil.LogError("111111");
            GameEntry.Audio.PlayBGM("maintheme1");
            GameEntry.Instance.ShowBackGround(BGType.Main, "Assets/Game/Download/Textures/BackGround/Home/home_map_1.png");
            GameEntry.Procedure.ChangeState(ProcedureState.Battle);
            GameEntry.UI.CloseUIForm<FormMain>();
        });
        guildBtn.SetButtonClick(() =>
        {
            GameEntry.Net.Requset.c2s_request_guild_list(1,10);
        });
        //GameEntry.SDK.DownloadAvatar("1", null);
    }
}

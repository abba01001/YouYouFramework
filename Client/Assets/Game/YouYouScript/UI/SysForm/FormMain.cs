using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Main;
using MessagePack;
using MessagePack.Resolvers;
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

    protected override void Awake()
    {
        base.Awake();
        loginBtn.SetButtonClick(() =>
        {
            GameEntry.Audio.PlayBGM("maintheme1");
        });
        //GameEntry.SDK.DownloadAvatar("1", null);
    }
}

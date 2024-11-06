using System;
using System.Collections;
using System.IO;
using System.Reflection;
using MessagePack;
using MessagePack.Resolvers;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using YouYou;


// "登录"界面
public class FormLogin : UIFormBase
{
    [SerializeField] private TMP_InputField account;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private Button loginBtn;

    protected override void Awake()
    {
        base.Awake();
        loginBtn.SetButtonClick(Login);
        GameEntry.SDK.DownloadAvatar("1", null);
    }

    private void Login()
    {
        if(account.text == "" || password.text == "") return;
        GameEntry.SQL.LoginAsync(account.text, password.text);
    }
}

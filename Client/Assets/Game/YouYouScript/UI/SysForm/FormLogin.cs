using System.Collections;
using System.IO;
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
        GameEntry.SQL.InitConnect();
        loginBtn.SetButtonClick(Login);
    }

    private void Login()
    {
        if(account.text == "" || password.text == "") return;
        GameEntry.SQL.Login(account.text, password.text);
    }
}

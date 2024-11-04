using System.Collections;
using System.IO;
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
        
        StaticCompositeResolver.Instance.Register(
            MessagePack.Resolvers.GeneratedResolver.Instance,
            MessagePack.Resolvers.StandardResolver.Instance
        );
        var option = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
        MessagePackSerializer.DefaultOptions = option;
        
        DataService dataService = new DataService();
        dataService.Age = 20;
        byte[] bytes = MessagePackSerializer.Serialize(dataService);
        DataService mc2 = MessagePackSerializer.Deserialize<DataService>(bytes);
        var json = MessagePackSerializer.ConvertToJson(bytes);
        GameUtil.LogError(json);
        
    }

    private void Login()
    {
        if(account.text == "" || password.text == "") return;
        GameEntry.SQL.LoginAsync(account.text, password.text);
    }
}

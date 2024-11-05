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
        
        DataManger data = new DataManger();
        data.Age = 10;

        // 使用对象序列化
        byte[] bytes = MessagePackSerializer.Serialize(data);

        // 使用反序列化后的对象生成 JSON
        var json = MessagePackSerializer.SerializeToJson(data); // 使用对象生成 JSON
        GameUtil.LogError($"json++++===={json}"); // 这里应该能看到 { "Age": 10, "UserId"
        
        PrintUserData();
    }
    
    public void PrintUserData()
    {
        
        byte[] bytes = MessagePackSerializer.Serialize(GameEntry.Data);
        DataManger mc2 = MessagePackSerializer.Deserialize<DataManger>(bytes);
        
        GameUtil.LogError("----------");
        PrintClassProperties(mc2.GetType());
    }
    
    public void PrintClassProperties(Type type)
    {
        PropertyInfo[] properties = type.GetProperties();
        foreach (var property in properties)
        {
            GameUtil.LogError($"属性名: {property.Name}, 属性类型: {property.PropertyType}");
        }
    }

    private void Login()
    {
        if(account.text == "" || password.text == "") return;
        GameEntry.SQL.LoginAsync(account.text, password.text);
    }
}

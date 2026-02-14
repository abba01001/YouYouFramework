using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Main;
using MessagePack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YouYou;


// "登录"界面
public class FormLogin : UIFormBase
{
    [SerializeField] private TMP_InputField account;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private Button loginBtn;
    [SerializeField] private Image bgImage;
    protected override void Awake()
    {
        base.Awake();
        LoadLoginBg();
        loginBtn.SetButtonClick(() =>
        {
            Login();
        });
#if !UNITY_EDITOR
        //GameUtil.GetSignatureMD5Hash();
#endif

#if UNITY_EDITOR
        Login();
#endif
        //GameEntry.SDK.DownloadAvatar("1", null);
    }

    private async Task Login()
    {
        //if(account.text == "" || password.text == "") return;
        //GameEntry.SDK.LoginAsync(account.text, password.text);
        
        //loginBtn.GetComponent<Image>().SetImage("Assets/Game/Download/Atlas/Textures/Common","JoyBg.png",true);
        
         // GameEntry.Net.Requset.c2s_request_register("a888888","99999");
         if (MainEntry.IsOfflineMode)
         {
             string result = PlayerPrefs.GetString("SaveData");
             byte[] binaryData = Convert.FromBase64String(result);
             GameEntry.Data.InitGameData(binaryData);
             long timestampSeconds = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
             GameEntry.Time.InitNetTime(timestampSeconds);

             await UniTask.Delay(100);
             
             GameEntry.Event.Dispatch(Constants.EventName.LoginSuccess);
             Constants.IsEntryGame = true;
         }
         else
         {
             await GameEntry.Net.ConnectServerAsync();
             GameEntry.Net.Requset.c2s_request_login("a888888","99999");
         } 
         return;
        
//将 Base64 字符串转换为二进制数据
        byte[] t = Convert.FromBase64String(
             "aGFaVmMyVnlTV1RaSkRZNE1UWmhPR1JoTFRFeVpEVXROREV5WkMxaE5EZzBMVE15T1RJMU1HUmpNRFU1WWJCSmMwWnBjbk4wVEc5bmFXNVVhVzFsd3E1RVlYUmhWWEJrWVhSbFZHbHRaYzVuYStoaHIweGhjM1JTWldaeVpYTm9WR2x0WmM1bmEraGhybEJzWVhsbGNsSnZiR1ZFWVhSaGlLUnVZVzFsb0xOMGIzUmhiRTl1YkdsdVpVUjFjbUYwYVc5dUFMTjBiMlJoZVU5dWJHbHVaVVIxY21GMGFXOXVBS3RrYVdGc2IyZDFaVWxrYzRDcFpYRjFhWEJKYm1admdLeGlZV2RYWVhKbFNHOTFjMldBcm1WeGRXbHdWMkZ5WlVodmRYTmxnS2h5YjJ4bFFYUjBjb2FxY205c1pWOXNaWFpsYkFHb2NtOXNaVjlsZUhBQXFYSnZiR1ZmYUdWaGRBQ3BjbTlzWlY5amIybHVBS2x5YjJ4bFgzTnViM2NBclhKdmJHVmZiR2wyWlc1bGMzTUE=");
        // GameUtil.LogError(t.ToBase64());
        // Convert.FromBase64String(t);
        byte[] test = Convert.FromBase64String(
            "haZVc2VySWTZJDY4MTZhOGRhLTEyZDUtNDEyZC1hNDg0LTMyOTI1MGRjMDU5YbBJc0ZpcnN0TG9naW5UaW1lwq5EYXRhVXBkYXRlVGltZc5na+v/r0xhc3RSZWZyZXNoVGltZc5na+v/rlBsYXllclJvbGVEYXRhiKRuYW1loLN0b3RhbE9ubGluZUR1cmF0aW9uALN0b2RheU9ubGluZUR1cmF0aW9uAKtkaWFsb2d1ZUlkc4CpZXF1aXBJbmZvgKxiYWdXYXJlSG91c2WArmVxdWlwV2FyZUhvdXNlgKhyb2xlQXR0coaqcm9sZV9sZXZlbAGocm9sZV9leHAAqXJvbGVfaGVhdACpcm9sZV9jb2luAKlyb2xlX3Nub3cArXJvbGVfbGl2ZW5lc3MA");
        
        string hexString = BitConverter.ToString(test).Replace("-", " ");
        GameUtil.LogError($"字节数组内容: {hexString}");
        
        try
        {
            DataManager mc2 = MessagePackSerializer.Deserialize<DataManager>(test);
            GameUtil.LogError("序列化成功");
            PropertyInfo[] properties = mc2.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(mc2);
                var targetProperty = this.GetType().GetProperty(property.Name);
                GameUtil.LogError($"设置{property.Name}======={value}");
                if (targetProperty != null && targetProperty.CanWrite)
                {
                    targetProperty.SetValue(this, value);
                }
            }
        }
        catch (MessagePackSerializationException ex)
        {
            GameUtil.LogError($"初始化失败 {ex.Message}===》用默认值");
        }
        
        // 获取 Unity 的持久存储路径
        string directoryPath = Path.Combine(Application.persistentDataPath, "GameSaves");
        // 确保目录存在，如果不存在则创建
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        // 设置文件名
        string fileName = "game_save.bin";
        // 获取完整的文件路径
        string fullFilePath = Path.Combine(directoryPath, fileName);
        // 保存二进制数据到文件
        File.WriteAllBytes(fullFilePath, test);
        // 输出保存路径以供调试
        Debug.Log($"Game data saved to: {fullFilePath}");

        
        //string filePath = Path.Combine(Application.persistentDataPath, "Logs.txt");
        //GameUtil.CompressFile($"{filePath}", null, null);
        //GameEntry.SDK.UploadLogData("1111111111");


        // Dictionary<string,object> dic = new Dictionary<string,object>();
        // dic.Add("测试数据1","家电");
        // TalkingDataSDK.OnEvent("游戏埋点数据",dic,null);

#if !UNITY_EDITOR
        ShowToastMessage("你好呀");
#endif
    }
    
    public void ShowToastMessage(string message)
    {
        using (AndroidJavaClass toastUtil = new AndroidJavaClass("com.example.mylibrary.ToastUtil"))
        {
            // 获取当前的 Activity
            using (AndroidJavaObject currentActivity = GetUnityActivity())
            {
                // 调用 showToast 方法
                toastUtil.CallStatic("showToast", currentActivity, message);
            }
        }
    }

    // 获取 Unity 的 Activity 对象
    private AndroidJavaObject GetUnityActivity()
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }

    public async UniTask LoadLoginBg()
    {
        // Texture2D texture = await GameEntry.Loader.LoadMainAssetAsync<Texture2D>($"Assets/Game/Download/Textures/BackGround/LoginBg.png",this.gameObject);
        // if (texture != null)
        // {
        //     // 将纹理转换为 Sprite
        //     Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        //     // 将加载的 sprite 赋值给 Image 组件
        //     GameUtil.LogError(bgImage == null,"===",sprite == null);
        //     bgImage.sprite = sprite;
        // }
    }

}

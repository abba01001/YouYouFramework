using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Main;
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
        loginBtn.SetButtonClick(Login);
#if !UNITY_EDITOR
        //GameUtil.GetSignatureMD5Hash();
#endif
        //GameEntry.SDK.DownloadAvatar("1", null);
    }

    private void Login()
    {
        //if(account.text == "" || password.text == "") return;
        //GameEntry.SDK.LoginAsync(account.text, password.text);
        // TalkingDataSDK.BackgroundSessionEnabled();
        // TalkingDataSDK.InitSDK(Constants.TalkingDataAppid,"101","");
        //
        // //用户获得隐私授权后才能调用StartA()
        // TalkingDataSDK.StartA();
        //
        // GameUtil.LogError("初始化TalkingDataSDK");
        //
        // Dictionary<string,object> dic = new Dictionary<string,object>();
        // dic.Add("测试数据1","家电");
        // TalkingDataSDK.OnEvent("游戏埋点数据",dic,null);
        MainEntry.Reporter.ShowLogPanel(true);

        ShowToastMessage("你好呀");
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

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
        // UnityEngine.AndroidJavaObject javaObject = default;
        // AndroidJavaClass javaClass = new AndroidJavaClass("com.example.mylibrary.Install");
        // javaClass.CallStatic<bool>("安装apk", "");
        // javaClass.GetStatic<AndroidJavaObject>("currentActivity");
        //下面呢？
        loginBtn.SetButtonClick(Login);
    }

    private void Login()
    {
        StartCoroutine(DownloadAndInstall("https://abab01001-1318826377.cos.ap-guangzhou.myqcloud.com/APK/Demo.apk"));
        //if(account.text == "" || password.text == "") return;
        //GameEntry.SQL.Login(account.text, password.text);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // jumpBtn.GetComponent<Button>().SetButtonClick(() =>
        // {
        //     jumpBtn.SetImage("Assets/Game/Download/Atlas/Textures/Common","JoyBg.png",true);
        // });
        //GameEntry.Event.AddEventListener(EventName.LoadingSceneUpdate, OnLoadingProgressChange);

        //txtTip.text = string.Empty;
        //m_Scrollbar.size = 0;
        //jumpBtn.SetSpriteByAtlas("Common","JoyKnob",true);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        //GameEntry.Event.RemoveEventListener(EventName.LoadingSceneUpdate, OnLoadingProgressChange);
    }
    
    private string apkFileName = "game.apk"; // APK 文件名，放在 StreamingAssets 中
      public IEnumerator DownloadAndInstall(string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SendWebRequest();
            // 循环检查进度
            while (!www.isDone)
            {
                GameUtil.LogError($"下载进度: {www.downloadProgress * 100}%");
                yield return null; // 等待下一帧
            }

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                GameUtil.LogError(www.error);
            }
            else
            {
                // 将 APK 数据保存到临时文件
                string tempPath = Path.Combine(Application.persistentDataPath, apkFileName);
                if (File.Exists(tempPath)) File.Delete(tempPath);
                File.WriteAllBytes(tempPath, www.downloadHandler.data);
                // 调用内部安装方法
                InstallAPKInternal(tempPath);
            }
        }
    }

    private bool InstallAPKInternal(string apkPath)
    {
        AndroidJavaClass javaClass = new AndroidJavaClass("com.example.mylibrary.Install");
        return javaClass.CallStatic<bool>("安装apk", apkPath);
    }
}

using Main;
using System;
using System.Collections;
using System.Collections.Generic;
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

    private void InstallAPKInternal(string apkPath)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            
            // 检查 APK 文件是否存在
            if (!File.Exists(apkPath))
            {
                Debug.LogError("APK 文件不存在: " + apkPath);
                return;
            }

            // 检查是否允许从未知来源安装
            bool canInstall = CheckInstallPermission(activity);
            if (!canInstall)
            {
                // 提示用户去设置
                OpenInstallSettings();
                return;
            }

            // 创建 Intent 进行安装
            using (AndroidJavaObject intent =
 new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW"))
            {
                AndroidJavaObject file = new AndroidJavaObject("java.io.File", apkPath);
            // 创建 AndroidJavaClass 实例来调用静态方法
            using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
            {
                AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject>("fromFile", file);
                intent.Call<AndroidJavaObject>("setData", uri);
            }

            intent.Call("addFlags", 268435456); // FLAG_ACTIVITY_NEW_TASK
            activity.Call("startActivity", intent);
            }
        }
#endif
    }

    private bool CheckInstallPermission(AndroidJavaObject activity)
    {
        // 检查是否允许从未知来源安装
        // 这里可以根据你的需求实现具体的检查逻辑
        // 此处是一个伪代码示例，实际需要根据不同设备的 API 进行检查
        return true; // 这里假设返回true，表示有权限
    }

    private void OpenInstallSettings()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
    {
        string packageName = Application.identifier;
        AndroidJavaObject intent = intentClass.CallStatic<AndroidJavaObject>("newIntent", "android.provider.Settings.ACTION_MANAGE_UNKNOWN_APP_SOURCES");
        
        // 创建 URI
        using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
        {
            AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null);
            intent.Call<AndroidJavaObject>("setData", uri);
        }

        intent.Call("addFlags", 268435456); // FLAG_ACTIVITY_NEW_TASK
        
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("startActivity", intent);
        }
    }
#endif
    }
    
}

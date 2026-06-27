using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Main;
using MessagePack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameScripts
{
    // "登录"界面
    public class FormLogin : UIFormBase
    {
        // [SerializeField] private TMP_InputField account;
        // [SerializeField] private TMP_InputField password;
        [SerializeField] private Button loginBtn;
        protected override async UniTask Awake()
        {
            await base.Awake();

            loginBtn.SetButtonClick(() => { Login(); });

#if !UNITY_EDITOR
            //GameUtil.GetSignatureMD5Hash();

#endif
            Login();
            //GameEntry.SDK.DownloadAvatar("1", null);
        }


        private async Task Login()
        {
            await UniTask.Delay(200);
            GameEntry.Event.Dispatch(Constants.EventName.LoginSuccess);
            Constants.IsEntryGame = true;
            //
            // await NetManager.Instance.ConnectServerAsync(false);
            // NetManager.Instance.Requset.c2s_request_login("a888888", "99999");
            return;
            //if(account.text == "" || password.text == "") return;
            //GameEntry.SDK.LoginAsync(account.text, password.text);
            //loginBtn.GetComponent<Image>().SetImage("Assets/Game/Download/Atlas/Textures/Common","JoyBg.png",true);
            // GameEntry.Net.Requset.c2s_request_register("a888888","99999");
            if (true || MainEntry.IsOfflineMode)
            {
                string result = PlayerPrefs.GetString("SaveData");
                byte[] binaryData = Convert.FromBase64String(result);
                GameEntry.Data.InitGameData(binaryData);
                long timestampSeconds = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                //GameEntry.Time.InitNetTime(timestampSeconds);
                await UniTask.Delay(100);
                GameEntry.Event.Dispatch(Constants.EventName.LoginSuccess);
                Constants.IsEntryGame = true;
            }
            else
            {
            }
        }
    }
}
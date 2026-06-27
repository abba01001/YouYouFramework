using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    public class AndroidManager : MonoBehaviour
    {
        public static AndroidManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    
        // --- 原有业务逻辑：安装结果回调 ---
        public void OnInstallResult(string message)
        {
            Debugger.Log($"[C#] 收到安卓层消息: {message}");
            if (message == "canceled")
            {
                ShowUpdateCancelDialog();
            }
        }
        
        private void ShowUpdateCancelDialog()
        {
            Debugger.LogError("由于你取消了更新，游戏即将退出。");
            Application.Quit();
        }
    }
}

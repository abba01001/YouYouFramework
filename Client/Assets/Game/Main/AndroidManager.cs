using UnityEngine;

public class AndroidManager : MonoBehaviour
{
    public static AndroidManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void OnInstallResult(string message)
    {
        Debug.Log($"[C#] 收到安卓层消息: {message}");

        if (message == "canceled")
        {
            ShowUpdateCancelDialog();
        }
    }

    private void ShowUpdateCancelDialog()
    {
        Debug.LogError("由于你取消了更新，游戏即将退出。");
        Application.Quit();
    }
}
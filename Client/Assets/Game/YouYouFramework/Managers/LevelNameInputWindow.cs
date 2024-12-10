using UnityEditor;
using UnityEngine;

public class LevelNameInputWindow : EditorWindow
{
    private string levelName = ""; // 用户输入的关卡名
    private System.Action<string> onLevelNameEntered; // 输入完成后的回调
    private bool isValid = true; // 标记关卡名称是否有效

    // 打开输入框窗口
    public static void OpenWindow(System.Action<string> onLevelNameEntered)
    {
        LevelNameInputWindow window = GetWindow<LevelNameInputWindow>("输入关卡名");
        window.onLevelNameEntered = onLevelNameEntered;
        window.Show();
    }

    public static void CloseWindow()
    {
        LevelNameInputWindow window = GetWindow<LevelNameInputWindow>("输入关卡名");
        window.Close();
    }

    // 绘制GUI
    private void OnGUI()
    {
        GUILayout.Label("请输入关卡名称:", EditorStyles.boldLabel);

        levelName = EditorGUILayout.TextField("关卡名", levelName);

        // 如果无效，给出错误提示
        if (!isValid)
        {
            EditorGUILayout.HelpBox("关卡名称无效，请重新输入。", MessageType.Error);
        }

        GUILayout.Space(10);

        // 确认按钮
        if (GUILayout.Button("确定"))
        {
            if (!string.IsNullOrEmpty(levelName))
            {
                isValid = true;
                onLevelNameEntered?.Invoke(levelName); // 输入完成，执行回调
            }
            else
            {
                isValid = false; // 无效输入
            }
        }

        // 关闭窗口按钮
        if (GUILayout.Button("取消"))
        {
            EditorApplication.isPlaying = false; 
            Close(); // 取消时关闭窗口
        }
    }

    // 用于重新验证并保持窗口打开
    public void SetInvalid()
    {
        isValid = false;
    }
}

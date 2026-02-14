using System;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    private string logContent = "";  // 用于存储日志
    private Vector2 scrollPosition;  // 滚动条位置
    private bool showLogWindow = false;  // 控制日志窗口的显示
    private GUIStyle logStyle;  // 用于设置字体样式
    private const float scrollSpeed = 10f;  // 滚动速度限制

    private void Awake()
    {
        // 监听 Unity 日志消息
        Application.logMessageReceived += HandleLog;
        logStyle = new GUIStyle();
        logStyle.fontSize = 26;  // 增大字体大小
        logStyle.normal.textColor = Color.white;  // 默认字体颜色
        logStyle.wordWrap = true;  // 允许自动换行
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }
    
    private const int MaxLogLines = 100;
    private readonly Queue<string> logLines = new Queue<string>();
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string newLine;

        if (type == LogType.Error || type == LogType.Exception)
        {
            newLine = $"<color=red>{logString}</color>\n<color=red>{stackTrace}</color>";
        }
        else
        {
            newLine = logString;
        }

        // 添加新日志
        logLines.Enqueue(newLine);

        // 超过最大行数时移除最早的一行
        while (logLines.Count > MaxLogLines)
        {
            logLines.Dequeue();
        }

        // 重新拼接
        logContent = string.Join("\n", logLines);
    }

    void OnGUI()
    {
        // 添加一个按钮来控制日志窗口的显示与隐藏，按钮大小增加一倍
        if (GUI.Button(new Rect(10, 10, 200, 60), "日志开关"))  // 改为“日志开关”
        {
            showLogWindow = !showLogWindow;
        }

        if (showLogWindow)
        {
            DrawLogWindow();
        }

        // 检测触摸并处理滑动
        HandleTouchScroll();
    }

    // 绘制日志窗口
    void DrawLogWindow()
    {
        // 定义日志窗口区域：屏幕的左上角，大小为屏幕的 1/2
        float windowWidth = Screen.width / 2;
        float windowHeight = Screen.height / 2;
        GUILayout.BeginArea(new Rect(10, 80, windowWidth, windowHeight));  // 调整位置以避免与按钮重叠
        GUILayout.Label("日志窗口", GUILayout.Height(20));

        // 使用 ScrollView 来显示可滚动的日志内容
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(windowWidth), GUILayout.Height(windowHeight - 30));

        // 显示日志文本，并应用自定义字体样式，设置宽度以便换行
        GUILayout.Label(logContent, logStyle, GUILayout.Width(windowWidth - 20));  // 宽度要减去一些以避免滚动条覆盖

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    // 处理触摸滑动的逻辑
    void HandleTouchScroll()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                // 根据触摸的垂直移动量调整 scrollPosition
                scrollPosition.y += touch.deltaPosition.y;
            }
        }
    }
}
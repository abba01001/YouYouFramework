using System;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    private float deltaTime = 0.0f;

    private void Start()
    {
        Application.targetFrameRate = 120; 
    }

    void Update()
    {
        // 每一帧计算帧时间
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        // 计算 FPS
        float fps = 1.0f / deltaTime;
        
        // 显示 FPS，位置为右上角
        GUIStyle style = new GUIStyle();
        style.fontSize = 30; // 设置字体大小
        style.normal.textColor = Color.white; // 设置字体颜色
        style.alignment = TextAnchor.UpperRight; // 设置文本对齐方式（右上角）
        
        // 显示 FPS
        GUI.Label(new Rect(Screen.width - 120, 30, 100, 30), "FPS: " + Mathf.Ceil(fps).ToString(), style);
    }
}
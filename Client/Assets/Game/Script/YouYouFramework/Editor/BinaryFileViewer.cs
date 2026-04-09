using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;  // 引入 Newtonsoft.Json 命名空间

public class BinaryFileViewer : EditorWindow
{
    private string fileContent = "";  // 用于展示文件内容

    [MenuItem("Assets/工具/查看二进制文件为JSON")]
    public static void ShowWindow()
    {
        // 创建一个新的编辑器窗口
        EditorWindow.GetWindow<BinaryFileViewer>("Binary 文件查看器");
    }

    private void OnGUI()
    {
        // 显示文件路径和按钮
        EditorGUILayout.LabelField("选择要查看的二进制文件", EditorStyles.boldLabel);

        // 当前选中的对象
        Object selectedObject = Selection.activeObject;
        if (selectedObject != null && AssetDatabase.GetAssetPath(selectedObject).EndsWith(".bytes"))
        {
            string path = AssetDatabase.GetAssetPath(selectedObject);

            if (GUILayout.Button("加载文件"))
            {
                LoadFile(path);
            }
        }
        else
        {
            EditorGUILayout.LabelField("请选中一个二进制文件(.bytes)进行查看");
        }

        // 显示解析后的 JSON 内容
        EditorGUILayout.LabelField("文件内容 (JSON 格式)：", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(fileContent, GUILayout.Height(300));
    }

    private void LoadFile(string path)
    {
        try
        {
            // 读取二进制文件内容
            byte[] fileBytes = File.ReadAllBytes(path);

            // 将二进制文件转换为 MMO_MemoryStream 流
            MMO_MemoryStream ms = new MMO_MemoryStream();
            ms.Write(fileBytes, 0, fileBytes.Length);
            ms.Position = 0;  // 重置流的位置

            // 解析文件内容，假设你有一个 LoadList 的方法来处理
            LoadList(ms);

            // 使用 Newtonsoft.Json 将字典转换为 JSON 格式字符串
            string jsonString = JsonConvert.SerializeObject(LocalizationDic, Formatting.Indented);
            fileContent = jsonString;  // 显示 JSON 内容
        }
        catch (System.Exception e)
        {
            fileContent = "加载文件时出错: " + e.Message;
        }
    }

    // 模拟的 LoadList 方法，使用 MMO_MemoryStream 解析文件
    private Dictionary<string, string> LocalizationDic = new Dictionary<string, string>();

    protected void LoadList(MMO_MemoryStream ms)
    {
        int rows = ms.ReadInt();
        int columns = ms.ReadInt();

        for (int i = 0; i < rows; i++)
        {
            string key = ms.ReadUTF8String();
            string value = ms.ReadUTF8String();
            LocalizationDic[key] = value;
        }
    }
}

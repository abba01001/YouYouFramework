using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "框架ScriptableObject/CommonToolsSettings")]
public class CommonToolsEditor : ScriptableObject
{
    [Serializable]
    public struct ToolEntry
    {
        [HorizontalGroup, HideLabel]
        [DisplayAsString]
        public string ToolName;

        [HorizontalGroup, HideLabel]
        public Action Action;

        public ToolEntry(string name, Action action)
        {
            this.ToolName = name;
            this.Action = action;
        }
    }

    // 这里存放所有的工具映射
    private List<ToolEntry> _allTools;

    [OnInspectorInit]
    private void InitTools()
    {
        // 🔥 核心：在这里一行代码添加一个功能，不需要写新方法
        _allTools = new List<ToolEntry>
        {
            new ("GM面板", GMEditorPanel.OpenWindow),
            new ("工程资源大小查看器", AssetSizeViewer.ShowWindow),
            new ("检查丢失脚本的预制体", InvalidScriptChecker.OpenWindow),
            new ("生成多语言图集", TMPAtlasBuilder.ShowWindow),
            new ("NameSpace域名管理工具", NamespaceManagerWindow.ShowWindow),
            new ("YooAsset 批量查看器",YooAssetBatchViewer.ShowWindow),
            // 新加功能只需要在这里加一行
            new ("新功能示例", () => Debug.Log("执行了自定义逻辑")) 
        };
    }

    [OnInspectorGUI]
    private void DrawModernButtons()
    {
        if (_allTools == null) InitTools();

        GUILayout.Space(10);
        var style = new GUIStyle(GUI.skin.button);
        style.fixedHeight = 35;
        style.margin = new RectOffset(4, 4, 4, 4);

        // 使用 Odin 的布局控制
        EditorGUILayout.BeginHorizontal();
        
        // 左列
        EditorGUILayout.BeginVertical();
        for (int i = 0; i < _allTools.Count; i += 2) DrawButton(_allTools[i], style);
        EditorGUILayout.EndVertical();

        // 右列
        EditorGUILayout.BeginVertical();
        for (int i = 1; i < _allTools.Count; i += 2) DrawButton(_allTools[i], style);
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawButton(ToolEntry entry, GUIStyle style)
    {
        if (GUILayout.Button(entry.ToolName, style))
        {
            entry.Action?.Invoke();
        }
    }
}
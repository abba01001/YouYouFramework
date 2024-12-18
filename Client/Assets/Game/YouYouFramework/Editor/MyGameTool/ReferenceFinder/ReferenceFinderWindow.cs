﻿using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

public class ReferenceFinderWindow : EditorWindow
{
    const string isDependPrefKey = "ReferenceFinderData_IsDepend";
    const string needUpdateStatePrefKey = "ReferenceFinderData_needUpdateState";

    private static ReferenceFinderData data = new ReferenceFinderData();
    private static bool initializedData = false;

    private bool isDepend = false;
    private bool needUpdateState = true;

    private bool needUpdateAssetTree = false;
    private bool initializedGUIStyle = false;

    private GUIStyle toolbarButtonGUIStyle;
    private GUIStyle toolbarGUIStyle;
    private List<string> selectedAssetGuid = new List<string>();

    private AssetTreeView m_AssetTreeView;

    [SerializeField] private TreeViewState m_TreeViewState;

    [MenuItem("Assets/工具/寻找该资源的引用 %#&f", false, 25)]
    static void FindRef()
    {
        InitDataIfNeeded();
        OpenWindow();
        ReferenceFinderWindow window = GetWindow<ReferenceFinderWindow>();
        window.UpdateSelectedAssets();
    }

    static void OpenWindow()
    {
        ReferenceFinderWindow window = GetWindow<ReferenceFinderWindow>();
        window.wantsMouseMove = false;
        window.titleContent = new GUIContent("Ref Finder");
        window.Show();
        window.Focus();
    }

    static void InitDataIfNeeded()
    {
        if (!initializedData)
        {
            if (!data.ReadFromCache())
            {
                data.CollectDependenciesInfo();
            }

            initializedData = true;
        }
    }

    void InitGUIStyleIfNeeded()
    {
        if (!initializedGUIStyle)
        {
            toolbarButtonGUIStyle = new GUIStyle("ToolbarButton");
            toolbarGUIStyle = new GUIStyle("Toolbar");
            initializedGUIStyle = true;
        }
    }

    private void OnEnable()
    {
        isDepend = PlayerPrefs.GetInt(isDependPrefKey, 0) == 1;
        needUpdateState = PlayerPrefs.GetInt(needUpdateStatePrefKey, 1) == 1;
        Selection.selectionChanged += UpdateSelectedAssets; // 注册选择更改事件
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= UpdateSelectedAssets; // 取消注册选择更改事件
    }

    private void UpdateSelectedAssets()
    {
        selectedAssetGuid.Clear();
        foreach (var obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (Directory.Exists(path))
            {
                string[] folder = new string[] {path};
                string[] guids = AssetDatabase.FindAssets(null, folder);
                foreach (var guid in guids)
                {
                    if (!selectedAssetGuid.Contains(guid) &&
                        !Directory.Exists(AssetDatabase.GUIDToAssetPath(guid)))
                    {
                        selectedAssetGuid.Add(guid);
                    }
                }
            }
            else
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                selectedAssetGuid.Add(guid);
            }
        }

        needUpdateAssetTree = true;
        Repaint(); // 更新UI
    }

    private void UpdateAssetTree()
    {
        if (needUpdateAssetTree && selectedAssetGuid.Count != 0)
        {
            var root = SelectedAssetGuidToRootItem(selectedAssetGuid);
            if (m_AssetTreeView == null)
            {
                if (m_TreeViewState == null)
                    m_TreeViewState = new TreeViewState();
                var headerState = AssetTreeView.CreateDefaultMultiColumnHeaderState(position.width);
                var multiColumnHeader = new MultiColumnHeader(headerState);
                m_AssetTreeView = new AssetTreeView(m_TreeViewState, multiColumnHeader);
            }

            m_AssetTreeView.assetRoot = root;
            m_AssetTreeView.Reload();

            // 自动展开所有节点
            m_AssetTreeView.ExpandAll();

            needUpdateAssetTree = false;
        }
    }


    private void OnGUI()
    {
        InitGUIStyleIfNeeded();
        DrawOptionBar();
        UpdateAssetTree();
        if (m_AssetTreeView != null)
        {
            m_AssetTreeView.OnGUI(new Rect(0, toolbarGUIStyle.fixedHeight, position.width,
                position.height - toolbarGUIStyle.fixedHeight));
        }
    }

    public void DrawOptionBar()
    {
        EditorGUILayout.BeginHorizontal(toolbarGUIStyle);
        if (GUILayout.Button("更新项目资源数据", toolbarButtonGUIStyle))
        {
            data.CollectDependenciesInfo();
            needUpdateAssetTree = true;
            EditorGUIUtility.ExitGUI();
        }

        bool PreIsDepend = isDepend;
        isDepend = GUILayout.Toggle(isDepend, isDepend ? "查找依赖(当前模式)" : "查找引用(当前模式)", toolbarButtonGUIStyle,
            GUILayout.Width(150));
        if (PreIsDepend != isDepend)
        {
            OnModelSelect();
        }

        bool PreNeedUpdateState = needUpdateState;
        needUpdateState = GUILayout.Toggle(needUpdateState, "是否更新状态", toolbarButtonGUIStyle);
        if (PreNeedUpdateState != needUpdateState)
        {
            PlayerPrefs.SetInt(needUpdateStatePrefKey, needUpdateState ? 1 : 0);
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Expand", toolbarButtonGUIStyle))
        {
            if (m_AssetTreeView != null) m_AssetTreeView.ExpandAll();
        }

        if (GUILayout.Button("Collapse", toolbarButtonGUIStyle))
        {
            if (m_AssetTreeView != null) m_AssetTreeView.CollapseAll();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void OnModelSelect()
    {
        needUpdateAssetTree = true;
        PlayerPrefs.SetInt(isDependPrefKey, isDepend ? 1 : 0);
    }

    private HashSet<string> updatedAssetSet = new HashSet<string>();

    private AssetViewItem SelectedAssetGuidToRootItem(List<string> selectedAssetGuid)
    {
        updatedAssetSet.Clear();
        int elementCount = 0;
        var root = new AssetViewItem {id = elementCount, depth = -1, displayName = "Root", data = null};
        int depth = 0;
        var stack = new Stack<string>();
        foreach (var childGuid in selectedAssetGuid)
        {
            var child = CreateTree(childGuid, ref elementCount, depth, stack);
            if (child != null)
                root.AddChild(child);
        }

        updatedAssetSet.Clear();
        return root;
    }

    private AssetViewItem CreateTree(string guid, ref int elementCount, int _depth, Stack<string> stack)
    {
        if (stack.Contains(guid))
            return null;

        stack.Push(guid);
        if (needUpdateState && !updatedAssetSet.Contains(guid))
        {
            data.UpdateAssetState(guid);
            updatedAssetSet.Add(guid);
        }

        ++elementCount;
        var referenceData = data.assetDict[guid];
        var root = new AssetViewItem
            {id = elementCount, displayName = referenceData.name, data = referenceData, depth = _depth};
        var childGuids = isDepend ? referenceData.dependencies : referenceData.references;
        foreach (var childGuid in childGuids)
        {
            var child = CreateTree(childGuid, ref elementCount, _depth + 1, stack);
            if (child != null)
                root.AddChild(child);
        }

        stack.Pop();
        return root;
    }
}
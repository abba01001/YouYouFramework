using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class StripLinkConfigWindow : EditorWindow
{
    private class ItemData
    {
        public bool isOn;
        public string dllName;
        public ItemData(bool isOn, string dllName)
        {
            this.isOn = isOn;
            this.dllName = dllName;
        }
    }
    private Vector2 scrollPosition;
    private string[] selectedDllList;
    private List<ItemData> dataList;
    private GUIStyle normalStyle;
    private GUIStyle selectedStyle;
    private void OnEnable()
    {
        normalStyle = new GUIStyle();
        normalStyle.normal.textColor = Color.white;

        selectedStyle = new GUIStyle();
        selectedStyle.normal.textColor = Color.green;
        dataList = new List<ItemData>();
        RefreshListData();
    }
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        if (dataList.Count <= 0)
        {
            EditorGUILayout.HelpBox("未找到程序集,请先Build项目以生成程序集.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("勾选需要添加到Link.xml的程序集,然后点击保存生效.", MessageType.Info);
        }
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);
        for (int i = 0; i < dataList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            var item = dataList[i];
            item.isOn = EditorGUILayout.ToggleLeft(item.dllName, item.isOn, item.isOn ? selectedStyle : normalStyle);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select All", GUILayout.Width(100)))
        {
            SelectAll(true);
        }
        if (GUILayout.Button("Cancel All", GUILayout.Width(100)))
        {
            SelectAll(false);
        }
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Reload", GUILayout.Width(120)))
        {
            RefreshListData();
        }
        if (GUILayout.Button("Save", GUILayout.Width(120)))
        {
            if (MyGameTools.Save2LinkFile(GetCurrentSelectedList()))
            {
                EditorUtility.DisplayDialog("Strip LinkConfig Editor", "Update link.xml success!", "OK");
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    private void SelectAll(bool isOn)
    {
        foreach (var item in dataList)
        {
            item.isOn = isOn;
        }
    }
    private string[] GetCurrentSelectedList()
    {
        List<string> result = new List<string>();
        foreach (var item in dataList)
        {
            if (item.isOn)
            {
                result.Add(item.dllName);
            }
        }
        return result.ToArray();
    }
    private void RefreshListData()
    {
        dataList.Clear();
        selectedDllList = MyGameTools.GetSelectedAssemblyDlls();
        foreach (var item in MyGameTools.GetProjectAssemblyDlls())
        {
            dataList.Add(new ItemData(IsInSelectedList(item), item));
        }
    }
    private bool IsInSelectedList(string dllName)
    {
        return ArrayUtility.Contains(selectedDllList, dllName);
    }
}
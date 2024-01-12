using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace SK.Framework
{
    public class AssetsStatistics : EditorWindow
    {
        [MenuItem("SKFramework/Assets Statistics")]
        private static void Open()
        {
            GetWindow<AssetsStatistics>("Assets Statistics").Show();
        }

        private Vector2 selectedListScroll;
        //当前选中项索引
        private int currentSelectedIndex = -1;

        private enum Mode
        {
            Dependence,
            Reference,
        }
        private Mode mode = Mode.Dependence;

        private Vector2 dependenceListScroll;
        private Vector2 referenceListScroll;

        private string[] dependenciesArray;
        private string[] referenceArray;

        private void OnGUI()
        {
            OnListGUI();

            OnMenuGUI();
        }

        private void OnListGUI()
        {
            if (Selection.assetGUIDs.Length == 0) return;
            selectedListScroll = EditorGUILayout.BeginScrollView(selectedListScroll);
            for (int i = 0; i < Selection.assetGUIDs.Length; i++)
            {
                //通过guid获取资产路径
                string path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]);
                GUILayout.BeginHorizontal(currentSelectedIndex == i ? "SelectionRect" : "dragtab first");
                //获取资产类型
                Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
                GUILayout.Label(EditorGUIUtility.IconContent(GetIconName(type.Name)), GUILayout.Width(20f), GUILayout.Height(15f));
                GUILayout.Label(path);
                //点击选中
                if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    currentSelectedIndex = i;
                    Event.current.Use();
                    GetDependencies();
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
        private void OnMenuGUI()
        {
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical("Box", GUILayout.Height(position.height * .7f));
            {
                GUILayout.BeginHorizontal();
                {
                    Color color = GUI.color;
                    GUI.color = mode == Mode.Dependence ? color : Color.gray;
                    if (GUILayout.Button("依赖", "ButtonLeft"))
                    {
                        mode = Mode.Dependence;
                    }
                    GUI.color = mode == Mode.Reference ? color : Color.gray;
                    if (GUILayout.Button("引用", "ButtonRight"))
                    {
                        mode = Mode.Reference;
                    }
                    GUI.color = color;
                }
                GUILayout.EndHorizontal();

                switch (mode)
                {
                    case Mode.Dependence: OnDependenceGUI(); break;
                    case Mode.Reference: OnReferenceGUI(); break;
                }
            }
            GUILayout.EndVertical();
        }
        private void GetDependencies()
        {
            string guid = Selection.assetGUIDs[currentSelectedIndex];
            string path = AssetDatabase.GUIDToAssetPath(guid);
            dependenciesArray = AssetDatabase.GetDependencies(path);
        }
        private void OnDependenceGUI()
        {
            EditorGUILayout.HelpBox("该资产的依赖项", MessageType.Info);
            if (currentSelectedIndex != -1)
            {
                dependenceListScroll = EditorGUILayout.BeginScrollView(dependenceListScroll);
                for (int i = 0; i < dependenciesArray.Length; i++)
                {
                    string dependency = dependenciesArray[i];
                    GUILayout.BeginHorizontal("dragtab first");
                    Type type = AssetDatabase.GetMainAssetTypeAtPath(dependency);
                    GUILayout.Label(EditorGUIUtility.IconContent(GetIconName(type.Name)), GUILayout.Width(20f), GUILayout.Height(15f));
                    GUILayout.Label(dependency);
                    if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    {
                        var obj = AssetDatabase.LoadAssetAtPath(dependency, type);
                        EditorGUIUtility.PingObject(obj);
                        Event.current.Use();
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
        }
        private void OnReferenceGUI()
        {
            EditorGUILayout.HelpBox("该资产的引用项（需点击刷新按钮获取，需要一定时间）", MessageType.Info);

            GUI.enabled = currentSelectedIndex != -1;
            if (GUILayout.Button("刷新"))
            {
                if (EditorUtility.DisplayDialog("提醒", "获取工程资产之间的引用关系需要一定时间，是否确定开始", "确定", "取消"))
                {
                    Dictionary<string, string[]> referenceDic = new Dictionary<string, string[]>();
                    string[] paths = AssetDatabase.GetAllAssetPaths();
                    for (int i = 0; i < paths.Length; i++)
                    {
                        referenceDic.Add(paths[i], AssetDatabase.GetDependencies(paths[i]));
                        EditorUtility.DisplayProgressBar("进度", "获取工程资产之间的依赖关系", i + 1 / paths.Length);
                    }
                    EditorUtility.ClearProgressBar();
                    string guid = Selection.assetGUIDs[currentSelectedIndex];
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    referenceArray = referenceDic.Where(m => m.Value.Contains(path)).Select(m => m.Key).ToArray();
                }
            }
            GUI.enabled = true;
            if (referenceArray != null)
            {
                referenceListScroll = EditorGUILayout.BeginScrollView(referenceListScroll);
                {
                    for (int i = 0; i < referenceArray.Length; i++)
                    {
                        string reference = referenceArray[i];
                        GUILayout.BeginHorizontal("dragtab first");
                        Type type = AssetDatabase.GetMainAssetTypeAtPath(reference);
                        GUILayout.Label(EditorGUIUtility.IconContent(GetIconName(type.Name)), GUILayout.Width(20f), GUILayout.Height(15f));
                        GUILayout.Label(reference);
                        if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        {
                            var obj = AssetDatabase.LoadAssetAtPath(reference, type);
                            EditorGUIUtility.PingObject(obj);
                            Event.current.Use();
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }
        private string GetIconName(string typeName)
        {
            switch (typeName)
            {
                case "Material": return "d_Material Icon";
                case "Mesh": return "d_Mesh Icon";
                case "AnimationClip": return "d_AnimationClip Icon";
                case "GameObject": return "d_Prefab Icon";
                case "Texture2D": return "d_Texture Icon";
                case "MonoScript": return "d_cs Script Icon";
                case "AnimatorController": return "d_AnimatorController Icon";
                case "DefaultAsset": return "d_DefaultAsset Icon";
                case "TextAsset": return "d_TextAsset Icon";
                case "TimelineAsset": return "d_UnityEditor.Timeline.TimelineWindow";
                default: return "d__Help@2x";
            }
        }
        private void OnSelectionChange()
        {
            currentSelectedIndex = -1;
            Repaint();
        }
    }
}
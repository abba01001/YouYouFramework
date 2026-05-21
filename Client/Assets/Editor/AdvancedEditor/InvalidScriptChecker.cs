using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class InvalidScriptChecker : EditorWindow
{
    private List<MissingScriptInfo> objectsWithMissingScripts = new List<MissingScriptInfo>();
    private Vector2 scrollPosition;

    [MenuItem("Tools/检查丢失脚本 (全项目+当前场景)")]
    public static void OpenWindow()
    {
        InvalidScriptChecker window = GetWindow<InvalidScriptChecker>("脚本丢失检查器");
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("操作面板", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("仅扫描当前场景", GUILayout.Height(30))) FindInScene();
        if (GUILayout.Button("全项目扫描 (Prefab)", GUILayout.Height(30))) FindInProject();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);
        GUILayout.Label("丢失脚本列表:", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        if (objectsWithMissingScripts.Count > 0)
        {
            foreach (var info in objectsWithMissingScripts)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.textArea);
                if (GUILayout.Button("选中", GUILayout.Width(40)))
                {
                    Selection.activeObject = info.targetObject;
                    EditorGUIUtility.PingObject(info.targetObject);
                }
                EditorGUILayout.ObjectField(info.targetObject, typeof(GameObject), true);
                EditorGUILayout.LabelField(info.sourceType, GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            GUILayout.Label("未发现异常。");
        }
        EditorGUILayout.EndScrollView();

        if (objectsWithMissingScripts.Count > 0)
        {
            GUI.color = Color.red;
            if (GUILayout.Button("尝试自动清理 (慎用)", GUILayout.Height(30))) RemoveMissingScripts();
            GUI.color = Color.white;
        }
    }

    // --- 核心逻辑：扫描场景 ---
    void FindInScene()
    {
        objectsWithMissingScripts.Clear();
        // 获取场景中所有根节点（包括隐藏的）
        GameObject[] rootObjs = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in rootObjs)
        {
            CheckGameObject(root, "Scene");
        }
        Repaint();
    }

    // --- 核心逻辑：扫描工程 Prefab ---
    void FindInProject()
    {
        objectsWithMissingScripts.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) CheckGameObject(prefab, "Prefab");
        }
        Repaint();
    }

    void CheckGameObject(GameObject go, string source)
    {
        Component[] components = go.GetComponentsInChildren<Component>(true);
        foreach (var comp in components)
        {
            // 如果组件本身为 null，说明脚本引用丢失
            if (comp == null)
            {
                objectsWithMissingScripts.Add(new MissingScriptInfo
                {
                    targetObject = go,
                    sourceType = source
                });
                Debug.LogWarning($"[{source}] 发现丢失脚本: {go.name}", go);
                break; 
            }
        }
    }

    void RemoveMissingScripts()
    {
        int count = 0;
        foreach (var info in objectsWithMissingScripts)
        {
            if (info.targetObject != null)
            {
                int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(info.targetObject);
                if (removed > 0)
                {
                    count += removed;
                    EditorUtility.SetDirty(info.targetObject);
                }
            }
        }
        
        // 如果是场景物体，标记场景已修改
        EditorSceneManager.MarkAllScenesDirty();
        // 如果是资产，保存资产
        AssetDatabase.SaveAssets();
        
        Debug.Log($"清理完成，共移除 {count} 个丢失的脚本组件。");
        objectsWithMissingScripts.Clear();
    }

    class MissingScriptInfo
    {
        public GameObject targetObject;
        public string sourceType; // "Scene" 或 "Prefab"
    }
}
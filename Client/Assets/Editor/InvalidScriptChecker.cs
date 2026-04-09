using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class InvalidScriptChecker : EditorWindow
{
    private List<MissingScriptInfo> objectsWithMissingScripts = new List<MissingScriptInfo>();
    private Vector2 scrollPosition;

    [MenuItem("Assets/工具/检查丢失脚本的预制体")]
    static void OpenWindow()
    {
        InvalidScriptChecker window = GetWindow<InvalidScriptChecker>("检查丢失脚本的预制体");
        window.Show();
        window.FindObjectsWithMissingScripts();
    }

    void OnGUI()
    {
        GUILayout.Label("Prefab 内部丢失脚本列表:", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        if (objectsWithMissingScripts.Count > 0)
        {
            foreach (var info in objectsWithMissingScripts)
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("选中", GUILayout.Width(40)))
                {
                    Selection.activeObject = info.targetObject;
                    EditorGUIUtility.PingObject(info.targetObject);
                }

                EditorGUILayout.ObjectField(info.targetObject, typeof(GameObject), true);
                GUILayout.Label($"路径: {info.hierarchyPath}");

                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            GUILayout.Label("没有发现丢失脚本的预制体。", EditorStyles.wordWrappedLabel);
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        if (GUILayout.Button("清理所有丢失脚本"))
        {
            RemoveMissingScripts();
        }
    }

    void FindObjectsWithMissingScripts()
    {
        objectsWithMissingScripts.Clear();
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string assetPath in assetPaths)
        {
            if (assetPath.EndsWith(".prefab"))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null) continue;

                Transform[] allTransforms = prefab.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allTransforms)
                {
                    var components = t.GetComponents<Component>();
                    foreach (var comp in components)
                    {
                        if (comp == null)
                        {
                            string path = GetHierarchyPath(t, prefab.transform);

                            // 添加到列表
                            objectsWithMissingScripts.Add(new MissingScriptInfo
                            {
                                prefabPath = assetPath,
                                targetObject = t.gameObject,
                                hierarchyPath = path
                            });

                            // ? 同时打印到 Console
                            Debug.Log($"发现丢失脚本: Prefab路径: {assetPath}, 节点路径: {path}, GameObject: {t.gameObject}", t.gameObject);
                            break; // 同一个节点只记录一次
                        }
                    }
                }
            }
        }

        Repaint();
    }

    void RemoveMissingScripts()
    {
        foreach (var info in objectsWithMissingScripts)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(info.targetObject);
            EditorUtility.SetDirty(info.targetObject);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("清理完毕！");
        Close();
    }

    string GetHierarchyPath(Transform target, Transform root)
    {
        List<string> pathParts = new List<string>();
        Transform current = target;
        while (current != null && current != root)
        {
            pathParts.Insert(0, current.name);
            current = current.parent;
        }

        if (current == root)
            pathParts.Insert(0, root.name);

        return string.Join("/", pathParts);
    }

    class MissingScriptInfo
    {
        public string prefabPath;
        public GameObject targetObject;
        public string hierarchyPath;
    }
}

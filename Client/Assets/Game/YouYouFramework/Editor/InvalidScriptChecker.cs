using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class InvalidScriptChecker : EditorWindow
{
    private List<GameObject> objectsWithMissingScripts = new List<GameObject>();
    private Vector2 scrollPosition;

    [MenuItem("YouYouTools/��鶪ʧ�ű���Ԥ����")]
    static void OpenWindow()
    {
        InvalidScriptChecker window = GetWindow<InvalidScriptChecker>("��鶪ʧ�ű���Ԥ����");
        window.Show();
        window.FindObjectsWithMissingScripts();
    }

    void OnGUI()
    {
        GUILayout.Label("Objects with Missing Scripts:", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        if (objectsWithMissingScripts.Count > 0)
        {
            foreach (GameObject go in objectsWithMissingScripts)
            {
                EditorGUILayout.ObjectField(go, typeof(GameObject), true);
            }
        }
        else
        {
            GUILayout.Label("No objects with missing scripts found.", EditorStyles.wordWrappedLabel);
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        if (GUILayout.Button("����ʧ�ű�"))
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
                if (prefab != null && GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab) > 0)
                {
                    objectsWithMissingScripts.Add(prefab);
                }
            }
        }

        Repaint();
    }

    void RemoveMissingScripts()
    {
        foreach (GameObject prefab in objectsWithMissingScripts)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab);
        }
        Debug.Log("�������");
        Close();
    }
}

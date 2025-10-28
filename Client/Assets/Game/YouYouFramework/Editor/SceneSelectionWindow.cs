using System.Linq;
using UnityEditor;
using UnityEngine;

public class SceneSelectionWindow : EditorWindow
{
    private bool[] selected;
    private string[] allScenes;

    public static string[] ShowWindow()
    {
        SceneSelectionWindow window = CreateInstance<SceneSelectionWindow>();
        window.titleContent = new GUIContent("选择打包场景");
        window.ShowModalUtility();
        return window.GetSelectedScenes();
    }

    private void OnEnable()
    {
        allScenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
        selected = new bool[allScenes.Length];

        // 默认选中 Scene_Launch.unity
        for (int i = 0; i < allScenes.Length; i++)
        {
            if (allScenes[i].EndsWith("Scene_Launch.unity"))
            {
                selected[i] = true;
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("请选择要打包的场景：", EditorStyles.boldLabel);

        for (int i = 0; i < allScenes.Length; i++)
        {
            bool isLaunch = allScenes[i].EndsWith("Scene_Launch.unity");

            if (isLaunch)
            {
                // 灰掉，固定勾选
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ToggleLeft(allScenes[i], true);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                selected[i] = EditorGUILayout.ToggleLeft(allScenes[i], selected[i]);
            }
        }

        GUILayout.Space(10);
        if (GUILayout.Button("确定"))
        {
            Close();
        }
    }

    private string[] GetSelectedScenes()
    {
        return allScenes.Where((s, i) => selected[i] || allScenes[i].EndsWith("Scene_Launch.unity")).ToArray();
    }
}
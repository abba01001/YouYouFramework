using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SceneSelectionWindow : EditorWindow
{
    private List<SceneEntry> sceneEntries = new List<SceneEntry>();
    private Vector2 scrollPos;
    private string searchFilter = "";
    
    private class SceneEntry
    {
        public string Path;
        public string Name;
        public bool Selected;
        public bool IsRequired; // 强制包含的场景（如 Launch）
    }

    public static string[] ShowWindow()
    {
        SceneSelectionWindow window = CreateInstance<SceneSelectionWindow>();
        window.titleContent = new GUIContent("打包场景选择", EditorGUIUtility.Load("SceneAsset Icon") as Texture2D);
        window.minSize = new Vector2(600, 500);
        window.ShowModalUtility();
        return window.GetSelectedScenes();
    }

    private void OnEnable()
    {
        var scenes = EditorBuildSettings.scenes;
        sceneEntries.Clear();

        foreach (var s in scenes)
        {
            bool isLaunch = s.path.EndsWith("Scene_Launch.unity");
            sceneEntries.Add(new SceneEntry
            {
                Path = s.path,
                Name = System.IO.Path.GetFileNameWithoutExtension(s.path),
                Selected = isLaunch,
                IsRequired = isLaunch
            });
        }
    }

    private void OnGUI()
    {
        // --- 顶部工具栏 ---
        DrawHeader();

        // --- 搜索框 ---
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);
        EditorGUILayout.EndHorizontal();

        // --- 场景列表 ---
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox);
        foreach (var entry in sceneEntries)
        {
            if (!string.IsNullOrEmpty(searchFilter) && entry.Name.ToLower().Contains(searchFilter.ToLower()))
                continue;

            DrawSceneItem(entry);
        }
        EditorGUILayout.EndScrollView();

        // --- 底部确定按钮 ---
        DrawFooter();
    }

    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("配置打包场景", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Scene_Launch 为强制包含场景。勾选其他场景以加入 Build。", MessageType.Info);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("全选", EditorStyles.miniButtonLeft)) sceneEntries.ForEach(s => { if(!s.IsRequired) s.Selected = true; });
        if (GUILayout.Button("取消全选", EditorStyles.miniButtonRight)) sceneEntries.ForEach(s => { if(!s.IsRequired) s.Selected = false; });
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }

    private void DrawSceneItem(SceneEntry entry)
    {
        // 获取当前绘制的矩形区域
        Rect rect = EditorGUILayout.BeginHorizontal(EditorStyles.textArea, GUILayout.Height(24));
    
        // --- 交互核心：点击整个框即选中 ---
        // 如果不是强制要求的场景，且用户点击了这一行
        if (!entry.IsRequired)
        {
            // 鼠标在当前矩形内按下且不是右键
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition) && Event.current.button == 0)
            {
                entry.Selected = !entry.Selected;
                Event.current.Use(); // 消耗掉事件，防止穿透
                GUI.FocusControl(null); // 清除焦点，强制刷新 UI 状态
            }

            // 鼠标滑过时显示高亮反馈（可选，增加视觉体验）
            if (rect.Contains(Event.current.mousePosition))
            {
                GUI.Box(rect, "", "SelectionRect"); 
            }
        }

        // --- 以下是内容绘制 ---
        GUILayout.Space(5);
    
        // 场景图标
        var icon = EditorGUIUtility.IconContent("SceneAsset Icon").image;
        GUILayout.Label(icon, GUILayout.Width(18), GUILayout.Height(18));

        // 勾选框（点击框依然有效）
        EditorGUI.BeginDisabledGroup(entry.IsRequired);
        // 这里改用原生的 Toggle，不带文字，因为文字我们后面单独画，这样布局更整齐
        entry.Selected = EditorGUILayout.Toggle(entry.Selected, GUILayout.Width(20));
        EditorGUI.EndDisabledGroup();

        // 场景名字
        GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
        if (entry.IsRequired) nameStyle.normal.textColor = Color.gray;
        EditorGUILayout.LabelField(entry.Name, nameStyle, GUILayout.Width(140));

        // 路径（淡化显示）
        GUIStyle pathStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.gray } };
        EditorGUILayout.LabelField(entry.Path, pathStyle);

        if (entry.IsRequired)
        {
            GUIStyle badgeStyle = new GUIStyle("AssetLabel Partial");
            badgeStyle.fixedHeight = 16;
            GUILayout.Label("Required", badgeStyle);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawFooter()
    {
        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("确定", GUILayout.Height(30)))
        {
            Close();
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);
    }

    private string[] GetSelectedScenes()
    {
        return sceneEntries.Where(e => e.Selected).Select(e => e.Path).ToArray();
    }
}
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class AssetSizeViewer : EditorWindow
{
    private class AssetInfo
    {
        public string path;
        public long size;
    }

    private List<AssetInfo> assets = new List<AssetInfo>();
    private Vector2 scroll;

    private int pageSize = 100;
    private int inputPageSize = 100;
    private int currentPage = 0;

    private string inputPath = "Assets";

    [MenuItem("Tools/资源大小查看器")]
    public static void ShowWindow()
    {
        GetWindow<AssetSizeViewer>("资源大小查看器");
    }

    private void OnGUI()
    {
        DrawTopBar();

        if (assets.Count == 0)
            return;

        DrawPagination();

        GUILayout.Space(5);

        DrawList();
    }

    private void DrawTopBar()
    {
        GUILayout.BeginVertical("box");

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("扫描", GUILayout.Width(100)))
        {
            ScanAssets();
        }

        GUILayout.Label($"总数: {assets.Count}", GUILayout.Width(120));

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUILayout.Label("扫描路径:", GUILayout.Width(70));
        inputPath = GUILayout.TextField(inputPath);

        if (GUILayout.Button("使用Assets", GUILayout.Width(100)))
        {
            inputPath = "Assets";
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUILayout.Label("每页数量:", GUILayout.Width(70));
        inputPageSize = EditorGUILayout.IntField(inputPageSize, GUILayout.Width(60));

        if (GUILayout.Button("应用", GUILayout.Width(60)))
        {
            ApplyPageSize();
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private void DrawPagination()
    {
        int totalPages = Mathf.CeilToInt((float)assets.Count / pageSize);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("<<", GUILayout.Width(40)))
            currentPage = 0;

        if (GUILayout.Button("<", GUILayout.Width(40)))
            currentPage = Mathf.Max(0, currentPage - 1);

        GUILayout.Label($"第 {currentPage + 1} / {totalPages} 页", GUILayout.Width(140));

        if (GUILayout.Button(">", GUILayout.Width(40)))
            currentPage = Mathf.Min(totalPages - 1, currentPage + 1);

        if (GUILayout.Button(">>", GUILayout.Width(40)))
            currentPage = totalPages - 1;

        GUILayout.EndHorizontal();
    }

    private void DrawList()
    {
        scroll = GUILayout.BeginScrollView(scroll);

        int start = currentPage * pageSize;
        int end = Mathf.Min(start + pageSize, assets.Count);

        for (int i = start; i < end; i++)
        {
            var asset = assets[i];

            GUILayout.BeginHorizontal();

            GUILayout.Label($"{i + 1}. {asset.path}", GUILayout.Width(position.width - 220));
            GUILayout.Label(FormatSize(asset.size), GUILayout.Width(80));

            if (GUILayout.Button("定位", GUILayout.Width(50)))
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(asset.path);
                Selection.activeObject = obj;
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }

    private void ScanAssets()
    {
        assets.Clear();
        currentPage = 0;

        if (string.IsNullOrEmpty(inputPath))
        {
            Debug.LogError("路径不能为空！");
            return;
        }

        if (!inputPath.StartsWith("Assets"))
        {
            Debug.LogError("路径必须以 Assets 开头！");
            return;
        }

        string fullPath = Path.Combine(Application.dataPath, inputPath.Replace("Assets", "").TrimStart('/', '\\'));

        if (!Directory.Exists(fullPath))
        {
            Debug.LogError($"路径不存在: {inputPath}");
            return;
        }

        var files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".meta"));

        foreach (var file in files)
        {
            FileInfo fi = new FileInfo(file);

            string assetPath = "Assets" + file.Replace(Application.dataPath, "").Replace("\\", "/");

            assets.Add(new AssetInfo
            {
                path = assetPath,
                size = fi.Length
            });
        }

        assets = assets.OrderByDescending(a => a.size).ToList();

        Debug.Log($"扫描完成：{inputPath}，共 {assets.Count} 个资源");
    }

    private void ApplyPageSize()
    {
        if (inputPageSize < 1)
            inputPageSize = 1;

        if (inputPageSize > 500)
            inputPageSize = 500;

        pageSize = inputPageSize;

        int totalPages = Mathf.CeilToInt((float)assets.Count / pageSize);
        currentPage = Mathf.Clamp(currentPage, 0, Mathf.Max(0, totalPages - 1));
    }

    private string FormatSize(long size)
    {
        if (size > 1024 * 1024)
            return (size / 1024f / 1024f).ToString("F2") + " MB";
        else
            return (size / 1024f).ToString("F2") + " KB";
    }
}
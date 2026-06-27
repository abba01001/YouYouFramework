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

    private List<AssetInfo> allAssets = new List<AssetInfo>();
    private List<AssetInfo> filteredAssets = new List<AssetInfo>();
    private Vector2 scroll;

    private int pageSize = 100;
    private int inputPageSize = 100;
    private int currentPage = 0;

    private string inputPath = "Assets";
    private string searchKeyword = "";

    [MenuItem("Tools/资源大小查看器")]
    public static void ShowWindow()
    {
        GetWindow<AssetSizeViewer>("资源大小查看器");
    }

    private void OnGUI()
    {
        DrawTopBar();

        if (allAssets.Count == 0) return;

        DrawPagination();
        GUILayout.Space(5);
        DrawList();
    }

    private void DrawTopBar()
    {
        GUILayout.BeginVertical("box");

        // 第一行：扫描与总数
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("扫描", GUILayout.Width(100))) ScanAssets();
        GUILayout.Label($"总数: {allAssets.Count} (过滤后: {filteredAssets.Count})");
        GUILayout.EndHorizontal();

        // 第二行：路径
        GUILayout.BeginHorizontal();
        GUILayout.Label("扫描路径:", GUILayout.Width(70));
        inputPath = GUILayout.TextField(inputPath);
        if (GUILayout.Button("重置", GUILayout.Width(60))) inputPath = "Assets";
        GUILayout.EndHorizontal();

        // 第三行：搜索
        GUILayout.BeginHorizontal();
        GUILayout.Label("关键字:", GUILayout.Width(70));
        string newKeyword = GUILayout.TextField(searchKeyword);
        if (newKeyword != searchKeyword)
        {
            searchKeyword = newKeyword;
            UpdateFilter();
        }
        GUILayout.EndHorizontal();

        // 第四行：分页配置
        GUILayout.BeginHorizontal();
        GUILayout.Label("每页数量:", GUILayout.Width(70));
        inputPageSize = EditorGUILayout.IntField(inputPageSize, GUILayout.Width(60));
        if (GUILayout.Button("应用", GUILayout.Width(60))) ApplyPageSize();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private void DrawPagination()
    {
        int totalPages = Mathf.CeilToInt((float)filteredAssets.Count / pageSize);
        if (totalPages == 0) totalPages = 1;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<<", GUILayout.Width(40))) currentPage = 0;
        if (GUILayout.Button("<", GUILayout.Width(40))) currentPage = Mathf.Max(0, currentPage - 1);
        
        GUILayout.Label($"第 {currentPage + 1} / {totalPages} 页", GUILayout.Width(100));

        if (GUILayout.Button(">", GUILayout.Width(40))) currentPage = Mathf.Min(totalPages - 1, currentPage + 1);
        if (GUILayout.Button(">>", GUILayout.Width(40))) currentPage = totalPages - 1;
        GUILayout.EndHorizontal();
    }

    private void DrawList()
    {
        scroll = GUILayout.BeginScrollView(scroll);
        int start = currentPage * pageSize;
        int end = Mathf.Min(start + pageSize, filteredAssets.Count);

        for (int i = start; i < end; i++)
        {
            var asset = filteredAssets[i];
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{i + 1}. {asset.path}", GUILayout.Width(position.width - 220));
            GUILayout.Label(FormatSize(asset.size), GUILayout.Width(80));

            if (GUILayout.Button("定位", GUILayout.Width(50)))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(asset.path);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
    }

    private void ScanAssets()
    {
        allAssets.Clear();
        string fullPath = Path.Combine(Application.dataPath, inputPath.Replace("Assets", "").TrimStart('/', '\\'));

        if (!Directory.Exists(fullPath))
        {
            Debug.LogError($"路径不存在: {fullPath}");
            return;
        }

        var files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".meta"));

        foreach (var file in files)
        {
            FileInfo fi = new FileInfo(file);
            allAssets.Add(new AssetInfo
            {
                path = "Assets" + file.Replace(Application.dataPath, "").Replace("\\", "/"),
                size = fi.Length
            });
        }

        allAssets = allAssets.OrderByDescending(a => a.size).ToList();
        UpdateFilter();
        Debug.Log($"扫描完成，共 {allAssets.Count} 个资源");
    }

    private void UpdateFilter()
    {
        if (string.IsNullOrEmpty(searchKeyword))
        {
            filteredAssets = new List<AssetInfo>(allAssets);
        }
        else
        {
            filteredAssets = allAssets.Where(a => a.path.Contains(searchKeyword, System.StringComparison.OrdinalIgnoreCase)).ToList();
        }
        currentPage = 0;
    }

    private void ApplyPageSize()
    {
        pageSize = Mathf.Clamp(inputPageSize, 1, 500);
        currentPage = 0;
    }

    private string FormatSize(long size)
    {
        return size > 1024 * 1024 ? 
            (size / 1024f / 1024f).ToString("F2") + " MB" : 
            (size / 1024f).ToString("F2") + " KB";
    }
}
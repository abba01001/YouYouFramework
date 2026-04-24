using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class YooAssetBatchViewer : EditorWindow
{
    private string targetFolder = "Assets";
    private List<BundleDetail> bundleList = new List<BundleDetail>();
    private Vector2 scrollPos;
    private string searchFilter = "";
    private long totalSizeBytes = 0;

    private class AssetInfo
    {
        public string Path;
        public long Size;
        public string FormattedSize;
    }

    private class BundleDetail
    {
        public string FileName;
        public string FullPath;
        public List<AssetInfo> Assets = new List<AssetInfo>(); // 修改为结构化信息
        public long FileSize;
        public string FormattedSize;
        public bool IsExpanded;
    }

    public static void ShowWindow() => GetWindow<YooAssetBatchViewer>("Bundle 批量查看").minSize = new Vector2(700, 650);

    private void OnGUI()
    {
        DrawHeader();

        // 搜索过滤
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label(" 搜索资源/Bundle:", GUILayout.Width(100));
        searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);
        EditorGUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        if (bundleList.Count == 0) EditorGUILayout.HelpBox("请先选择文件夹并点击扫描。", MessageType.Info);
        else
        {
            foreach (var bundle in bundleList)
            {
                if (!string.IsNullOrEmpty(searchFilter) && 
                    !bundle.FileName.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase) && 
                    !bundle.Assets.Any(a => a.Path.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase)))
                    continue;

                DrawBundleItem(bundle);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Bundle 资源明细分析", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUIStyle totalSizeStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = new Color(0.2f, 1f, 0.2f) } };
        GUILayout.Label($"总占用: {FormatBytes(totalSizeBytes)}", totalSizeStyle);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        targetFolder = EditorGUILayout.TextField("扫描路径:", targetFolder);
        if (GUILayout.Button("选择文件夹", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFolderPanel("选择目录", Application.dataPath, "");
            if (!string.IsNullOrEmpty(path)) { targetFolder = path; ScanFolder(path); }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("展开全部", GUILayout.Width(70))) bundleList.ForEach(b => b.IsExpanded = true);
        if (GUILayout.Button("收起全部", GUILayout.Width(70))) bundleList.ForEach(b => b.IsExpanded = false);
        if (GUILayout.Button("刷新", GUILayout.Width(70))) ScanFolder(targetFolder);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawBundleItem(BundleDetail bundle)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        
        bundle.IsExpanded = EditorGUILayout.Foldout(bundle.IsExpanded, bundle.FileName, true);
        GUILayout.FlexibleSpace();
        
        GUILayout.Label(bundle.FormattedSize, new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.cyan } });
        if (GUILayout.Button("定位", GUILayout.Width(45))) PingAndSelectPath(bundle.FullPath);
        EditorGUILayout.EndHorizontal();

        if (bundle.IsExpanded)
        {
            EditorGUI.indentLevel++;
            // 内部资源按大小降序排列
            var sortedAssets = bundle.Assets.OrderByDescending(a => a.Size);
            foreach (var asset in sortedAssets)
            {
                if (!string.IsNullOrEmpty(searchFilter) && !asset.Path.Contains(searchFilter, System.StringComparison.OrdinalIgnoreCase)) continue;

                EditorGUILayout.BeginHorizontal();
                // 资源名称占位宽一点
                EditorGUILayout.LabelField(asset.Path, EditorStyles.miniLabel);
                
                // 资源原始大小显示（灰色）
                GUIStyle assetSizeStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.gray }, alignment = TextAnchor.MiddleRight };
                GUILayout.Label(asset.FormattedSize, assetSizeStyle, GUILayout.Width(80));
                
                if (GUILayout.Button("定位", EditorStyles.miniButton, GUILayout.Width(40))) PingAndSelectPath(asset.Path);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }

    private void ScanFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;
        bundleList.Clear();
        totalSizeBytes = 0;

        string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".bundle") || f.EndsWith(".assetbundle")).ToArray();

        foreach (string file in files)
        {
            FileInfo bInfo = new FileInfo(file);
            totalSizeBytes += bInfo.Length;

            AssetBundle ab = AssetBundle.LoadFromFile(file);
            if (ab != null)
            {
                var detail = new BundleDetail
                {
                    FileName = Path.GetFileName(file),
                    FullPath = file,
                    FileSize = bInfo.Length,
                    FormattedSize = FormatBytes(bInfo.Length),
                    IsExpanded = false
                };

                foreach (var assetPath in ab.GetAllAssetNames())
                {
                    long aSize = 0;
                    // 获取工程内文件的实际大小
                    string absolutePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
                    if (File.Exists(absolutePath))
                    {
                        aSize = new FileInfo(absolutePath).Length;
                    }

                    detail.Assets.Add(new AssetInfo
                    {
                        Path = assetPath,
                        Size = aSize,
                        FormattedSize = FormatBytes(aSize)
                    });
                }
                bundleList.Add(detail);
                ab.Unload(true);
            }
        }
        bundleList = bundleList.OrderByDescending(b => b.FileSize).ToList();
    }

    private void PingAndSelectPath(string path)
    {
        string relPath = path.StartsWith(Application.dataPath) ? "Assets" + path.Substring(Application.dataPath.Length) : path;
        Object obj = AssetDatabase.LoadAssetAtPath<Object>(relPath);
        if (obj != null) { Selection.activeObject = obj; EditorGUIUtility.PingObject(obj); }
        else EditorUtility.RevealInFinder(path);
    }

    private string FormatBytes(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB" };
        double val = bytes;
        int i = 0;
        while (val >= 1024 && i < units.Length - 1) { val /= 1024; i++; }
        return $"{val:F2} {units[i]}";
    }
}
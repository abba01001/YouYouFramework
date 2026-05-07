using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class TMPPrefabFinder : EditorWindow
{
    private enum WindowMode { BatchModify, ReferenceAnalysis }
    private WindowMode _currentMode = WindowMode.BatchModify;

    private class TMPNode
    {
        public bool Selected;
        public string ComponentPath; 
        public string OriginalFontName;
        public string HierarchyName;
        public int ComponentIndex; 
        public TextMeshProUGUI SceneTarget;
    }

    private class PrefabEntry
    {
        public bool Selected;
        public bool IsExpanded;
        public string Path;
        public GameObject Prefab;
        public List<TMPNode> Nodes = new List<TMPNode>();
    }

    private List<PrefabEntry> _entries = new List<PrefabEntry>();
    private List<TMP_FontAsset> _allFonts = new List<TMP_FontAsset>();
    private string[] _fontDisplayNames;
    private int _selectedFontIndex = -1;
    private TMP_FontAsset _targetFont;

    private Material[] _availableMaterials;
    private string[] _materialNames;
    private int _selectedMaterialIndex = 0;

    private Vector2 _scrollPos;
    private string _searchFilter = "";

    private int _refFontPopupIndex = -1;
    private Dictionary<TMP_FontAsset, int> _fontUsageCount = new Dictionary<TMP_FontAsset, int>();

    [MenuItem("Tools/UI/TMP 综合管理工具")]
    public static void ShowWindow()
    {
        var window = GetWindow<TMPPrefabFinder>("TMP Manager Pro");
        window.RefreshFontList();
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        _currentMode = (WindowMode)GUILayout.Toolbar((int)_currentMode, new string[] { "批量修改模式", "引用查找模式" });
        if (EditorGUI.EndChangeCheck())
        {
            // 核心改进：切换 Tab 时清理数据
            _entries.Clear();
            _searchFilter = "";
            _scrollPos = Vector2.zero;
            if (_currentMode == WindowMode.ReferenceAnalysis) 
            {
                _refFontPopupIndex = -1;
                RefreshUsageStatistics(); // 切换时顺便刷新下统计
            }
        }
        
        EditorGUILayout.Space(5);

        if (_currentMode == WindowMode.BatchModify)
            DrawBatchModifyUI();
        else
            DrawReferenceAnalysisUI();

        RenderListHeader();
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        RenderListItems();
        EditorGUILayout.EndScrollView();
    }

    private void DrawBatchModifyUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawFontAndMaterialSection();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("扫描全部 (预制体+场景)", GUILayout.Height(25))) ScanPrefabs(null);
        
        GUI.enabled = _entries.Count > 0 && _targetFont != null;
        if (GUILayout.Button("执行批量替换", GUILayout.Height(25))) ApplyToSelectedNodes();
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawReferenceAnalysisUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("1. 选择要分析的字体:", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (_fontDisplayNames != null && _fontDisplayNames.Length > 0)
        {
            EditorGUI.BeginChangeCheck();
            string[] refDisplayNames = _allFonts.Select(f => {
                int count = _fontUsageCount.ContainsKey(f) ? _fontUsageCount[f] : 0;
                return $"{f.name} (引用: {count})";
            }).ToArray();

            _refFontPopupIndex = EditorGUILayout.Popup("选择字体", _refFontPopupIndex, refDisplayNames);
            if (EditorGUI.EndChangeCheck() && _refFontPopupIndex >= 0)
            {
                ScanPrefabs(_allFonts[_refFontPopupIndex]);
            }
        }

        if (GUILayout.Button("刷新引用计数", GUILayout.Width(100))) RefreshUsageStatistics();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawFontAndMaterialSection()
    {
        EditorGUILayout.BeginHorizontal();
        if (_fontDisplayNames != null && _fontDisplayNames.Length > 0)
        {
            EditorGUI.BeginChangeCheck();
            _selectedFontIndex = EditorGUILayout.Popup("目标字体", _selectedFontIndex, _fontDisplayNames);
            if (EditorGUI.EndChangeCheck() && _selectedFontIndex >= 0)
            {
                _targetFont = _allFonts[_selectedFontIndex];
                RefreshMaterialList();
            }
        }
        if (GUILayout.Button("刷新", GUILayout.Width(40))) RefreshFontList();
        EditorGUILayout.EndHorizontal();

        _targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField("字体对象", _targetFont, typeof(TMP_FontAsset), false);

        if (_targetFont != null && _materialNames != null && _materialNames.Length > 0)
            _selectedMaterialIndex = EditorGUILayout.Popup("材质 Preset", _selectedMaterialIndex, _materialNames);
    }

    private void RefreshUsageStatistics()
    {
        _fontUsageCount.Clear();
        foreach (var f in _allFonts) _fontUsageCount[f] = 0;

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go)
            {
                var tmps = go.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in tmps)
                {
                    if (t.font != null && _fontUsageCount.ContainsKey(t.font))
                        _fontUsageCount[t.font]++;
                }
            }
        }
        var sceneTMPs = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>()
            .Where(t => !EditorUtility.IsPersistent(t.transform.root.gameObject) && t.hideFlags == HideFlags.None);
        foreach (var t in sceneTMPs)
        {
            if (t.font != null && _fontUsageCount.ContainsKey(t.font))
                _fontUsageCount[t.font]++;
        }
    }

    private void ScanPrefabs(TMP_FontAsset filter)
    {
        _entries.Clear();
        var sceneTMPs = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>()
            .Where(t => !EditorUtility.IsPersistent(t.transform.root.gameObject) && t.hideFlags == HideFlags.None)
            .ToList();

        if (sceneTMPs.Count > 0)
        {
            var sceneEntry = new PrefabEntry { Selected = true, Path = " [SCENE] ", IsExpanded = true };
            for (int i = 0; i < sceneTMPs.Count; i++)
            {
                var tmp = sceneTMPs[i];
                if (filter != null && tmp.font != filter) continue;
                sceneEntry.Nodes.Add(new TMPNode {
                    Selected = true,
                    HierarchyName = tmp.gameObject.name,
                    ComponentPath = GetFullHierarchyPath(tmp.transform),
                    OriginalFontName = tmp.font != null ? tmp.font.name : "None",
                    ComponentIndex = i,
                    SceneTarget = tmp
                });
            }
            if (sceneEntry.Nodes.Count > 0) _entries.Add(sceneEntry);
        }
        
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;

            var tmps = go.GetComponentsInChildren<TextMeshProUGUI>(true);
            var entry = new PrefabEntry { Selected = true, Path = path, Prefab = go, IsExpanded = false };
            for (int i = 0; i < tmps.Length; i++)
            {
                var tmp = tmps[i];
                if (filter != null && tmp.font != filter) continue;
                entry.Nodes.Add(new TMPNode { Selected = true, HierarchyName = tmp.gameObject.name, ComponentPath = GetRelativePath(tmp.transform, go.transform), OriginalFontName = tmp.font != null ? tmp.font.name : "None", ComponentIndex = i });
            }
            if (entry.Nodes.Count > 0) _entries.Add(entry);
        }
    }

    private void ApplyToSelectedNodes()
    {
        Material targetMat = (_availableMaterials != null && _availableMaterials.Length > _selectedMaterialIndex) 
                             ? _availableMaterials[_selectedMaterialIndex] : null;

        int totalModified = 0;
        foreach (var entry in _entries)
        {
            if (!IsPathMatchFilter(entry.Path) || !entry.Nodes.Any(n => n.Selected)) continue;
            if (entry.Prefab == null)
            {
                foreach (var node in entry.Nodes)
                {
                    if (node.Selected && node.SceneTarget != null)
                    {
                        ModifyTMP(node.SceneTarget, targetMat);
                        totalModified++;
                    }
                }
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                continue;
            }
            GameObject root = PrefabUtility.LoadPrefabContents(entry.Path);
            var allTMPs = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            bool changed = false;
            foreach (var node in entry.Nodes)
            {
                if (node.Selected && node.ComponentIndex < allTMPs.Length)
                {
                    ModifyTMP(allTMPs[node.ComponentIndex], targetMat);
                    changed = true;
                    totalModified++;
                }
            }
            if (changed) PrefabUtility.SaveAsPrefabAsset(root, entry.Path);
            PrefabUtility.UnloadPrefabContents(root);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"操作完成：共处理 {totalModified} 个组件。");
        ScanPrefabs(_currentMode == WindowMode.ReferenceAnalysis && _refFontPopupIndex >= 0 ? _allFonts[_refFontPopupIndex] : null);
    }

    private void ModifyTMP(TextMeshProUGUI tmp, Material targetMat)
    {
        Undo.RecordObject(tmp, "Batch TMP Change");
        if (PrefabUtility.IsPartOfPrefabInstance(tmp)) PrefabUtility.RecordPrefabInstancePropertyModifications(tmp);
        tmp.font = _targetFont;
        if (targetMat != null) tmp.fontSharedMaterial = targetMat;
        else tmp.fontSharedMaterial = _targetFont.material;
        tmp.SetAllDirty();
        EditorUtility.SetDirty(tmp);
    }

    private string GetRelativePath(Transform t, Transform root)
    {
        if (t == root) return "";
        string path = t.name;
        Transform parent = t.parent;
        while (parent != null && parent != root) { path = parent.name + "/" + path; parent = parent.parent; }
        return path;
    }

    private string GetFullHierarchyPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }

    private void RefreshFontList()
    {
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        var fontData = new List<(TMP_FontAsset font, string path)>();
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font != null) fontData.Add((font, path));
        }
        fontData = fontData.OrderBy(x => x.font.name).ToList();
        _allFonts = fontData.Select(x => x.font).ToList();
        _fontDisplayNames = fontData.Select(x => x.font.name).ToArray(); 
        _selectedFontIndex = (_targetFont == null) ? -1 : _allFonts.IndexOf(_targetFont);
        RefreshUsageStatistics();
    }

    private void RefreshMaterialList()
    {
        if (_targetFont == null || _targetFont.atlasTexture == null) { _availableMaterials = null; _materialNames = null; return; }
        Texture2D fontTexture = _targetFont.atlasTexture;
        string[] matGuids = AssetDatabase.FindAssets("t:Material");
        List<Material> mats = new List<Material>();
        foreach (var guid in matGuids)
        {
            Material m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (m != null && m.HasProperty(ShaderUtilities.ID_MainTex) && m.GetTexture(ShaderUtilities.ID_MainTex) == fontTexture)
                mats.Add(m);
        }
        _availableMaterials = mats.OrderBy(m => m.name).ToArray();
        _materialNames = _availableMaterials.Select(m => m.name).ToArray();
        _selectedMaterialIndex = 0;
    }

    private void RenderListHeader()
    {
        if (_entries.Count <= 0) return;
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField);
        if (GUILayout.Button("全选", EditorStyles.toolbarButton, GUILayout.Width(50))) SetAllSelection(true);
        if (GUILayout.Button("全不选", EditorStyles.toolbarButton, GUILayout.Width(60))) SetAllSelection(false);
        EditorGUILayout.EndHorizontal();
    }

    private void RenderListItems()
    {
        foreach (var entry in _entries)
        {
            if (!IsPathMatchFilter(entry.Path)) continue;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            entry.Selected = EditorGUILayout.Toggle(entry.Selected, GUILayout.Width(20));
            if (EditorGUI.EndChangeCheck()) foreach (var node in entry.Nodes) node.Selected = entry.Selected;
            entry.IsExpanded = EditorGUILayout.Foldout(entry.IsExpanded, entry.Path, true);
            GUILayout.FlexibleSpace();
            if (entry.Prefab != null && GUILayout.Button("定位", EditorStyles.miniButton, GUILayout.Width(40))) EditorGUIUtility.PingObject(entry.Prefab);
            EditorGUILayout.EndHorizontal();
            if (entry.IsExpanded)
            {
                EditorGUI.indentLevel++;
                foreach (var node in entry.Nodes)
                {
                    EditorGUILayout.BeginHorizontal();
                    node.Selected = EditorGUILayout.Toggle(node.Selected, GUILayout.Width(20));
                    EditorGUILayout.LabelField(new GUIContent(node.HierarchyName, node.ComponentPath), EditorStyles.miniLabel);
                    if (_currentMode == WindowMode.BatchModify) { GUI.contentColor = Color.gray; EditorGUILayout.LabelField(node.OriginalFontName, EditorStyles.miniLabel, GUILayout.Width(120)); GUI.contentColor = Color.white; }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }
    }

    private void SetAllSelection(bool select)
    {
        foreach (var entry in _entries) { if (IsPathMatchFilter(entry.Path)) { entry.Selected = select; foreach (var node in entry.Nodes) node.Selected = select; } }
    }

    private bool IsPathMatchFilter(string path) => string.IsNullOrEmpty(_searchFilter) || path.ToLower().Contains(_searchFilter.ToLower());
}
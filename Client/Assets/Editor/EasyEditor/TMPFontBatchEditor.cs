using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class TMPPrefabFinder : EditorWindow
{
    private class TMPNode
    {
        public bool Selected;
        public string ComponentPath; // 组件在预制体内的相对路径
        public string OriginalFontName;
        public string HierarchyName;
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
    private string[] _fontDisplayNames; // 存储带路径的显示名称
    private int _selectedFontIndex = -1;
    private TMP_FontAsset _targetFont;

    private Material[] _availableMaterials;
    private string[] _materialNames;
    private int _selectedMaterialIndex = 0;

    private Vector2 _scrollPos;
    private string _searchFilter = "";

    [MenuItem("Tools/UI/TMP 预制体管理器 (高级版)")]
    public static void ShowWindow()
    {
        var window = GetWindow<TMPPrefabFinder>("TMP Manager Pro");
        window.RefreshFontList();
        window.Show();
    }

    private void OnGUI()
    {
        // 顶部配置区
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawFontAndMaterialSection();
        
        EditorGUILayout.Space(2);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("1. 扫描所有预制体", GUILayout.Height(25))) ScanPrefabs();
        
        GUI.enabled = _entries.Count > 0 && _targetFont != null;
        if (GUILayout.Button("2. 执行批量修改", GUILayout.Height(25))) ApplyToSelectedNodes();
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        // 列表操作区
        RenderListHeader();

        // 列表滚动区
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        RenderListItems();
        EditorGUILayout.EndScrollView();
    }

    private void DrawFontAndMaterialSection()
    {
        // 目标字体下拉 (显示完整路径)
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        if (_fontDisplayNames != null && _fontDisplayNames.Length > 0)
        {
            _selectedFontIndex = EditorGUILayout.Popup("目标字体 (路径)", _selectedFontIndex, _fontDisplayNames);
            if (EditorGUI.EndChangeCheck() && _selectedFontIndex >= 0)
            {
                _targetFont = _allFonts[_selectedFontIndex];
                RefreshMaterialList();
            }
        }
        else
        {
            EditorGUILayout.LabelField("目标字体", "工程内未找到 TMP 字体", EditorStyles.helpBox);
        }
        
        if (GUILayout.Button("刷新字体库", GUILayout.Width(80))) RefreshFontList();
        EditorGUILayout.EndHorizontal();

        // 对象引用框
        EditorGUI.BeginChangeCheck();
        _targetFont = (TMP_FontAsset)EditorGUILayout.ObjectField("当前选定对象", _targetFont, typeof(TMP_FontAsset), false);
        if (EditorGUI.EndChangeCheck())
        {
            SyncFontIndex();
            RefreshMaterialList();
        }

        // 材质 Preset 下拉
        if (_targetFont != null && _materialNames != null && _materialNames.Length > 0)
        {
            _selectedMaterialIndex = EditorGUILayout.Popup("材质 Preset", _selectedMaterialIndex, _materialNames);
        }
    }

    private void RenderListHeader()
    {
        if (_entries.Count <= 0) return;

        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField);
        
        if (GUILayout.Button("全选", EditorStyles.toolbarButton, GUILayout.Width(50))) SetAllSelection(true);
        if (GUILayout.Button("全不选", EditorStyles.toolbarButton, GUILayout.Width(60))) SetAllSelection(false);
        EditorGUILayout.EndHorizontal();
    }

    private void RenderListItems()
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            if (!IsPathMatchFilter(entry.Path)) continue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // --- 预制体行绘制 ---
            Rect headerRect = EditorGUILayout.BeginHorizontal();
            
            // 勾选框
            EditorGUI.BeginChangeCheck();
            entry.Selected = EditorGUILayout.Toggle(entry.Selected, GUILayout.Width(20));
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var node in entry.Nodes) node.Selected = entry.Selected;
            }

            // 路径与折叠 (点击文字整行触发折叠)
            entry.IsExpanded = EditorGUILayout.Foldout(entry.IsExpanded, entry.Path, true);
            
            // 响应整行点击（排除 Checkbox 区域）
            Rect clickRect = headerRect;
            clickRect.xMin += 25; clickRect.xMax -= 45;
            if (Event.current.type == EventType.MouseDown && clickRect.Contains(Event.current.mousePosition))
            {
                entry.IsExpanded = !entry.IsExpanded;
                Event.current.Use();
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("定位", EditorStyles.miniButton, GUILayout.Width(40))) EditorGUIUtility.PingObject(entry.Prefab);
            
            EditorGUILayout.EndHorizontal();

            // --- 子组件列表绘制 ---
            if (entry.IsExpanded)
            {
                EditorGUI.indentLevel++;
                foreach (var node in entry.Nodes)
                {
                    EditorGUILayout.BeginHorizontal();
                    node.Selected = EditorGUILayout.Toggle(node.Selected, GUILayout.Width(20));
                    
                    // 使用 Label 且开启裁剪，防止路径过长挡住右侧
                    GUIContent nodeContent = new GUIContent($"[TMP] {node.HierarchyName}", node.ComponentPath);
                    EditorGUILayout.LabelField(nodeContent, EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
                    
                    // 右侧固定宽度显示当前字体，防止重叠
                    GUI.contentColor = Color.gray;
                    EditorGUILayout.LabelField(node.OriginalFontName, EditorStyles.miniLabel, GUILayout.Width(120));
                    GUI.contentColor = Color.white;
                    
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }
    }

    private void ScanPrefabs()
    {
        _entries.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;

            var tmps = go.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (tmps.Length > 0)
            {
                var entry = new PrefabEntry { Selected = true, Path = path, Prefab = go, IsExpanded = false };
                foreach (var tmp in tmps)
                {
                    entry.Nodes.Add(new TMPNode {
                        Selected = true,
                        HierarchyName = tmp.gameObject.name,
                        ComponentPath = GetGameObjectPath(tmp.transform, go.transform),
                        OriginalFontName = tmp.font != null ? tmp.font.name : "None"
                    });
                }
                _entries.Add(entry);
            }
        }
    }

    private void ApplyToSelectedNodes()
    {
        Material targetMat = (_availableMaterials != null && _availableMaterials.Length > _selectedMaterialIndex) 
                             ? _availableMaterials[_selectedMaterialIndex] : null;

        int prefabCount = 0;
        foreach (var entry in _entries)
        {
            if (!IsPathMatchFilter(entry.Path) || !entry.Nodes.Any(n => n.Selected)) continue;

            GameObject root = PrefabUtility.LoadPrefabContents(entry.Path);
            bool changed = false;

            foreach (var node in entry.Nodes)
            {
                if (!node.Selected) continue;

                Transform targetTrans = root.transform.Find(node.ComponentPath);
                if (targetTrans == null && node.ComponentPath == "") targetTrans = root.transform;

                if (targetTrans != null)
                {
                    var tmp = targetTrans.GetComponent<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        Undo.RecordObject(tmp, "Batch TMP Change");
                        tmp.font = _targetFont;
                        if (targetMat != null) tmp.fontSharedMaterial = targetMat;
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(root, entry.Path);
                prefabCount++;
            }
            PrefabUtility.UnloadPrefabContents(root);
        }
        AssetDatabase.Refresh();
        ScanPrefabs(); // 刷新列表以显示更新后的字体名称
        Debug.Log($"操作完成：成功修改了 {prefabCount} 个预制体。");
    }

    private string GetGameObjectPath(Transform transform, Transform root)
    {
        if (transform == root) return "";
        string path = transform.name;
        while (transform.parent != null && transform.parent != root)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
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

        // 按路径排序
        fontData = fontData.OrderBy(x => x.path).ToList();

        _allFonts = fontData.Select(x => x.font).ToList();
        _fontDisplayNames = fontData.Select(x => $"{x.path} [{x.font.name}]").ToArray();

        SyncFontIndex();
    }

    private void SyncFontIndex()
    {
        _selectedFontIndex = (_targetFont == null) ? -1 : _allFonts.IndexOf(_targetFont);
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
            {
                mats.Add(m);
            }
        }
        _availableMaterials = mats.OrderBy(m => m.name).ToArray();
        _materialNames = _availableMaterials.Select(m => m.name).ToArray();
        _selectedMaterialIndex = 0;
    }

    private void SetAllSelection(bool select)
    {
        foreach (var entry in _entries)
        {
            if (IsPathMatchFilter(entry.Path))
            {
                entry.Selected = select;
                foreach (var node in entry.Nodes) node.Selected = select;
            }
        }
    }

    private bool IsPathMatchFilter(string path) => string.IsNullOrEmpty(_searchFilter) || path.ToLower().Contains(_searchFilter.ToLower());
}
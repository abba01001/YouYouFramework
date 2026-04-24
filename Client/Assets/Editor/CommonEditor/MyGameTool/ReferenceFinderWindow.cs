using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Main.Editor
{
    public class ReferenceFinderWindow : EditorWindow
    {
        private const string IsDependPrefKey = "RefFinder_IsDepend";
        private const string NeedUpdateStatePrefKey = "RefFinder_UpdateState";
        private const string IsLockedPrefKey = "RefFinder_IsLocked";

        private static ReferenceFinderData _data = new ReferenceFinderData();
        private static bool _initializedData = false;

        private bool _isDepend = false;
        private bool _needUpdateState = true;
        private bool _isLocked = false;
        private string _searchString = "";

        private List<string> _selectedAssetGuid = new List<string>();
        private AssetTreeView _mAssetTreeView;

        [SerializeField] private TreeViewState _mTreeViewState;

        [MenuItem("Assets/工具/寻找该资源的引用 %#&f", false, 25)]
        private static void FindRef()
        {
            InitDataIfNeeded();
            var window = GetWindow<ReferenceFinderWindow>();
            window.titleContent = new GUIContent("资源引用查找", EditorGUIUtility.IconContent("Search Icon").image);
            window.UpdateSelectedAssets(true);
            window.Show();
        }

        private static void InitDataIfNeeded()
        {
            if (!_initializedData)
            {
                if (!_data.ReadFromCache()) _data.CollectDependenciesInfo();
                _initializedData = true;
            }
        }

        private void OnEnable()
        {
            _isDepend = PlayerPrefs.GetInt(IsDependPrefKey, 0) == 1;
            _needUpdateState = PlayerPrefs.GetInt(NeedUpdateStatePrefKey, 1) == 1;
            _isLocked = PlayerPrefs.GetInt(IsLockedPrefKey, 0) == 1;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnDisable() => Selection.selectionChanged -= OnSelectionChanged;

        private void OnSelectionChanged() => UpdateSelectedAssets(false);

        private void UpdateSelectedAssets(bool force)
        {
            if (_isLocked && !force) return;

            var newSelection = new List<string>();
            foreach (var obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) continue;

                if (Directory.Exists(path))
                {
                    string[] guids = AssetDatabase.FindAssets(null, new[] { path });
                    foreach (var guid in guids)
                        if (!newSelection.Contains(guid) && !Directory.Exists(AssetDatabase.GUIDToAssetPath(guid)))
                            newSelection.Add(guid);
                }
                else newSelection.Add(AssetDatabase.AssetPathToGUID(path));
            }

            if (newSelection.Count > 0)
            {
                _selectedAssetGuid = newSelection;
                RefreshTree();
            }
        }

        private void RefreshTree()
        {
            if (_selectedAssetGuid.Count == 0) return;

            var root = SelectedAssetGuidToRootItem(_selectedAssetGuid);
            if (_mAssetTreeView == null)
            {
                _mTreeViewState = _mTreeViewState ?? new TreeViewState();
                var header = new MultiColumnHeader(AssetTreeView.CreateDefaultHeaderState());
                _mAssetTreeView = new AssetTreeView(_mTreeViewState, header);
            }
            _mAssetTreeView.assetRoot = root;
            _mAssetTreeView.searchString = _searchString;
            _mAssetTreeView.Reload();
            _mAssetTreeView.ExpandAll();
            Repaint();
        }

        private void OnGUI()
        {
            InitDataIfNeeded();
            
            // 1. 顶部工具栏
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            DrawToolbar();
            EditorGUILayout.EndHorizontal();

            // 2. 选中资源信息条
            DrawInfoBar();

            // 3. 树状列表
            Rect lastRect = GUILayoutUtility.GetLastRect();
            float treeY = lastRect.yMax;
            Rect treeRect = new Rect(0, treeY, position.width, position.height - treeY);
            _mAssetTreeView?.OnGUI(treeRect);

            if (_selectedAssetGuid.Count == 0)
            {
                var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontSize = 13 };
                GUI.Label(new Rect(0, 0, position.width, position.height), "请在 Project 窗口选择资源以开始查找", style);
            }
        }

        private void DrawToolbar()
        {
            Color oldColor = GUI.color;
            GUI.color = new Color(0.7f, 0.9f, 1f); 
            if (GUILayout.Button("更新项目数据", EditorStyles.toolbarButton, GUILayout.Width(90))) 
            { 
                _data.CollectDependenciesInfo(); 
                RefreshTree(); 
            }
            GUI.color = oldColor;

            GUILayout.Space(10);

            if (_isLocked) GUI.color = Color.yellow;
            if (GUILayout.Button(_isLocked ? "🔒 视图已锁定" : "🔓 点击锁定视图", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                _isLocked = !_isLocked;
                PlayerPrefs.SetInt(IsLockedPrefKey, _isLocked ? 1 : 0);
            }
            GUI.color = oldColor;

            bool prevDepend = _isDepend;
            _isDepend = GUILayout.Toggle(_isDepend, _isDepend ? "查看模式：资源依赖" : "查看模式：被谁引用", EditorStyles.toolbarButton, GUILayout.Width(130));
            if (prevDepend != _isDepend)
            {
                PlayerPrefs.SetInt(IsDependPrefKey, _isDepend ? 1 : 0);
                RefreshTree();
            }

            GUILayout.Space(5);
            _searchString = EditorGUILayout.TextField(_searchString, EditorStyles.toolbarSearchField);
            if (GUI.changed)
            {
                if (_mAssetTreeView != null) { _mAssetTreeView.searchString = _searchString; _mAssetTreeView.Reload(); }
            }

            GUILayout.FlexibleSpace();
        }

        private void DrawInfoBar()
        {
            // 1. 获取一个固定的绘制区域
            Rect infoRect = EditorGUILayout.GetControlRect(false, 24);
            // 绘制 HelpBox 背景
            GUI.Box(infoRect, "", EditorStyles.helpBox);

            string path = _selectedAssetGuid.Count > 0 ? AssetDatabase.GUIDToAssetPath(_selectedAssetGuid[0]) : "无选择内容";
            if (_selectedAssetGuid.Count > 1) path += $" (+{_selectedAssetGuid.Count - 1} 个资源)";
            
            float xOffset = infoRect.x + 4;

            // 2. 绘制图标 (纯 GUI 绘制，强制 16x16，绝对不会变形)
            var icon = AssetDatabase.GetCachedIcon(path);
            if (icon != null)
            {
                Rect iconRect = new Rect(xOffset, infoRect.y + 4, 16, 16);
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit); // 确保比例正确
                xOffset += 20;
            }

            // 3. 计算右侧组件所需的宽度，剩下的全给路径
            float toggleWidth = 80;
            float pathWidth = infoRect.width - (xOffset - infoRect.x) - toggleWidth - 5;

            // 4. 绘制路径 (SelectableLabel，支持点击滚动和 MiddleTruncate)
            if (pathWidth > 10)
            {
                Rect labelRect = new Rect(xOffset, infoRect.y + 2, pathWidth, 20);
                
                GUIStyle pathStyle = new GUIStyle(EditorStyles.label);
                pathStyle.fontSize = 11;
                pathStyle.alignment = TextAnchor.MiddleLeft;
                // 注意：SelectableLabel 在纯 GUI 模式下对中间省略支持不稳，
                // 但 Clip 模式允许点击后左右滑动查看，配合 Tooltip，体验最好。
                pathStyle.clipping = TextClipping.Clip; 

                EditorGUI.SelectableLabel(labelRect, path, pathStyle);
            }

            // 5. 绘制右侧勾选框
            Rect toggleRect = new Rect(infoRect.xMax - toggleWidth, infoRect.y + 2, toggleWidth, 20);
            EditorGUI.BeginChangeCheck();
            // 使用 EditorGUI 而不是 EditorGUILayout
            _needUpdateState = EditorGUI.ToggleLeft(toggleRect, "检查状态", _needUpdateState);
            if (EditorGUI.EndChangeCheck())
            {
                PlayerPrefs.SetInt(NeedUpdateStatePrefKey, _needUpdateState ? 1 : 0);
            }
        }

        private HashSet<string> _updatedAssetSet = new HashSet<string>();
        private AssetViewItem SelectedAssetGuidToRootItem(List<string> guids)
        {
            _updatedAssetSet.Clear();
            int count = 0;
            var root = new AssetViewItem { id = 0, depth = -1, displayName = "Root" };
            var stack = new Stack<string>();
            foreach (var g in guids)
            {
                var child = CreateTree(g, ref count, 0, stack);
                if (child != null) root.AddChild(child);
            }
            return root;
        }

        private AssetViewItem CreateTree(string guid, ref int count, int depth, Stack<string> stack)
        {
            if (stack.Contains(guid) || !_data.assetDict.ContainsKey(guid)) return null;
            stack.Push(guid);
            if (_needUpdateState && !_updatedAssetSet.Contains(guid)) { _data.UpdateAssetState(guid); _updatedAssetSet.Add(guid); }
            var desc = _data.assetDict[guid];
            var item = new AssetViewItem { id = ++count, displayName = desc.name, data = desc, depth = depth };
            var children = _isDepend ? desc.dependencies : desc.references;
            foreach (var cGuid in children)
            {
                var child = CreateTree(cGuid, ref count, depth + 1, stack);
                if (child != null) item.AddChild(child);
            }
            stack.Pop();
            return item;
        }

        public class ReferenceFinderData
        {
            public const string CACHE_PATH = "Library/ReferenceFinderCache";
            private const string CACHE_VERSION = "V2";
            public Dictionary<string, AssetDescription> assetDict = new Dictionary<string, AssetDescription>();

            public void CollectDependenciesInfo()
            {
                try
                {
                    assetDict.Clear();
                    var allAssets = AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/")).ToArray();
                    for (int i = 0; i < allAssets.Length; i++)
                    {
                        if (i % 100 == 0 && EditorUtility.DisplayCancelableProgressBar("刷新中", $"正在扫描资源: {i}/{allAssets.Length}", (float)i / allAssets.Length)) break;
                        ImportAsset(allAssets[i]);
                    }
                    UpdateReferenceInfo();
                    WriteToCache();
                    EditorUtility.ClearProgressBar();
                }
                catch (Exception e) { Debug.LogError(e); EditorUtility.ClearProgressBar(); }
            }

            private void ImportAsset(string path)
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                string hash = AssetDatabase.GetAssetDependencyHash(path).ToString();
                var deps = AssetDatabase.GetDependencies(path, false).Select(AssetDatabase.AssetPathToGUID).ToList();
                assetDict[guid] = new AssetDescription { name = Path.GetFileNameWithoutExtension(path), path = path, assetDependencyHash = hash, dependencies = deps };
            }

            private void UpdateReferenceInfo()
            {
                foreach (var asset in assetDict)
                    foreach (var depGuid in asset.Value.dependencies)
                        if (assetDict.TryGetValue(depGuid, out var ad) && !ad.references.Contains(asset.Key))
                            ad.references.Add(asset.Key);
            }

            public bool ReadFromCache()
            {
                if (!File.Exists(CACHE_PATH)) return false;
                try
                {
                    using (FileStream fs = File.OpenRead(CACHE_PATH))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        if ((string)bf.Deserialize(fs) != CACHE_VERSION) return false;
                        var guids = (List<string>)bf.Deserialize(fs);
                        var hashes = (List<string>)bf.Deserialize(fs);
                        var depIndexes = (List<int[]>)bf.Deserialize(fs);
                        assetDict.Clear();
                        for (int i = 0; i < guids.Count; i++)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                            if (string.IsNullOrEmpty(path)) continue;
                            assetDict[guids[i]] = new AssetDescription { name = Path.GetFileNameWithoutExtension(path), path = path, assetDependencyHash = hashes[i] };
                        }
                        for (int i = 0; i < guids.Count; i++)
                            if (assetDict.TryGetValue(guids[i], out var ad))
                                ad.dependencies = depIndexes[i].Select(idx => guids[idx]).Where(g => assetDict.ContainsKey(g)).ToList();
                    }
                    UpdateReferenceInfo();
                    return true;
                }
                catch { return false; }
            }

            private void WriteToCache()
            {
                var guids = assetDict.Keys.ToList();
                var guidToIndex = guids.Select((g, i) => new { g, i }).ToDictionary(x => x.g, x => x.i);
                using (FileStream fs = File.OpenWrite(CACHE_PATH))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, CACHE_VERSION);
                    bf.Serialize(fs, guids);
                    bf.Serialize(fs, guids.Select(g => assetDict[g].assetDependencyHash).ToList());
                    bf.Serialize(fs, guids.Select(g => assetDict[g].dependencies.Where(guidToIndex.ContainsKey).Select(d => guidToIndex[d]).ToArray()).ToList());
                }
            }

            public void UpdateAssetState(string guid)
            {
                if (assetDict.TryGetValue(guid, out var ad))
                {
                    if (!File.Exists(ad.path)) ad.state = AssetState.缺失;
                    else ad.state = ad.assetDependencyHash != AssetDatabase.GetAssetDependencyHash(ad.path).ToString() ? AssetState.已修改 : AssetState.正常;
                }
                else
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    assetDict[guid] = new AssetDescription { name = Path.GetFileNameWithoutExtension(path), path = path, state = AssetState.无数据 };
                }
            }

            [Serializable]
            public class AssetDescription
            {
                public string name, path, assetDependencyHash;
                public List<string> dependencies = new List<string>(), references = new List<string>();
                public AssetState state = AssetState.正常;
            }

            public enum AssetState { 正常, 已修改, 缺失, 无数据 }
        }

        public class AssetViewItem : TreeViewItem { public ReferenceFinderData.AssetDescription data; }

        public class AssetTreeView : TreeView
        {
            public AssetViewItem assetRoot;
            public AssetTreeView(TreeViewState state, MultiColumnHeader header) : base(state, header)
            {
                rowHeight = 20; 
                showAlternatingRowBackgrounds = true;
                showBorder = true;
            }

            protected override TreeViewItem BuildRoot() => assetRoot;

            protected override void DoubleClickedItem(int id)
            {
                if (FindItem(id, rootItem) is AssetViewItem item)
                {
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(item.data.path);
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }
            }

            protected override void ContextClickedItem(int id)
            {
                if (FindItem(id, rootItem) is AssetViewItem item)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("复制路径"), false, () => EditorGUIUtility.systemCopyBuffer = item.data.path);
                    menu.AddItem(new GUIContent("在资源管理器中显示"), false, () => EditorUtility.RevealInFinder(item.data.path));
                    menu.ShowAsContext();
                }
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                var item = (AssetViewItem)args.item;
                for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                {
                    Rect rect = args.GetCellRect(i);
                    int columnIndex = args.GetColumn(i);

                    if (columnIndex == 0) 
                    {
                        float indent = GetContentIndent(item);
                        rect.x += indent;
                        rect.width -= indent;

                        Rect iconRect = new Rect(rect.x, rect.y + 2, 16, 16);
                        GUI.DrawTexture(iconRect, AssetDatabase.GetCachedIcon(item.data.path));

                        Rect labelRect = new Rect(rect.x + 20, rect.y, rect.width - 20, rect.height);
                        GUI.Label(labelRect, item.displayName);
                    }
                    else if (columnIndex == 1) 
                    {
                        GUI.Label(rect, item.data.path, EditorStyles.miniLabel);
                    }
                    else if (columnIndex == 2) 
                    {
                        DrawStateBadge(rect, item.data.state);
                    }
                }
            }

            private void DrawStateBadge(Rect rect, ReferenceFinderData.AssetState state)
            {
                if (state == ReferenceFinderData.AssetState.正常) return;
                Color color = state == ReferenceFinderData.AssetState.缺失 ? new Color(0.8f, 0.2f, 0.2f) : (state == ReferenceFinderData.AssetState.已修改 ? new Color(0.9f, 0.6f, 0.1f) : Color.gray);
                Rect badgeRect = new Rect(rect.x + 2, rect.y + 3, rect.width - 4, rect.height - 6);
                EditorGUI.DrawRect(badgeRect, color);
                var style = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white }, fontSize = 9 };
                GUI.Label(badgeRect, state.ToString(), style);
            }

            public static MultiColumnHeaderState CreateDefaultHeaderState()
            {
                return new MultiColumnHeaderState(new[] {
                    new MultiColumnHeaderState.Column { headerContent = new GUIContent("名称"), width = 240, autoResize = true },
                    new MultiColumnHeaderState.Column { headerContent = new GUIContent("路径"), width = 350, autoResize = true },
                    new MultiColumnHeaderState.Column { headerContent = new GUIContent("状态"), width = 60, autoResize = false }
                });
            }
        }
    }
}
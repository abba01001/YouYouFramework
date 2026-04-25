using OctoberStudio.Save;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor;
using System.Linq; // 引入用于过滤

public class SurvivorGMEditor : OdinEditorWindow
{
    [MenuItem("Tools/存档管理器 (Pro)")]
    public static void ShowWindow() => GetWindow<SurvivorGMEditor>("GM面板");

    [BoxGroup("核心控制", centerLabel: true)]
    [HorizontalGroup("核心控制/Buttons", Width = 0.5f)]
    [Button("🔄 刷新内存数据", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
    [EnableIf("@UnityEngine.Application.isPlaying")]
    public void LoadSave()
    {
        if (!Application.isPlaying) return;
        var manager = FindObjectOfType<SaveManager>();
        if (manager != null)
        {
            var property = typeof(SaveManager).GetProperty("SaveDatabase", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            currentDatabase = property?.GetValue(manager) as SaveDatabase;
            RefreshView();
        }
    }

    [HorizontalGroup("核心控制/Buttons")]
    [Button("💾 强制保存到磁盘", ButtonSizes.Large), GUIColor(0.4f, 1f, 0.4f)]
    [EnableIf("@currentDatabase != null && UnityEngine.Application.isPlaying")]
    public void SaveSave()
    {
        currentDatabase.Flush(); // 先把 RuntimeInstance 同步到 JsonData
        var manager =  FindObjectOfType<SaveManager>();
        manager?.Save(false);
        ShowNotification(new GUIContent("存档已成功落盘！"));
    }

    // --- 优化点 1：增加搜索过滤，数据多的时候极其好用 ---
    [Header("数据筛选")]
    [InlineButton("RefreshView", "清除过滤")]
    [EnableIf("@currentDatabase != null && UnityEngine.Application.isPlaying")]
    [OnValueChanged("RefreshView")]
    [LabelText("🔍 搜索模块")]
    public string searchFilter;

    [Space(10)]
    [TitleGroup("存档明细", "修改数值后记得点上方【保存】按钮")]
    [ShowIf("@currentDatabase != null")]
    [TableList(IsReadOnly = true, AlwaysExpanded = false, NumberOfItemsPerPage = 10)]
    public List<SaveCellView> cellViews = new List<SaveCellView>();

    [HideInInspector]
    private SaveDatabase currentDatabase;

    [InfoBox("提示：请先在 Editor 中启动游戏再进行操作", InfoMessageType.Info, "@!UnityEngine.Application.isPlaying")]
    [ShowInInspector, HideLabel, DisplayAsString]
    private string Status => Application.isPlaying ? "🟢 状态：游戏运行中 (内存读写已就绪)" : "🔴 状态：未运行 (请启动游戏)";

    private void RefreshView()
    {
        InitHashMap(); 
        cellViews.Clear();

        var field = typeof(SaveDatabase).GetField("saveCellsList", BindingFlags.NonPublic | BindingFlags.Instance);
        var list = field?.GetValue(currentDatabase) as List<SaveCell>;

        if (list != null)
        {
            foreach (var cell in list)
            {
                // --- 优化点 2：实时过滤逻辑 ---
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    _hashToKeyMap.TryGetValue(cell.Hash, out string name);
                    if (name == null || !name.ToLower().Contains(searchFilter.ToLower()))
                        continue;
                }
                cellViews.Add(new SaveCellView(cell, _hashToKeyMap));
            }
        }
    }

    [System.Serializable]
    public class SaveCellView
    {
        [TableColumnWidth(160), ReadOnly]
        [LabelText("模块")]
        [GUIColor(0.9f, 0.9f, 0.9f)] // 淡淡的灰色背景，区分标题和数值
        public string KeyName;

        [ShowInInspector]
        [LabelText("编辑数值")]
        [BoxGroup("Data", showLabel: false)]
        [HideReferenceObjectPicker] 
        public ISave RuntimeInstance;

        public SaveCellView(SaveCell cell, Dictionary<int, string> map)
        {
            this.RuntimeInstance = cell.Save; 
            if (map.TryGetValue(cell.Hash, out string name))
                this.KeyName = "📦 " + name;
            else
                this.KeyName = "❓ Unknown";
        }
    }
    
    #region 内部工具
    private static Dictionary<int, string> _hashToKeyMap;
    private static void InitHashMap()
    {
        if (_hashToKeyMap != null) return;
        _hashToKeyMap = new Dictionary<int, string>();
        var fields = typeof(SaveKey).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        foreach (var field in fields)
        {
            if (field.IsLiteral && field.FieldType == typeof(string))
            {
                string value = (string)field.GetRawConstantValue();
                _hashToKeyMap[value.GetHashCode()] = field.Name;
            }
        }
    }
    #endregion
}
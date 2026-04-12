using Main;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class DataTableManager
{
    internal Action OnLoadDataTableComplete;

    internal void Init()
    {
    }


    public LocalizationDBModel LocalizationDBModel { get; private set; }
    public Sys_UIFormDBModel Sys_UIFormDBModel { get; private set; }
    public Sys_PrefabDBModel Sys_PrefabDBModel { get; private set; }
    public Sys_BGMDBModel Sys_BGMDBModel { get; private set; }
    public Sys_BuildingsDBModel Sys_BuildingsDBModel { get; private set; }
    public Sys_AudioDBModel Sys_AudioDBModel { get; private set; }
    public Sys_SceneDBModel Sys_SceneDBModel { get; private set; }
    public Sys_GuideDBModel Sys_GuideDBModel { get; private set; }
    public Sys_AtlasDBModel Sys_AtlasDBModel { get; private set; }
    public Sys_DialogueDBModel Sys_DialogueDBModel { get; private set; }
    public Sys_ModelDBModel Sys_ModelDBModel { get; private set; }
    public Sys_EquipDBModel Sys_EquipDBModel { get; private set; }
    public Sys_ItemDBModel Sys_ItemDBModel { get; private set; }
    public Sys_UnlockFuncDBModel Sys_UnlockFuncDBModel { get; private set; }
    public Sys_LevelDBModel Sys_LevelDBModel { get; private set; }

    /// <summary>
    /// 加载表格
    /// </summary>
    private void LoadDataTable()
    {
        TaskGroup m_TaskGroup = GameEntry.Task.CreateTaskGroup();
        LocalizationDBModel = new LocalizationDBModel();
        LocalizationDBModel.LoadData(m_TaskGroup);

        Sys_UIFormDBModel = new Sys_UIFormDBModel();
        Sys_UIFormDBModel.LoadData(m_TaskGroup);

        Sys_PrefabDBModel = new Sys_PrefabDBModel();
        Sys_PrefabDBModel.LoadData(m_TaskGroup);

        Sys_AudioDBModel = new Sys_AudioDBModel();
        Sys_AudioDBModel.LoadData(m_TaskGroup);

        Sys_BGMDBModel = new Sys_BGMDBModel();
        Sys_BGMDBModel.LoadData(m_TaskGroup);

        Sys_BuildingsDBModel = new Sys_BuildingsDBModel();
        Sys_BuildingsDBModel.LoadData(m_TaskGroup);

        Sys_SceneDBModel = new Sys_SceneDBModel();
        Sys_SceneDBModel.LoadData(m_TaskGroup);

        Sys_GuideDBModel = new Sys_GuideDBModel();
        Sys_GuideDBModel.LoadData(m_TaskGroup);

        Sys_AtlasDBModel = new Sys_AtlasDBModel();
        Sys_AtlasDBModel.LoadData(m_TaskGroup);

        Sys_DialogueDBModel = new Sys_DialogueDBModel();
        Sys_DialogueDBModel.LoadData(m_TaskGroup);

        Sys_ModelDBModel = new Sys_ModelDBModel();
        Sys_ModelDBModel.LoadData(m_TaskGroup);

        Sys_EquipDBModel = new Sys_EquipDBModel();
        Sys_EquipDBModel.LoadData(m_TaskGroup);

        Sys_ItemDBModel = new Sys_ItemDBModel();
        Sys_ItemDBModel.LoadData(m_TaskGroup);

        Sys_UnlockFuncDBModel = new Sys_UnlockFuncDBModel();
        Sys_UnlockFuncDBModel.LoadData(m_TaskGroup);

        Sys_LevelDBModel = new Sys_LevelDBModel();
        Sys_LevelDBModel.LoadData(m_TaskGroup);

        m_TaskGroup.OnComplete += OnLoadDataTableComplete;
        m_TaskGroup.OnComplete += () =>
        {
            GameEntry.UI.OpenUIForm<FormMask>();
            Constants.IsLoadDataTable = true;
        };
        m_TaskGroup.Run(true);
    }

    /// <summary>
    /// 表格资源包
    /// </summary>
    private AssetBundle m_DataTableBundle;

    /// <summary>
    /// 加载表格
    /// </summary>
    internal void LoadDataAllTable(Action onComplete = null)
    {
        OnLoadDataTableComplete += onComplete;
        LoadDataTable();
    }

    /// <summary>
    /// 获取表格的字节数组
    /// </summary>
    public void GetDataTableBuffer(string dataTableName, Action<byte[]> onComplete)
    {
        TextAsset asset = GameEntry.Loader.LoadMainAsset<TextAsset>($"Assets/Game/Download/DataTable/{dataTableName}.bytes");
        if (onComplete != null) onComplete(asset.bytes);
    }
}
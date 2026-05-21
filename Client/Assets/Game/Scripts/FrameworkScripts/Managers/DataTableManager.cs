using Main;
using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using GameScripts;
using UniRx;
using UnityEngine;

namespace GameScripts
{
    public class DataTableManager
    {
        internal Action OnLoadDataTableComplete;

        // 1. 你只需要在这里定义，名字必须和类名一致
        public LocalizationDBModel LocalizationDBModel { get; private set; }
        public Sys_UIFormDBModel Sys_UIFormDBModel { get; private set; }
        public Sys_BGMDBModel Sys_BGMDBModel { get; private set; }
        public Sys_AudioDBModel Sys_AudioDBModel { get; private set; }
        public Sys_SceneDBModel Sys_SceneDBModel { get; private set; }
        public Sys_GuideDBModel Sys_GuideDBModel { get; private set; }
        public Sys_UnlockFuncDBModel Sys_UnlockFuncDBModel { get; private set; }
        public Sys_ModelDBModel Sys_ModelDBModel { get; private set; }
        public Sys_EnemyDataDBModel Sys_EnemyDataDBModel { get; private set; }
        public Sys_EquipmentDBModel Sys_EquipmentDBModel { get; private set; }
    
        /// <summary>
        /// 加载表格
        /// </summary>
        private void LoadDataTable()
        {
            TaskGroup m_TaskGroup = GameEntry.Task.CreateTaskGroup();
            
            // 获取当前 DataTableManager 定义的所有属性
            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                // 1. 判断是否继承自 DataTableDBModelBase<,>
                if (prop.PropertyType.IsClass && IsSubclassOfRawGeneric(typeof(DataTableDBModelBase<,>), prop.PropertyType))
                {
                    // 2. 实例化子类 (子类必须有无参构造函数，你的基类约束了 where T : class, new())
                    var model = Activator.CreateInstance(prop.PropertyType);
                    // 3. 赋值给属性
                    prop.SetValue(this, model);
                    // 4. 反射调用 internal 的 LoadData 方法
                    // 注意：因为 LoadData 是 internal，需要指定 BindingFlags
                    var loadMethod = prop.PropertyType.GetMethod("LoadData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (loadMethod != null)
                    {
                        loadMethod.Invoke(model, new object[] { m_TaskGroup });
                    }
                    else
                    {
                        UnityEngine.Debug.LogError($"无法在 {prop.PropertyType.Name} 中找到 LoadData 方法");
                    }
                }
            }
    
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
        public async UniTask GetDataTableBuffer(string dataTableName, Action<byte[]> onComplete)
        {
            TextAsset asset = await GameEntry.Loader.LoadMainAssetAsync<TextAsset>($"Assets/Game/Download/DataTable/{dataTableName}.bytes");
            if (asset != null)
            {
                onComplete?.Invoke(asset.bytes);
            }
            else
            {
                Debugger.LogError($"[DataTable] 无法加载表格资源: {dataTableName}");
            }
        }
        
        /// <summary>
        /// 辅助方法：判断一个类型是否继承自指定的泛型基类
        /// </summary>
        private bool IsSubclassOfRawGeneric(Type generic, Type toCheck) 
        {
            while (toCheck != null && toCheck != typeof(object)) 
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) 
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
    
    
    /// <summary>
    /// 数据表实体基类
    /// </summary>
    public class DataTableEntityBase
    {
        /// <summary>
        /// 实体编号
        /// </summary>
        public int Id;
    }
    
    /// <summary>
    /// 数据表管理基类
    /// </summary>
    /// <typeparam name="T">数据表管理子类的类型</typeparam>
    /// <typeparam name="P">数据表实体子类的类型</typeparam>
    public abstract class DataTableDBModelBase<T, P>
        where T : class, new()
        where P : DataTableEntityBase
    {
        /// <summary>
        /// Entity对象的集合
        /// </summary>
        protected List<P> m_List;
    
        public int Count
        {
            get { return m_List.Count; }
        }
    
        /// <summary>
        /// Key:Entity的ID
        /// Value:Entity对象
        /// </summary>
        protected Dictionary<int, P> m_Dic;
    
        public DataTableDBModelBase()
        {
            m_List = new List<P>();
            m_Dic = new Dictionary<int, P>();
        }
    
        #region 需要子类实现的属性,方法
    
        /// <summary>
        /// 数据表名称
        /// </summary>
        public abstract string DataTableName { get; }
    
        /// <summary>
        /// 加载数据列表
        /// </summary>
        protected abstract void LoadList(MMO_MemoryStream ms);
    
        protected virtual void OnLoadListComple()
        {
        }
    
        #endregion
    
        #region LoadData 加载数据表数据
    
        /// <summary>
        /// 加载数据表数据
        /// </summary>
        internal void LoadData(TaskGroup taskGroup)
        {
            taskGroup.AddTask((taskRoutine) =>
            {
                //1.拿到这个表格的buffer
                GameEntry.DataTable.GetDataTableBuffer(DataTableName, (byte[] buffer) =>
                {
                    using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
                    {
                        LoadList(ms);
                    }
    
                    OnLoadListComple();
                    taskRoutine.Leave();
                });
            });
        }
    
        public void TestLoad(MMO_MemoryStream ms)
        {
            LoadList(ms);
            OnLoadListComple();
        }
    
        #endregion
    
        #region GetList 获取子类对应的数据实体List
    
        /// <summary>
        /// 获取子类对应的数据实体List
        /// </summary>
        /// <returns></returns>
        public List<P> GetList()
        {
            return m_List;
        }
    
        #endregion
    
        #region GetDic 根据ID获取实体
    
        /// <summary>
        /// 根据ID获取实体
        /// </summary>
        public P GetDic(int id)
        {
            P p;
            if (m_Dic.TryGetValue(id, out p))
            {
                return p;
            }
            else
            {
                //Debug.Log("该ID对应的数据实体不存在");
                return null;
            }
        }
    
        public void PrintDic()
        {
            foreach (var pair in m_Dic)
            {
                Debugger.LogError(pair.Value.ToJson());
            }
        }
    
        #endregion
    
        /// <summary>
        /// 清空数据
        /// </summary>
        internal void Clear()
        {
            m_List.Clear();
            m_Dic.Clear();
        }
    }
    
}
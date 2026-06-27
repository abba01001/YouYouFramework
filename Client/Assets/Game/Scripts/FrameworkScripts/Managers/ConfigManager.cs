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
    public class ConfigManager
    {
        // 1. 你只需要在这里定义，名字必须和类名一致
        public Sys_LocalizationDBModel Sys_LocalizationDBModel { get; private set; }
        public Sys_UIFormDBModel Sys_UIFormDBModel { get; private set; }
        public Sys_BGMDBModel Sys_BGMDBModel { get; private set; }
        public Sys_AudioDBModel Sys_AudioDBModel { get; private set; }
        public Sys_SceneDBModel Sys_SceneDBModel { get; private set; }
        public Sys_GuideDBModel Sys_GuideDBModel { get; private set; }
        public Sys_UnlockFuncDBModel Sys_UnlockFuncDBModel { get; private set; }
        public Sys_ModelDBModel Sys_ModelDBModel { get; private set; }
        public Sys_EnemyDataDBModel Sys_EnemyDataDBModel { get; private set; }
        public Sys_EquipmentDBModel Sys_EquipmentDBModel { get; private set; }
    
        /// 自动加载所有ConfigBase表格
        public async UniTask LoadDataTableAsync(IProgress<float> realProgress)
        {
            Debugger.BeginProfile("LoadDataTableAsync","开始加载所有配置表==>>");
            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var models = new List<object>();

            // 1. 实例化所有模型
            foreach (var prop in properties)
            {
                if (prop.PropertyType.IsClass && IsSubclassOfRawGeneric(typeof(ConfigBase<,>), prop.PropertyType))
                {
                    var model = Activator.CreateInstance(prop.PropertyType);
                    prop.SetValue(this, model);
                    models.Add(model);
                }
            }

            int totalCount = models.Count;
            int completedCount = 0;
            // 使用对象锁保证计数安全
            object lockObj = new object();

            // 2. 将每个加载任务包装成一个带进度报告的任务
            var tasks = models.Select(async model =>
            {
                var method = model.GetType().GetMethod("LoadDataAsync", BindingFlags.Instance | BindingFlags.NonPublic);
                if (method != null)
                {
                    await (UniTask)method.Invoke(model, null);
            
                    // 任务完成后更新计数并报告
                    int current;
                    lock (lockObj)
                    {
                        completedCount++;
                        current = completedCount;
                    }
                    realProgress?.Report(((float)current / totalCount));
                }
            });

            // 3. 并发等待
            await UniTask.WhenAll(tasks);
            Debugger.EndProfile("LoadDataTableAsync","所有配置表加载完成==>>");
        }
 
        /// <summary>
        /// 获取表格的字节数组
        /// </summary>
        public async UniTask<byte[]> GetDataTableBufferAsync(string dataTableName)
        {
            TextAsset asset = await GameEntry.Loader.LoadMainAssetAsync<TextAsset>($"Assets/Game/Download/DataTable/{dataTableName}.bytes");
            if (asset == null)
            {
                Debugger.LogError($"[DataTable] 无法加载表格资源: {dataTableName}");
                return null;
            }
            return asset.bytes;
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
    public class ConfigEntityBase
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
    public abstract class ConfigBase<T, P>
        where T : class, new()
        where P : ConfigEntityBase
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
    
        public ConfigBase()
        {
            m_List = new List<P>();
            m_Dic = new Dictionary<int, P>();
        }
    
        #region 需要子类实现的属性,方法
    
        /// <summary>
        /// 数据表名称
        /// </summary>
        public abstract string ConfigName { get; }
    
        /// <summary>
        /// 加载数据列表
        /// </summary>
        protected abstract void LoadList(MMO_MemoryStream ms);
    
        protected virtual void OnLoadListComple()
        {
        }
    
        #endregion
    
        #region LoadData 加载数据表数据
        internal async UniTask LoadDataAsync()
        {
            // 使用 System.Diagnostics.Stopwatch 记录性能
            // Debugger.BeginProfile(DataTableName,"开始加载中...");
            byte[] buffer = await GameEntry.Config.GetDataTableBufferAsync(ConfigName);
            if (buffer == null)
            {
                // 关键失败点：一定要用 Error 级别，方便在日志平台直接过滤
                Debugger.LogError($"[DataTable] 加载失败: {ConfigName} (Buffer 为空)");
                return;
            }
            try 
            {
                using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
                {
                    LoadList(ms);
                }
                OnLoadListComple();
                // Debugger.EndProfile(DataTableName,$"配置表完成加载 | 数据行数: {Count} |");
            }
            catch (System.Exception e)
            {
                // 关键失败点：解析失败通常是数据格式版本不匹配，必须报出来
                Debugger.LogError($"[DataTable] 解析失败: {ConfigName} | 异常: {e.Message}");
            }
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
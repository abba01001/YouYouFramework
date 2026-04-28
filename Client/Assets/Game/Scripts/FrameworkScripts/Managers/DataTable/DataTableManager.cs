using Main;
using System;
using System.Collections.Generic;
using System.Reflection;
using GameScripts;
using UniRx;
using UnityEngine;

namespace GameScripts
{
    public class DataTableManager
    {
        internal Action OnLoadDataTableComplete;
    
        internal void Init()
        {
        }
    
        // 1. 你只需要在这里定义，名字必须和类名一致
        public LocalizationDBModel LocalizationDBModel { get; private set; }
        public Sys_UIFormDBModel Sys_UIFormDBModel { get; private set; }
        public Sys_BGMDBModel Sys_BGMDBModel { get; private set; }
        public Sys_AudioDBModel Sys_AudioDBModel { get; private set; }
        public Sys_SceneDBModel Sys_SceneDBModel { get; private set; }
        public Sys_GuideDBModel Sys_GuideDBModel { get; private set; }
        public Sys_AtlasDBModel Sys_AtlasDBModel { get; private set; }
        public Sys_DialogueDBModel Sys_DialogueDBModel { get; private set; }
        public Sys_UnlockFuncDBModel Sys_UnlockFuncDBModel { get; private set; }
        public Sys_ModelDBModel Sys_ModelDBModel { get; private set; }
    
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
        public void GetDataTableBuffer(string dataTableName, Action<byte[]> onComplete)
        {
            TextAsset asset =
                GameEntry.Loader.LoadMainAsset<TextAsset>($"Assets/Game/Download/DataTable/{dataTableName}.bytes");
            if (onComplete != null) onComplete(asset.bytes);
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
}
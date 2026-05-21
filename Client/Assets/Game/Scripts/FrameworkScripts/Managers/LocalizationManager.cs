using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts
{
    /// <summary>
    /// 多语言枚举
    /// </summary>
    public enum FrameworkLanguage
    {
        /// <summary>
        /// 中文
        /// </summary>
        Chinese = 0,
    
        /// <summary>
        /// 英文
        /// </summary>
        English = 1
    }
    
    
    public class LocalizationManager
    {
        public LocalizationManager()
        {
#if !UNITY_EDITOR
            switch (Application.systemLanguage)
            {
                default:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                case SystemLanguage.Chinese:
                    GameEntry.CurrLanguage = FrameworkLanguage.Chinese;
                    break;
                case SystemLanguage.English:
                    GameEntry.CurrLanguage = FrameworkLanguage.English;
                    break;
            }
#endif
        }
    
        /// <summary>
        /// 获取本地化文本内容
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string GetString(string key, params object[] args)
        {
            string value = null;
            if (GameEntry.DataTable.LocalizationDBModel.LocalizationDic.TryGetValue(key, out value))
            {
                return string.Format(value, args);
            }
    
            return value;
        }
    }
    
    
    /// <summary>
    /// LocalizationDBModel数据管理
    /// </summary>
    public partial class LocalizationDBModel : DataTableDBModelBase<LocalizationDBModel, DataTableEntityBase>
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public override string DataTableName
        {
            get { return "Localization/" + GameEntry.CurrLanguage.ToString(); }
        }
    
        /// <summary>
        /// 当前语言字典
        /// </summary>
        public Dictionary<string, string> LocalizationDic = new Dictionary<string, string>();
    
        /// <summary>
        /// 加载列表
        /// </summary>
        /// <param name="ms"></param>
        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();
    
            for (int i = 0; i < rows; i++)
            {
                LocalizationDic[ms.ReadUTF8String()] = ms.ReadUTF8String();
            }
        }
    }
}
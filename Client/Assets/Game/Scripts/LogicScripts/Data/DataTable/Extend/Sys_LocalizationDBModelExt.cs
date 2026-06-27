using System;
using System.Collections.Generic;
using GameScripts;
using Main;
using TMPro;
using UnityEngine;

namespace GameScripts
{
    // 多语言枚举
    public enum FrameworkLanguage
    {
        Chinese = 0,
        English = 1
    }

    public partial class Sys_LocalizationDBModel
    {
        // --- 新增：专门给解析器用的静态方法 ---
        public static void ParseBuffer(byte[] buffer, Action<Sys_LocalizationEntity> onEntityLoaded)
        {
            using (var ms = new MMO_MemoryStream(buffer))
            {
                int rows = ms.ReadInt();
                int columns = ms.ReadInt();
                for (int i = 0; i < rows; i++)
                {
                    var entity = new Sys_LocalizationEntity();
                    entity.Id = ms.ReadInt();
                    entity.Key = ms.ReadUTF8String();
                    entity.Chinese = ms.ReadUTF8String();
                    entity.English = ms.ReadUTF8String();
                    onEntityLoaded(entity);
                }
            }
        }
        
        // 定义一个结构体来保存映射关系
        private struct TextEntry
        {
            public TextMeshProUGUI Text;
            public string Key;
        }

        // 预定义的语言映射表 (如果未来增加语言，只需在此处添加一行)
        private static readonly Dictionary<FrameworkLanguage, Func<Sys_LocalizationEntity, string>> LangGetterMap =
            new Dictionary<FrameworkLanguage, Func<Sys_LocalizationEntity, string>>
            {
                { FrameworkLanguage.Chinese, e => e.Chinese },
                { FrameworkLanguage.English, e => e.English },
                // { FrameworkLanguage.Japanese, e => e.Japanese }, // 以后加语言只需这样
            };
        
        
        private readonly List<TextEntry> _entries = new List<TextEntry>();
        private Dictionary<string, string> _localeDict = new Dictionary<string, string>();

        protected override void OnLoadListComple()
        {
            base.OnLoadListComple();
            RefreshLocaleDict();
        }

        public void RefreshLocaleDict()
        {
            var lang = GameEntry.CurrLanguage;
            // 如果不支持该语言，默认使用中文
            if (!LangGetterMap.TryGetValue(lang, out var getter))
                getter = e => e.Chinese;

            _localeDict.Clear();
            foreach (var entity in m_List)
            {
                _localeDict[entity.Key] = getter(entity);
            }

            RefreshAll();
        }

        // 提供给外部的访问接口
        public string GetText(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";
            return _localeDict.GetValueOrDefault(key, key);
        }

        public void SetText(TextMeshProUGUI tmp, string key)
        {
            Register(tmp, key);
        }

        void Register(TextMeshProUGUI tmp, string key)
        {
            var index = _entries.FindIndex(x => x.Text == tmp);
            if (index != -1 && _entries[index].Key == key) return;

            Unregister(tmp);
            _entries.Add(new TextEntry { Text = tmp, Key = key });
            RefreshText(tmp, key);
        }

        void Unregister(TextMeshProUGUI tmp)
        {
            _entries.RemoveAll(x => x.Text == tmp);
        }

        // 调用此方法刷新所有文本
        void RefreshAll()
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var entry = _entries[i];
                if (entry.Text == null)
                {
                    _entries.RemoveAt(i);
                    continue;
                }
                RefreshText(entry.Text, entry.Key);
            }
        }

        private void RefreshText(TextMeshProUGUI tmp, string key)
        {
            tmp.text = GetText(key);
        }
    }
}
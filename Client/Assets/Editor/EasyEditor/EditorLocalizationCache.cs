#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq; // 记得加上这个，方便转数组
using GameScripts;

[InitializeOnLoad]
public static class EditorLocalizationCache
{
    private static Dictionary<string, string> _dict = new Dictionary<string, string>();
    private static List<string> _allKeys = new List<string>(); // 新增：缓存所有的 Key

    static EditorLocalizationCache() => Load();

    public static void Load()
    {
        string path = Path.Combine(UnityEngine.Application.dataPath, "Game/Download/DataTable/Sys_Localization.bytes");
        if (!File.Exists(path)) return;

        _dict.Clear();
        _allKeys.Clear(); // 清空旧列表

        byte[] buffer = File.ReadAllBytes(path);
        
        Sys_LocalizationDBModel.ParseBuffer(buffer, (e) => {
            _dict[e.Key] = e.Chinese; 
            _allKeys.Add(e.Key); // 新增：解析时顺便添加 Key
        });
    }

    // 新增：提供给 Inspector 面板调用的接口
    public static string[] GetAllKeys() => _allKeys.ToArray();

    public static string GetText(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";
        return _dict.GetValueOrDefault(key, key);
    }
}
#endif
using System.Collections.Generic;
using System.IO;
using GameScripts;
using Main;
using UnityEngine;
using TMPro; // 必须引用

[RequireComponent(typeof(TextMeshProUGUI))] // 自动添加 TMP 组件
public class LocalizedTMP : MonoBehaviour
{
#if UNITY_EDITOR
    private static System.Type _cachedType;
    private static System.Reflection.MethodInfo _cachedMethod;
#endif

    [HideInInspector]
    public string m_Key; // 同样隐藏，因为我们已经在 Editor 里用自定义字段管理了
    public string Key 
    { 
        get => m_Key; 
        set { m_Key = value; UpdateText(null); } 
    }

    private TextMeshProUGUI _tmpText;

    void Awake()
    {
        _tmpText = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        // 注册事件：语言切换时自动刷新
        GameEntry.Event.AddEventListener(Constants.EventName.LanguageChangedEvent, UpdateText);
        UpdateText(null);
    }

    void OnDisable()
    {
        // 反注册，防止内存泄漏
        GameEntry.Event.RemoveEventListener(Constants.EventName.LanguageChangedEvent, UpdateText);
    }

    void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (_tmpText == null)
            {
                _tmpText = GetComponent<TextMeshProUGUI>();
            }
            // 延迟一帧调用，防止在某些 Unity 版本中组件未初始化完全
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) UpdateText(null);
            };
        }
#endif
    }
    
    public void UpdateText(object userdata = null)
    {
        if (_tmpText == null)
        {
            _tmpText = GetComponent<TextMeshProUGUI>();
        }
    
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // 只有缓存为空时才去反射查找，只查找一次！
            if (_cachedType == null)
            {
                _cachedType = System.Type.GetType("EditorLocalizationCache, MyEditor");
                if (_cachedType != null) _cachedMethod = _cachedType.GetMethod("GetText", new[] { typeof(string) });
            }
            if (_cachedMethod != null)
            {
                _tmpText.text = (string)_cachedMethod.Invoke(null, new object[] { m_Key });
            }
            return;
        }
#endif
        if (GameEntry.Config?.Sys_LocalizationDBModel != null)
        {
            _tmpText.text = GameEntry.Config.Sys_LocalizationDBModel.GetText(m_Key);
            Debugger.Log(m_Key,"===", GameEntry.Config.Sys_LocalizationDBModel.GetText(m_Key));
        }
    }
}
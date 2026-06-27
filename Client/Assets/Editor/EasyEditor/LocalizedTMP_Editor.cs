#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizedTMP))]
public class LocalizedTMP_Editor : Editor
{
    private string[] _keyOptions;
    private int _selectedIndex;
    private SerializedProperty _keyProperty;

    private void OnEnable()
    {
        _keyOptions = EditorLocalizationCache.GetAllKeys();
        _keyProperty = serializedObject.FindProperty("m_Key");
        
        // 初始化下拉菜单索引
        _selectedIndex = System.Array.IndexOf(_keyOptions, _keyProperty.stringValue);
        if (_selectedIndex < 0) _selectedIndex = 0;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        _selectedIndex = System.Array.IndexOf(_keyOptions, _keyProperty.stringValue);
        if (_selectedIndex < 0) _selectedIndex = 0;
        
        // 1. 顶部标题区
        GUILayout.Space(5);
        EditorGUILayout.LabelField("多语言配置", EditorStyles.boldLabel);
        
        // 2. 下拉选择器
        EditorGUI.BeginChangeCheck();
        _selectedIndex = EditorGUILayout.Popup("选择语言Key", _selectedIndex, _keyOptions);
        if (EditorGUI.EndChangeCheck())
        {
            _keyProperty.stringValue = _keyOptions[_selectedIndex];
        }

        // 3. 文本输入框
        EditorGUILayout.PropertyField(_keyProperty, new GUIContent("当前Key"));

        // 4. 预览区（使用 HelpBox 显示中文，非常直观）
        string preview = EditorLocalizationCache.GetText(_keyProperty.stringValue);
        EditorGUILayout.HelpBox("预览: " + preview, MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
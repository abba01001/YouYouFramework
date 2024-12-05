using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class LevelEditorWindow : OdinEditorWindow
{
    private const string LevelDataDirectory = "Assets/Game/Download/MapLevel"; // 关卡文件夹路径
    private const string DefaultBasePath = "Assets/Game/Download/MapLevel/"; // 默认的保存路径
    private string basePath = DefaultBasePath; // 当前保存路径

    // 窗口显示
    [MenuItem("Tools/关卡编辑器")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("关卡编辑器");
    }

    [HorizontalGroup("LevelInfo", width: 0.1f)] [Button("新建关卡", ButtonSizes.Medium)]
    public void CreateLevel() => CreateCurrentLevel();
    
    [HorizontalGroup("LevelInfo", width: 0.2f)] [LabelText("关卡ID")] [OnValueChanged("UpdateSavePath")] [PropertySpace(2)]
    public string levelName = ""; // 默认关卡名

    [HorizontalGroup("LevelInfo", width: 0.1f)] [Button("保存关卡", ButtonSizes.Medium)]
    public void SaveLevel() => SaveCurrentLevel();

    [HorizontalGroup("LevelInfo", width: 0.1f)] [Button("加载关卡", ButtonSizes.Medium)]
    public void LoadLevel() => LoadCurrentLevel();
    [HorizontalGroup("LevelInfo", width: 0.1f)] [Button("扫描关卡", ButtonSizes.Medium)]
    public void CheckLevel() => LevelDataComparer.ShowWindow();

    [BoxGroup("关卡编辑器")] [LabelText("保存路径")] [ReadOnly]
    public string fullSavePath;
    
    [BoxGroup("关卡编辑器")] [LabelText("关卡数据")]
    public LevelData CurLevelData;
    
    // 实时更新保存路径
    private void UpdateSavePath()
    {
        fullSavePath = Path.Combine(DefaultBasePath, levelName + ".json");
        Repaint();
    }

    private void OnEnable()
    {
        UpdateSavePath();
    }

    // 保存关卡
    private void SaveCurrentLevel()
    {
        if (string.IsNullOrEmpty(levelName))
        {
            EditorUtility.DisplayDialog(
                "提示",
                "关卡名称不能为空！",
                "确定"
            );
            return;
        }

        if (File.Exists(fullSavePath) && !ConfirmOverwrite(fullSavePath)) return;
        SaveLevelDataToFile(fullSavePath, CurLevelData);
    }

    // 确认覆盖文件
    private bool ConfirmOverwrite(string path)
    {
        return EditorUtility.DisplayDialog("确认覆盖", $"文件 '{path}' 已经存在，是否覆盖？", "是", "否");
    }

    // 保存关卡数据到文件
    private void SaveLevelDataToFile(string path, LevelData levelData)
    {
        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(path, json);
        Debug.Log($"关卡数据已保存到: {path}");
    }

    // 加载关卡
    private void LoadCurrentLevel()
    {
        if (string.IsNullOrEmpty(levelName))
        {
            EditorUtility.DisplayDialog(
                "提示",
                "关卡名称不能为空！",
                "确定"
            );
            return;
        }

        if (!File.Exists(fullSavePath))
        {
            Debug.LogWarning($"未找到文件: {fullSavePath}");
            return;
        }

        string json = File.ReadAllText(fullSavePath);
        LoadLevelDataFromJson(json);
    }

    //新建关卡
    private void CreateCurrentLevel()
    {
        bool bo = EditorUtility.DisplayDialog(
            "提示","请确保当前关卡是否保存！","确定","取消"
        );
        if(!bo) return;
        CurLevelData = new LevelData();
        Repaint();
    }
    
    // 从JSON加载关卡数据
    private void LoadLevelDataFromJson(string json)
    {
        CurLevelData = JsonUtility.FromJson<LevelData>(json);
        // 清理之前的场景
        CleanUpDungeon();

        Repaint();
    }

    // 清理当前场景
    private void CleanUpDungeon()
    {
        // 可以在这里处理清理之前场景的逻辑
    }

}
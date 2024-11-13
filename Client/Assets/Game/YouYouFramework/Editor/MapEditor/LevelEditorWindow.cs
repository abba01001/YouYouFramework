using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class LevelEditorWindow : OdinEditorWindow
{
    private const string DefaultBasePath = "Assets/Game/Download/MapLevel/"; // 默认的保存路径
    private string basePath = DefaultBasePath; // 当前保存路径

    // 窗口显示
    [MenuItem("Tools/关卡编辑器")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("关卡编辑器");
    }

    // 关卡名称与保存按钮
    [HorizontalGroup("LevelInfo")] 
    [LabelText("关卡名称")] 
    [PropertySpace(10)] 
    [OnValueChanged("UpdateSavePath")]
    public string levelName = "DefaultLevel"; // 默认关卡名

    [HorizontalGroup("LevelInfo", width: 0.3f)]
    [Button("保存关卡", ButtonSizes.Large)]
    [GUIColor(0.5f, 0.8f, 0.5f)]
    [PropertySpace(5)]
    public void SaveLevel() => SaveCurrentLevel();

    [HorizontalGroup("LevelInfo", width: 0.3f)]
    [Button("加载关卡", ButtonSizes.Large)]
    [GUIColor(0.5f, 0.8f, 0.5f)]
    [PropertySpace(5)]
    public void LoadLevel() => LoadCurrentLevel();

    // 保存路径设置
    [BoxGroup("关卡编辑器")] 
    [LabelText("保存路径")] 
    [ReadOnly]
    public string fullSavePath;

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
            Debug.LogWarning("关卡名称不能为空！");
            return;
        }

        if (File.Exists(fullSavePath) && !ConfirmOverwrite(fullSavePath)) return;

        LevelData levelData = GatherLevelData();
        SaveLevelDataToFile(fullSavePath, levelData);
    }

    // 确认覆盖文件
    private bool ConfirmOverwrite(string path)
    {
        return EditorUtility.DisplayDialog("确认覆盖", $"文件 '{path}' 已经存在，是否覆盖？", "是", "否");
    }

    // 收集关卡数据
    private LevelData GatherLevelData()
    {
        LevelData levelData = new LevelData
        {
            levelID = 1, // 这里可以按需要设置ID
            levelName = levelName,  // 使用 levelName 的值
            levelDescription = "关卡描述", // 按需要设置描述
            levelDifficulty = 1, // 设置关卡难度，1表示简单
            stages = new List<LevelStage>(), // 这里可以添加阶段数据
            events = new List<EventData>(), // 这里可以添加事件数据
            rewards = new List<RewardData>(), // 这里可以添加奖励数据
            goal = LevelGoal.DefeatAllEnemies, // 设置目标
            backgroundMusic = "BackgroundMusicPath", // 设置背景音乐
            mapSceneName = "MapSceneName", // 设置场景名
            levelTips = new List<string> { "关卡提示信息" } // 设置关卡提示信息
        };

        // 如果有具体数据，可以填充 stages, events 和 rewards 等列表
        // 例如：
        // levelData.stages.Add(new LevelStage { stageName = "第一波敌人", ... });

        return levelData;
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
            Debug.LogWarning("关卡名称不能为空！");
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

    // 从JSON加载关卡数据
    private void LoadLevelDataFromJson(string json)
    {
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);
        Debug.Log($"加载关卡: {levelData.levelName}");

        // 清理之前的场景
        CleanUpDungeon();

        // 处理关卡数据，例如加载敌人、奖励等
        foreach (var stage in levelData.stages)
        {
            // 加载每个阶段
            Debug.Log($"加载阶段: {stage.stageName}");
        }
    }

    // 清理当前场景
    private void CleanUpDungeon()
    {
        // 可以在这里处理清理之前场景的逻辑
    }

    // 将LevelData直接显示并可编辑
    [FoldoutGroup("关卡数据")]
    [PropertySpace(10)]
    public LevelData levelData = new LevelData();

    // 对于关卡中的子数据（如阶段、事件等），使用 [InlineEditor] 或 [ListDrawerSettings] 来序列化显示
    [FoldoutGroup("关卡数据/阶段")]
    [LabelText("阶段信息")]
    [InlineEditor(InlineEditorModes.FullEditor)]
    public List<LevelStage> stages = new List<LevelStage>();

    [FoldoutGroup("关卡数据/事件")]
    [LabelText("事件信息")]
    [InlineEditor(InlineEditorModes.FullEditor)]
    public List<EventData> events = new List<EventData>();

    [FoldoutGroup("关卡数据/奖励")]
    [LabelText("奖励信息")]
    [InlineEditor(InlineEditorModes.FullEditor)]
    public List<RewardData> rewards = new List<RewardData>();

}

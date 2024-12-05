using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class LevelDataComparer : EditorWindow
{
    private const string LevelDataDirectory = "Assets/Game/Download/MapLevel"; // 关卡文件夹路径
    private List<string> levelFiles = new List<string>(); // 存储所有关卡文件路径
    private List<string> differences = new List<string>(); // 存储结构差异的日志
    private Dictionary<int,string> levelIdList = new Dictionary<int,string>();
    public static void ShowWindow()
    {
        GetWindow<LevelDataComparer>("关卡数据结构比较");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("开始比较"))
        {
            CompareLevelDataStructures();
        }

        EditorGUILayout.Space();
        GUILayout.Label("差异日志：", EditorStyles.boldLabel);
        foreach (var diff in differences)
        {
            GUILayout.Label(diff);
        }
    }

    private void CompareLevelDataStructures()
    {
        // 获取所有关卡文件路径
        levelFiles.Clear();
        string[] files = Directory.GetFiles(LevelDataDirectory, "*.json");
        levelFiles.AddRange(files);

        // 清空差异日志
        differences.Clear();

        foreach (var file in levelFiles)
        {
            string json = File.ReadAllText(file);
            LevelData loadedLevel = JsonUtility.FromJson<LevelData>(json);

            // 比较关卡数据结构
            CompareStructure(loadedLevel, file);
        }
    }

    private void CompareStructure(LevelData loadedLevel, string filePath)
    {
        if (!levelIdList.ContainsKey(loadedLevel.levelID))
        {
            levelIdList.Add(loadedLevel.levelID,filePath);
        }
        else
        {
            differences.Add($"[{filePath}]===关卡ID重复===[{levelIdList[loadedLevel.levelID]}]");
        }
        
        // 比较每个字段
        if (loadedLevel.levelID == -1)
        {
            differences.Add($"[{filePath}] 关卡ID为空");
        }
        
        // 比较每个字段
        if (string.IsNullOrEmpty(loadedLevel.path))
        {
            differences.Add($"[{filePath}] 关卡路径为空");
        }

        if (string.IsNullOrEmpty(loadedLevel.levelName))
        {
            differences.Add($"[{filePath}] 关卡名称为空");
        }

        // 你可以根据新的结构继续扩展，比较每个字段是否存在或是否为默认值
        if (loadedLevel.stages == null || loadedLevel.stages.Count == 0)
        {
            differences.Add($"[{filePath}] 关卡阶段为空");
        }

        // 其他字段的比较逻辑可以继续补充
        if (loadedLevel.rewards == null || loadedLevel.rewards.Count == 0)
        {
            differences.Add($"[{filePath}] 关卡奖励为空");
        }
        
        // ... 根据新的字段继续扩展比较
    }
}

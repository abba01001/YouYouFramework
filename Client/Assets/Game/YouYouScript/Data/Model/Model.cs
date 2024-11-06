using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MessagePack;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class TestModel
{
    public string path;
    public long version;
}

[Serializable]
public class LevelModelData
{
    public string modelPrefabName;  // 模型的Prefab路径
    public Vector3 position;         // 模型的位置
    public Quaternion rotation;      // 模型的旋转
    public Vector3 scale;            // 模型的缩放
    
    public LevelModelData() 
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        scale = Vector3.one;
    }
}

[Serializable]
[MessagePackObject(keyAsPropertyName: true)]
public class PlayerRoleData
{
    public List<float> playerBornPos;
    public List<float> playerPos;
    public List<float> playerRotate;
    public List<float> cameraRotate;
    public bool firstEntryLevel;
    public int levelId;
    public int curExp;
    public int totalExp;
    public int curHp;
    public int maxHp;
    public int curMp;
    public int maxMp;
    public int totalOnlineDuration;
    public int todayOnlineDuration;
    public Dictionary<int,int> dialogueIds;
    public PlayerRoleData()
    {
        playerBornPos = new List<float>();
        playerPos = new List<float>();
        playerRotate = new List<float>();
        cameraRotate = new List<float>();
        firstEntryLevel = true;
        levelId = 1;
        dialogueIds = new Dictionary<int, int>();
        totalOnlineDuration = 0;
        todayOnlineDuration = 0;
    }
}

public class DialogueCommand
{
    public string type { get; set; }        // 命令类型 (如 say, wait, jump)
    public string block { get; set; }       // 区块名称 (适用于 say, wait)
    public string text { get; set; }        // 对话文本 (适用于 say)
    public int characterId { get; set; }    // 角色 ID (适用于 say)
    public string sprite { get; set; }      // 角色精灵路径 (适用于 say)
    public float duration { get; set; }     // 等待时间 (适用于 wait)
    public string fromBlock { get; set; }   // 源区块 (适用于 jump)
    public string toBlock { get; set; }     // 目标区块 (适用于 jump)
}

[Serializable]
public class LevelData
{
    public List<LevelModelData> models;  // 存储模型数据的列表
    public bool isRandomGenerated; // 添加随机地图标志
    public List<float> bornPos;
    public LevelData()
    {
        models = new List<LevelModelData>();
        isRandomGenerated = false;
        bornPos = new List<float>(3) {0, 0, 0};
    }
}

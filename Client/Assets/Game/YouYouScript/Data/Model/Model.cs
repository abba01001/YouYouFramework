using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MessagePack;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class TestModel
{
    public string path;
    public long version;
}

public class DialogueModel
{
    public int dialogueId;
    public float delay = default;
    public Action finishAction;
}

public class LevelModel
{
    public int levelId;
    public int mapLevel;
    public int levelNum;
    public LevelData levelData;
}


[Serializable]
[MessagePackObject(keyAsPropertyName: true)]
public class PlayerRoleData
{
    public string name;
    public int totalOnlineDuration;
    public int todayOnlineDuration;
    public float masterVolume;
    public float audioVolume;
    public float bgmVolume;
    public List<int> dialogueIds;
    public List<int> guideIds;
    public List<string> guideEvent;
    public int curGuide;
    public Dictionary<int, int> equipLevels;//装备栏等级信息
    public Dictionary<string, int> roleAttr;//角色属性
    public List<BagItemData> bagWareHouse;//背包仓库
    public PlayerRoleData()
    {
        equipLevels = new Dictionary<int, int>()
        {
            {1,0},{2,0},{3,0},{4,0},{5,0},{6,0},
        };
        guideIds = new List<int>();
        guideEvent = new List<string>();
        dialogueIds = new List<int>();
        bagWareHouse = new List<BagItemData>();
        roleAttr = new Dictionary<string, int>();
        totalOnlineDuration = 0;
        todayOnlineDuration = 0;
        masterVolume = 0.5f;
        audioVolume = 0.5f;
        bgmVolume = 0.5f;
        name = "";
    }
}

[Serializable]
[MessagePackObject(keyAsPropertyName: true)]
public class BagItemData
{
    public int itemId;
    public int itemCount;
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

public class HeroModel
{
    public int modelId;
}

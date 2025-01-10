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

[Serializable]
[MessagePackObject(keyAsPropertyName: true)]
public class PlayerRoleData
{
    public string name;
    public int totalOnlineDuration;
    public int todayOnlineDuration;
    public Dictionary<int,int> dialogueIds;
    public Dictionary<int, int> equipLevels;//装备栏等级信息
    public List<EqiupItemData> equipWareHouse;//装备仓库
    public List<BagItemData> bagWareHouse;//背包仓库
    public Dictionary<string, int> roleAttr;//角色属性
    public PlayerRoleData()
    {
        equipLevels = new Dictionary<int, int>()
        {
            {1,0},{2,0},{3,0},{4,0},{5,0},{6,0},
        };
        dialogueIds = new Dictionary<int, int>();
        equipWareHouse = new List<EqiupItemData>();
        bagWareHouse = new List<BagItemData>();
        roleAttr = new Dictionary<string, int>();
        totalOnlineDuration = 0;
        todayOnlineDuration = 0;
        name = "";
    }
}

[Serializable]
[MessagePackObject(keyAsPropertyName: true)]
public class EqiupItemData
{
    public int equipId;
    public int quality;
    public Dictionary<int, int> extraAttr = new Dictionary<int, int>();
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
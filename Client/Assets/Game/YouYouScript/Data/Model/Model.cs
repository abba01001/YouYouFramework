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
[MessagePackObject(keyAsPropertyName: true)]
public class PlayerRoleData
{
    public string name;
    public int totalOnlineDuration;
    public int todayOnlineDuration;
    public Dictionary<int,int> dialogueIds;
    public Dictionary<int, int> equipInfo;//穿戴装备信息
    public Dictionary<int, int> bagWareHouse;//背包仓库
    public Dictionary<int, int> equipWareHouse;//装备仓库
    public Dictionary<string, int> roleAttr;//角色属性
    public PlayerRoleData()
    {
        dialogueIds = new Dictionary<int, int>();
        equipInfo = new Dictionary<int, int>();
        bagWareHouse = new Dictionary<int, int>();
        equipWareHouse = new Dictionary<int, int>();
        roleAttr = new Dictionary<string, int>();
        totalOnlineDuration = 0;
        todayOnlineDuration = 0;
        name = "";
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
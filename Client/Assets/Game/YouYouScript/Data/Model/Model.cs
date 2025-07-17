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
    public List<EqiupItemData> equipWareHouse;//装备仓库
    public List<BagItemData> bagWareHouse;//背包仓库
    public RestaurantData restaurantData;
    public PlayerRoleData()
    {
        restaurantData = new RestaurantData();
        equipLevels = new Dictionary<int, int>()
        {
            {1,0},{2,0},{3,0},{4,0},{5,0},{6,0},
        };
        guideIds = new List<int>();
        guideEvent = new List<string>();
        dialogueIds = new List<int>();
        equipWareHouse = new List<EqiupItemData>();
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
public class EqiupItemData
{
    public int equipId;
    public int quality;
    public bool isWear;
    public Dictionary<int, int> extraAttr = new Dictionary<int, int>();
}

[Serializable]
[MessagePackObject(keyAsPropertyName: true)]
public class BagItemData
{
    public int itemId;
    public int itemCount;
}

[Serializable]
[MessagePackObject(keyAsPropertyName: true)]
public class RestaurantData
{
    public List<CustomerData> customers = new List<CustomerData>();
    public List<HelperData> helpers = new List<HelperData>();
    public List<BuildingData> buildings = new List<BuildingData>();
}

[Serializable]
[MessagePackObject(keyAsPropertyName: true)]
public class BuildingData
{
    public int buildingId;
}

[Serializable]
[MessagePackObject(keyAsPropertyName: true)]
public class CustomerData
{
    public int customerId;
    public int type;//类型
    public string name;
    public List<int> needFoodList =  new List<int>();//所需食物列表
    public List<int> hasFoodList =  new List<int>();//拥有实物列表
    public int hatColorIndex;//帽子颜色索引
    public int meshColorIndex;//网格颜色索引
}

[Serializable]
[MessagePackObject(keyAsPropertyName: true)]
public class HelperData
{
    public int helperId;
    public int type;//类型
    public string name;
    public int maxFoodCarry;//食物容量
    public int speed;//速度
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

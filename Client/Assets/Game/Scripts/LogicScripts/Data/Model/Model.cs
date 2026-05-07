using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MessagePack;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

namespace GameScripts
{
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

    [Serializable]
    [MessagePackObject(keyAsPropertyName: true)]
    public class PlayerRoleData
    {
        public Dictionary<int, int> propDic;//货币数据
        public string name;
        public int totalOnlineDuration;
        public int todayOnlineDuration;
        public float soundVolume;
        public float musicVolume;
        public List<int> dialogueIds;
        public List<int> guideIds;
        public List<string> guideEvent;
        public int curGuide;
        public bool isVibrationEnabled;
        public PlayerRoleData()
        {
            propDic = new Dictionary<int, int>()
            {
                {(int)PropEnum.Coin,100},
                {(int)PropEnum.Energy,5},
            };
            guideIds = new List<int>();
            guideEvent = new List<string>();
            dialogueIds = new List<int>();
            totalOnlineDuration = 0;
            todayOnlineDuration = 0;
            soundVolume = 0.5f;
            musicVolume = 0.5f;
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

    public class PropChangeModel
    {
        public PropEnum PropType { get; set; }
        public int PropValue { get; set; }
    }
}
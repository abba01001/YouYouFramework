using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    [Header("关卡id")]
    public int levelID;              // 关卡ID
    public string levelName;         // 关卡名称
    public string levelDescription;  // 关卡描述
    public int levelDifficulty;      // 关卡难度 (1 = Easy, 2 = Medium, 3 = Hard)
    public List<LevelStage> stages;  // 关卡包含的各个阶段
    public List<EventData> events;   // 该关卡的事件（商店、宝箱、特殊事件等）
    public List<RewardData> rewards; // 该关卡的奖励
    public LevelGoal goal;           // 关卡目标
    public string backgroundMusic;   // 背景音乐资源路径
    public string mapSceneName;      // 关卡地图场景名
    public List<string> levelTips;   // 关卡提示信息（例如教程、说明等）
}

[System.Serializable]
public class LevelStage
{
    public string stageName;         // 阶段名称（例如：第一波敌人、第二波敌人）
    public List<EnemyData> enemies;  // 该阶段的敌人
    public List<EventData> events;   // 该阶段的特殊事件
    public List<RewardData> rewards; // 该阶段的奖励
    public float stageDuration;      // 阶段持续时间（例如回合数或计时）
}

[System.Serializable]
public class EnemyData
{
    public string enemyName;         // 敌人名称
    public int health;               // 敌人血量
    public int attack;               // 敌人攻击力
    public int defense;              // 敌人防御力
    public string enemyType;         // 敌人类型（普通、精英、boss等）
    public int level;                // 敌人的等级，影响属性
    public List<EnemySkill> skills;  // 敌人的技能
}

[System.Serializable]
public class EnemySkill
{
    public string skillName;         // 技能名称
    public string skillDescription;  // 技能描述
    public int cooldownTurns;        // 技能冷却回合数
    public int damage;               // 技能伤害（根据技能设定）
    public string effect;            // 技能效果（如：眩晕、火焰伤害等）
}

[System.Serializable]
public class EventData
{
    public string eventName;         // 事件名称（例如：商店、宝箱、陷阱等）
    public string eventDescription;  // 事件描述
    public List<string> eventChoices; // 事件选项（例如商店购买、选择路径等）
    public string eventOutcome;      // 事件结果（例如：获得金币、受到伤害、增加卡牌等）
    public bool isDynamic;           // 事件是否是动态的（例如基于玩家选择或条件触发）
    public string triggerCondition;  // 触发条件（例如玩家是否拥有某物品、是否达成某条件等）
}

[System.Serializable]
public class RewardData
{
    public string rewardType;        // 奖励类型（例如：金币、卡牌等）
    public int amount;               // 奖励数量或价值
    public string rewardDescription; // 奖励描述
    public string itemID;            // 如果奖励是物品，则存储物品的ID
}

[System.Serializable]
public enum LevelGoal
{
    DefeatAllEnemies,  // 击败所有敌人
    CollectItems,      // 收集物品
    SurviveRounds,     // 生存若干回合
    ReachTarget,       // 达到目标位置
    SpecialEvent       // 完成特殊事件
}

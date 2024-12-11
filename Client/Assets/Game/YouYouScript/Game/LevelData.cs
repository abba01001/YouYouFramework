using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    [LabelText("关卡ID")] public int levelID = -1; // 关卡ID
    [LabelText("关卡路径")] public string path; // 关卡路径
    [LabelText("战斗背景")] public string bgPath; // 关卡ID
    [LabelText("关卡名称")] public string levelName; // 关卡名称
    [LabelText("关卡描述")] public string levelDescription; // 关卡描述
    [LabelText("关卡难度")] public LevelDifficulty difficulty; // 关卡难度 (1 = Easy, 2 = Medium, 3 = Hard)
    
    
    [LabelText("关卡目标")]  public LevelGoal goal; // 关卡目标
    [ShowIf("IsGoalCollectItems"), LabelText("收集指定物品"),Indent(1)] public List<ItemData> collectItems;
    [ShowIf("IsGoalDefeatInputEnemies"), LabelText("击败配置敌人数"),Indent(1)] public int inputEnemyCount;
    private bool IsGoalCollectItems() => goal == LevelGoal.CollectItems;
    private bool IsGoalDefeatInputEnemies() => goal == LevelGoal.DefeatInputEnemies;
    
    
    [LabelText("关卡阶段")] public List<LevelStage> stages; // 关卡包含的各个阶段
    [LabelText("关卡事件")] public List<EventData> events; // 该关卡的事件（商店、宝箱、特殊事件等）
    [LabelText("关卡奖励")] public List<ItemData> rewards; // 该关卡的奖励
    [LabelText("背景音乐")] public List<string> backgroundMusic; // 背景音乐资源路径
    [LabelText("关卡提示")] public List<string> levelTips; // 关卡提示信息（例如教程、说明等）
}

[System.Serializable]
public class LevelStage
{
    [LabelText("阶段ID")] public string stageIndex; //阶段索引
    [LabelText("开始时间(第x秒)")] public int startTime; // 阶段持续时间（例如回合数或计时）
    [LabelText("怪物模型")] public EnemyData enemy; // 该阶段的敌人
    [LabelText("阶段事件")] public List<EventData> events; // 该阶段的特殊事件
    [LabelText("阶段奖励")] public List<ItemData> finalRewards; // 该阶段的奖励
}

[System.Serializable]
public class EnemyData
{
    [LabelText("模型ID")] public int modelId; // 模型ID
    [LabelText("血量")] public int health; // 敌人血量
    [LabelText("攻击力")] public int attack; // 敌人攻击力
    [LabelText("防御力")] public int defense; // 敌人防御力
    [LabelText("类型")] public string enemyType; // 敌人类型（普通、精英、boss等）
    [LabelText("等级")] public int level; // 敌人的等级，影响属性
    [LabelText("怪物数量")] public int enemyCount; // 敌人数量
    [LabelText("产生间隔")] public float interval; // 生成间隔
    [LabelText("技能")] public List<EnemySkill> skills; // 敌人的技能
    [LabelText("奖励")] public List<ItemData> rewards; // 使用 List<RewardData> 替代 Dictionary
}


[System.Serializable]
public class EnemySkill
{
    public string skillName; // 技能名称
    public string skillDescription; // 技能描述
    public int cooldownTurns; // 技能冷却回合数
    public int damage; // 技能伤害（根据技能设定）
    public string effect; // 技能效果（如：眩晕、火焰伤害等）
}

[System.Serializable]
public class EventData
{
    [LabelText("事件ID")] public int eventId; // 事件ID
    [LabelText("事件参数")] public string eventParams; // 事件参数
}

[System.Serializable]
public class ItemData
{
    [LabelText("物品")]public int type; // 奖励类型（例如：金币、卡牌等）
    [LabelText("数量")]public int amount; // 奖励数量或价值
}

[System.Serializable]
public enum LevelGoal
{
    [LabelText("无配置")] None,
    [LabelText("击败关卡所有敌人")] DefeatAllEnemies, // 击败所有敌人
    [LabelText("击败配置敌人数")] DefeatInputEnemies, // 击败所有敌人
    [LabelText("收集指定物品")] CollectItems, // 收集物品
    [LabelText("生存若干回合")] SurviveRounds, // 生存若干回合
    [LabelText("完成特殊事件")] SpecialEvent // 完成特殊事件
}

[System.Serializable]
public enum LevelDifficulty
{
    [LabelText("简单")] Easy,         // 简单
    [LabelText("中等")] Medium,       // 中等
    [LabelText("困难")] Hard,         // 困难
    [LabelText("地狱")] Nightmare     // 地狱
}

[System.Serializable]
public enum StageEvenName
{
    [LabelText("简单")] Easy,         // 简单
    [LabelText("中等")] Medium,       // 中等
    [LabelText("困难")] Hard,         // 困难
    [LabelText("地狱")] Nightmare     // 地狱
}

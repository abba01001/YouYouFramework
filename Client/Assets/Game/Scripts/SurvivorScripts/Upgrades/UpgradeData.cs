using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace OctoberStudio.Upgrades
{
    [CreateAssetMenu(fileName = "Upgrades", menuName = "October/Upgrades/Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        [SerializeField] UpgradeType upgradeType;
        [SerializeField] Sprite icon;
        [SerializeField] string title;
        [SerializeField] int devStartLevel = 0;

        // --- 伤害：保持原样 ---
        [Header("伤害加成曲线配置")]
        [ShowIf(nameof(upgradeType), UpgradeType.Damage)]
        [LabelText("基础伤害加成")] public float baseAdd_Damage = 2f;
        [ShowIf(nameof(upgradeType), UpgradeType.Damage)]
        [LabelText("每级伤害递增")] public float addStep_Damage = 0.35f;

        // --- 生命：保持原样 ---
        [Header("生命加成曲线配置")]
        [ShowIf(nameof(upgradeType), UpgradeType.Health)]
        [LabelText("基础生命加成")] public float baseAdd_Health = 20f;
        [ShowIf(nameof(upgradeType), UpgradeType.Health)]
        [LabelText("每级生命递增")] public float addStep_Health = 4f;

        // --- 护甲：保持原样 ---
        [Header("护甲减免曲线配置")]
        [ShowIf(nameof(upgradeType), UpgradeType.Armor)]
        [LabelText("基础护甲数值")] public float baseAdd_Armor = 1f;
        [ShowIf(nameof(upgradeType), UpgradeType.Armor)]
        [LabelText("每级护甲减免递增")] public float addStep_Armor = 0.01f;

        // --- 治疗：保持原样 ---
        [Header("血量回复曲线配置")]
        [ShowIf(nameof(upgradeType), UpgradeType.Healing)]
        [LabelText("基础回血加成")] public float baseAdd_Healing = 0.5f;
        [ShowIf(nameof(upgradeType), UpgradeType.Healing)]
        [LabelText("每级回血递增")] public float addStep_Healing = 0.1f;

        // --- 费用：保持原样 ---
        [Header("费用配置")]
        [LabelText("初始升级费用(整10)")]
        public int baseCost = 50;
        [LabelText("费用膨胀系数")]
        public float costFactor = 1.082f;

        public UpgradeType UpgradeType => upgradeType;
        public Sprite Icon => icon;
        public string Title => title;
        public int DevStartLevel => devStartLevel;

        [SerializeField] List<UpgradeLevel> levels;

        public int LevelsCount => levels.Count;
        public UpgradeLevel GetLevel(int id) => levels[id];

        [Button("✅ 生成 1~100级｜金币整10", ButtonSizes.Large), PropertyOrder(999)]
        private void Generate100Levels()
        {
            levels.Clear();

            for (int lv = 1; lv <= 100; lv++)
            {
                float addValue = 0;
                float multiplyValue = 0;

                // 核心逻辑：针对每个属性类型，使用完全独立的计算公式
                switch (upgradeType)
                {
                    case UpgradeType.Damage:
                        // 逻辑：基础值 + (等级-1) * 步长
                        addValue = baseAdd_Damage + (lv - 1) * addStep_Damage;
                        break;

                    case UpgradeType.Health:
                        // 逻辑：基础值 + (等级-1) * 步长
                        addValue = baseAdd_Health + (lv - 1) * addStep_Health;
                        break;

                    case UpgradeType.Armor:
                        // 逻辑：基础值 - (等级-1) * 步长 (护甲通常是百分比减伤，用multiplyValue存储)
                        multiplyValue = baseAdd_Armor - (lv - 1) * addStep_Armor;
                        break;

                    case UpgradeType.Healing:
                        // 逻辑：基础值 + (等级-1) * 步长
                        addValue = baseAdd_Healing + (lv - 1) * addStep_Healing;
                        break;

                    case UpgradeType.Revive:
                        addValue = 1;
                        break;
                }

                // 费用逻辑：完全保持你原来的
                float rawCost = baseCost * Mathf.Pow(costFactor, lv - 1);
                int cost = Mathf.RoundToInt(rawCost / 10f) * 10;

                levels.Add(new UpgradeLevel(cost, addValue, multiplyValue));
            }

            Debug.Log($"✅ {title} 1~100级生成完成");
        }

        [Button("🗑️ 清空等级数据")]
        private void ClearLevels() => levels.Clear();
    }

    [System.Serializable]
    public class UpgradeLevel
    {
        [SerializeField] int cost;
        [SerializeField] float addValue;
        [SerializeField] float multiplyValue;

        public UpgradeLevel(int c, float a, float m)
        {
            cost = c;
            addValue = a;
            multiplyValue = m;
        }

        public int Cost => cost;
        public float AddValue => addValue;
        public float MultiplyValue => multiplyValue;
    }

    public enum UpgradeType
    {
        Armor = 0,
        Health = 1,
        Healing = 2,
        Damage = 3,
        Revive = 4,
    }

    public enum ValueType
    {
        AddValue = 0,
        SubValue = 0,
        MultiplyValue = 1,
    }
}
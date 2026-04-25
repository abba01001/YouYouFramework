using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Abilities Database", menuName = "October/Abilities/Database")]
    public class AbilitiesDatabase : ScriptableObject
    {
        [Header("技能容量设置")]
        [SerializeField] [LabelText("主动技能最大容量")] int activeAbilitiesCapacity = 5;
        [SerializeField] [LabelText("被动技能最大容量")] int passiveAbilitiesCapacity = 5;

        public int ActiveAbilitiesCapacity => activeAbilitiesCapacity;
        public int PassiveAbilitiesCapacity => passiveAbilitiesCapacity;

        [Header("技能权重倍率")]
        [SerializeField] [LabelText("主动技能初始等级权重倍率")] float firstLevelsActiveAbilityWeightMultiplier = 1.2f;
        [SerializeField] [LabelText("已获得技能权重倍率")] float aquiredAbilityWeightMultiplier = 1.2f;
        [SerializeField] [LabelText("同类技能较少时权重倍率")] float lessAbilitiesOfTypeWeightMultiplier = 1.2f;
        [SerializeField] [LabelText("可进化技能权重倍率")] float evolutionAbilityWeightMultiplier = 10f;
        [SerializeField] [LabelText("进化所需技能权重倍率")] float requiredForEvolutionWeightMultiplier = 1.2f;

        public float FirstLevelsActiveAbilityWeightMultiplier => firstLevelsActiveAbilityWeightMultiplier;
        public float AquiredAbilityWeightMultiplier => aquiredAbilityWeightMultiplier;
        public float LessAbilitiesOfTypeWeightMultiplier => lessAbilitiesOfTypeWeightMultiplier;
        public float EvolutionAbilityWeightMultiplier => evolutionAbilityWeightMultiplier;
        public float RequiredForEvolutionWeightMultiplier => requiredForEvolutionWeightMultiplier;

        [Header("技能列表")]
        [SerializeField] [LabelText("技能数据列表")] List<AbilityData> abilities;

        public int AbilitiesCount => abilities.Count;

        public AbilityData GetAbility(int index)
        {
            return abilities[index];
        }

        public AbilityData GetAbility(AbilityType type)
        {
            for(int i = 0; i < abilities.Count; i++)
            {
                AbilityData ability = abilities[i];
                if(ability.AbilityType == type) return ability;
            }
            return null;
        }
    }
}
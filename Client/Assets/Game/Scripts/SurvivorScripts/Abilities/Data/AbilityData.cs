using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector; // 确保你项目中安装了 Odin

namespace OctoberStudio.Abilities
{
    public abstract class AbilityData : ScriptableObject 
    {
        [ShowInInspector]
        [PropertyOrder(-1)]
        [LabelText("是否为初始武器")]
        public bool IsInitWeapon => isWeaponAbility && !isEvolution;
        
        [LabelText("解锁关卡")]
        [SerializeField] protected int unlockLv;
        public int UnlockLv => unlockLv;
        
        [LabelText("技能类型")]
        [SerializeField] protected AbilityType type;
        public AbilityType AbilityType => type;

        [LabelText("技能名称")]
        [SerializeField] string title;
        public string Title => title;

        [LabelText("技能描述")]
        [SerializeField] string description;
        public string Description => description;

        [LabelText("UI图标")]
        [SerializeField] Sprite icon;
        public Sprite Icon => icon;

        [LabelText("技能预制体")]
        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [LabelText("是否为主动技能")]
        [SerializeField] protected bool isActiveAbility;
        public bool IsActiveAbility => isActiveAbility;

        [LabelText("武器绑定")]
        [SerializeField] protected bool isWeaponAbility;
        public bool IsWeaponAbility => isWeaponAbility;

        [LabelText("保底技能 (无可选时出现)")]
        [SerializeField] protected bool isEndgameAbility;
        public bool IsEndgameAbility => isEndgameAbility;

        [LabelText("是否为进化/超武")]
        [SerializeField] bool isEvolution;
        public bool IsEvolution => isEvolution;
        
        [LabelText("进化需求清单")]
        [SerializeField] List<EvolutionRequirement> evolutionRequirements;
        public List<EvolutionRequirement> EvolutionRequirements => evolutionRequirements;

        
        public abstract AbilityLevel[] Levels { get; }
        public int LevelsCount => Levels.Length;

        public event UnityAction<int> onAbilityUpgraded;

        public void Upgrade(int level)
        {
            if(level < LevelsCount)
            {
                onAbilityUpgraded?.Invoke(level);
            }
        }

        public AbilityLevel GetLevel(int index)
        {
            return Levels[index];
        }
    }

    [System.Serializable]
    public abstract class AbilityLevel
    {
    }

    [System.Serializable]
    public class EvolutionRequirement
    {
        [LabelText("所需前置技能")]
        [SerializeField] AbilityType abilityType;
        public AbilityType AbilityType => abilityType;

        [LabelText("所需等级")]
        [SerializeField, Min(0)] int requiredAbilityLevel;
        public int RequiredAbilityLevel => requiredAbilityLevel;

        [LabelText("进化后移除前置")]
        [SerializeField] bool shouldRemoveAfterEvolution;
        public bool ShouldRemoveAfterEvolution => shouldRemoveAfterEvolution;
    }
}
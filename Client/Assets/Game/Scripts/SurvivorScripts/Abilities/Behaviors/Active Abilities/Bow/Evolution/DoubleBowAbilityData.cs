using UnityEngine;
using Sirenix.OdinInspector;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "双界分森弓数据", menuName = "October/Abilities/Evolution/双界分森弓")]
    public class DoubleBowAbilityData : GenericAbilityData<DoubleBowAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.DoubleBow;
            isWeaponAbility = true;
        }

        private void OnValidate()
        {
            type = AbilityType.DoubleBow;
            isWeaponAbility = true;
        }
    }

    [System.Serializable]
    public class DoubleBowAbilityLevel : AbilityLevel
    {
        [LabelText("攻击间隔 (冷却)")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [LabelText("投掷斧头个数")]
        [SerializeField] int projectilesCount;
        public int ProjectilesCount => projectilesCount;

        [LabelText("基础伤害倍率")]
        [SerializeField] float damage;
        public float Damage => damage;

        [LabelText("挥砍范围/大小")]
        [SerializeField] float slashSize;
        public float SlashSize => slashSize;

        [LabelText("发射弓箭距离")]
        [SerializeField] float distance;
        public float Distance => distance;
        
        [LabelText("挥砍间隔 (连斩延迟)")]
        [SerializeField] float timeBetweenSlashes;
        public float TimeBetweenSlashes => timeBetweenSlashes;
    }
}
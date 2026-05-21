using UnityEngine;
using Sirenix.OdinInspector;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "角木弓数据", menuName = "October/Abilities/Active/歧木角弓")]
    public class WoodenBowWeaponAbilityData : GenericAbilityData<WoodenBowWeaponAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.WoodenBow;
            isWeaponAbility = true;
        }

        private void OnValidate()
        {
            type = AbilityType.WoodenBow;
            isWeaponAbility = true;
        }
    }

    [System.Serializable]
    public class WoodenBowWeaponAbilityLevel : AbilityLevel
    {
        [LabelText("攻击间隔 (冷却)")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [LabelText("发射箭矢个数")]
        [SerializeField] int projectilesCount;
        public int ProjectilesCount => projectilesCount;

        [LabelText("基础伤害倍率")]
        [SerializeField] float damage;
        public float Damage => damage;

        [LabelText("弓箭大小")]
        [SerializeField] float slashSize;
        public float SlashSize => slashSize;

        [LabelText("发射弓箭距离")]
        [SerializeField] float distance;
        public float Distance => distance;
        
        [LabelText("发射弓箭间隔")]
        [SerializeField] float timeBetweenSlashes;
        public float TimeBetweenSlashes => timeBetweenSlashes;
    }
}
using UnityEngine;
using Sirenix.OdinInspector;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Wooden Axe Data", menuName = "October/Abilities/Active/Wooden Axe")]
    public class WoodenAxeWeaponAbilityData : GenericAbilityData<WoodenAxeWeaponAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.WoodenAxe;
            isWeaponAbility = true;
        }

        private void OnValidate()
        {
            type = AbilityType.WoodenAxe;
            isWeaponAbility = true;
        }
    }

    [System.Serializable]
    public class WoodenAxeWeaponAbilityLevel : AbilityLevel
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

        [LabelText("挥砍间隔 (连斩延迟)")]
        [SerializeField] float timeBetweenSlashes;
        public float TimeBetweenSlashes => timeBetweenSlashes;
    }
}
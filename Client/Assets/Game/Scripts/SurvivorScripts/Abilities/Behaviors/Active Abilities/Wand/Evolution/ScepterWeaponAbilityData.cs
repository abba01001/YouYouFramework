using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Scepter Data", menuName = "October/Abilities/Evolution/Scepter")]
    public class ScepterWeaponAbilityData : GenericAbilityData<ScepterWeaponAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.AncientScepter;
            isWeaponAbility = true;
        }

        private void OnValidate()
        {
            type = AbilityType.AncientScepter;
            isWeaponAbility = true;
        }
    }

    [System.Serializable]
    public class ScepterWeaponAbilityLevel : AbilityLevel
    {
        [Tooltip("两次攻击之间的时间间隔")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("弹射物的飞行速度")]
        [SerializeField] float projectileSpeed;
        public float ProjectileSpeed => projectileSpeed;

        [Tooltip("弹射物伤害计算公式：玩家伤害 * 基础伤害")]
        [SerializeField] float damage;
        public float Damage => damage;

        [Tooltip("弹射物的大小尺寸")]
        [SerializeField] float projectileSize;
        public float ProjectileSize => projectileSize;

        [Tooltip("弹射物在场景中存活的时间")]
        [SerializeField] float projectileLifetime;
        public float ProjectileLifetime => projectileLifetime;
    }
}
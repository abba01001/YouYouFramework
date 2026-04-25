using OctoberStudio.Pool;
using OctoberStudio.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class DestructiveAxeAbilityBehavior : AbilityBehavior<DestructiveAxeAbilityData, DestructiveAxeAbilityLevel>
    {
        public static readonly int STEEL_SWORD_ATTACK_HASH = "Steel Sword Attack".GetHashCode();

        [SerializeField] GameObject axePrefab;
        public GameObject AxePrefab => axePrefab;

        private PoolComponent<AxeProjectileBehavior> axeProjectilesPool;
        private List<AxeProjectileBehavior> axeProjectiles = new List<AxeProjectileBehavior>();

        Coroutine abilityCoroutine;

        private float AbilityCooldown => AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier;

        private void Awake()
        {
            axeProjectilesPool = new PoolComponent<AxeProjectileBehavior>("Wooden Axe", AxePrefab, 50);
        }

        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            if (abilityCoroutine != null) Disable();

            abilityCoroutine = StartCoroutine(AbilityCoroutine());
        }

        private IEnumerator AbilityCoroutine()
        {
            var lastTimeSpawned = Time.time - AbilityCooldown;

            while (true)
            {
                for(int i = 0; i < AbilityLevel.ProjectilesCount; i++)
                {
                    var axe = axeProjectilesPool.GetEntity();
                    axe.DamageMultiplier = AbilityLevel.Damage;
                    axe.KickBack = false;
                    axe.Size = 1;
                    axe.Init();
                    axe.StartThrowAxe(i,AbilityLevel);
                    axe.onFinished += OnProjectileFinished;
                    axeProjectiles.Add(axe);

                    GameController.AudioManager.PlaySound(STEEL_SWORD_ATTACK_HASH);

                    yield return new WaitForSeconds(AbilityLevel.TimeBetweenSlashes * PlayerBehavior.Player.CooldownMultiplier);
                }

                yield return new WaitForSeconds(AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier - AbilityLevel.TimeBetweenSlashes * PlayerBehavior.Player.CooldownMultiplier * AbilityLevel.ProjectilesCount);
            }
        }
        
        private void OnProjectileFinished(AxeProjectileBehavior projectile)
        {
            projectile.onFinished -= OnProjectileFinished;
            axeProjectiles.Remove(projectile);
        }

        private void Disable()
        {
            for (int i = 0; i < axeProjectiles.Count; i++)
            {
                axeProjectiles[i].Disable();
            }
            axeProjectiles.Clear();
            StopCoroutine(abilityCoroutine);
        }

        public override void Clear()
        {
            Disable();
            axeProjectilesPool.Destroy();
            base.Clear();
        }
    }
}
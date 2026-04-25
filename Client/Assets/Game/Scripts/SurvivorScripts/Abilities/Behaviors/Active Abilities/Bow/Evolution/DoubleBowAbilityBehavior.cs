using OctoberStudio.Easing;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class DoubleBowAbilityBehavior : AbilityBehavior<DoubleBowAbilityData, DoubleBowAbilityLevel>
    {
        public static readonly int STEEL_SWORD_ATTACK_HASH = "Steel Sword Attack".GetHashCode();

        [SerializeField] GameObject bowPrefab;
        public GameObject BowPrefab => bowPrefab;

        private PoolComponent<BowProjectileBehavior> bowProjectilesPool;
        private List<BowProjectileBehavior> bowProjectiles = new List<BowProjectileBehavior>();

        Coroutine abilityCoroutine;

        private float AbilityCooldown => AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier;

        private void Awake()
        {
            bowProjectilesPool = new PoolComponent<BowProjectileBehavior>("Wooden Bow", BowPrefab, 50);
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
                    for (int j = 1; j <= 2; j++)
                    {
                        var bow = bowProjectilesPool.GetEntity();
                        bow.DamageMultiplier = AbilityLevel.Damage;
                        bow.KickBack = false;
                        bow.Size = AbilityLevel.SlashSize;
                        bow.Init();
                        bow.StartThrowAxe(i,AbilityLevel,j == 1);
                        bow.onFinished += OnProjectileFinished;
                        bowProjectiles.Add(bow);

                        GameController.AudioManager.PlaySound(STEEL_SWORD_ATTACK_HASH);
                    }
                    yield return new WaitForSeconds(AbilityLevel.TimeBetweenSlashes * PlayerBehavior.Player.CooldownMultiplier);
                }

                yield return new WaitForSeconds(AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier - AbilityLevel.TimeBetweenSlashes * PlayerBehavior.Player.CooldownMultiplier * AbilityLevel.ProjectilesCount);
            }
        }

        
        
        private void OnProjectileFinished(BowProjectileBehavior projectile)
        {
            projectile.onFinished -= OnProjectileFinished;
            bowProjectiles.Remove(projectile);
        }

        private void Disable()
        {
            for (int i = 0; i < bowProjectiles.Count; i++)
            {
                bowProjectiles[i].Disable();
            }
            bowProjectiles.Clear();
            StopCoroutine(abilityCoroutine);
        }

        public override void Clear()
        {
            Disable();

            base.Clear();
        }
    }
}
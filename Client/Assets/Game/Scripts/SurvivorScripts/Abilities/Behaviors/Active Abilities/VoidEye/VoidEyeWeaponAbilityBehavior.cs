using OctoberStudio.Easing;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class VoidEyeWeaponAbilityBehavior : AbilityBehavior<VoidEyeWeaponAbilityData, VoidEyeWeaponAbilityLevel>
    {
        public static readonly int STEEL_SWORD_ATTACK_HASH = "Steel Sword Attack".GetHashCode();

        [SerializeField] GameObject voidEyePrefab;
        public GameObject VoidEyePrefab => voidEyePrefab;

        private PoolComponent<VoidEyeProjectileBehavior> voidEyeProjectilesPool;
        private List<VoidEyeProjectileBehavior> voidEyeProjectiles = new List<VoidEyeProjectileBehavior>();

        Coroutine abilityCoroutine;

        private float AbilityCooldown => AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier;

        private void Awake()
        {
            voidEyeProjectilesPool = new PoolComponent<VoidEyeProjectileBehavior>("Void Eye", VoidEyePrefab, 50);
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
                    var voidEye = voidEyeProjectilesPool.GetEntity();
                    voidEye.DamageMultiplier = AbilityLevel.Damage;
                    voidEye.KickBack = false;
                    voidEye.Size = AbilityLevel.SlashSize;
                    voidEye.Init();
                    voidEye.StartThrowAxe(i,AbilityLevel);
                    voidEye.onFinished += OnProjectileFinished;
                    voidEyeProjectiles.Add(voidEye);

                    GameController.AudioManager.PlaySound(STEEL_SWORD_ATTACK_HASH);

                    yield return new WaitForSeconds(AbilityLevel.TimeBetweenSlashes * PlayerBehavior.Player.CooldownMultiplier);
                }

                yield return new WaitForSeconds(AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier - AbilityLevel.TimeBetweenSlashes * PlayerBehavior.Player.CooldownMultiplier * AbilityLevel.ProjectilesCount);
            }
        }

        
        
        private void OnProjectileFinished(VoidEyeProjectileBehavior projectile)
        {
            projectile.onFinished -= OnProjectileFinished;
            voidEyeProjectiles.Remove(projectile);
        }

        private void Disable()
        {
            for (int i = 0; i < voidEyeProjectiles.Count; i++)
            {
                voidEyeProjectiles[i].Disable();
            }
            voidEyeProjectiles.Clear();
            StopCoroutine(abilityCoroutine);
        }

        public override void Clear()
        {
            Disable();

            base.Clear();
        }
    }
}
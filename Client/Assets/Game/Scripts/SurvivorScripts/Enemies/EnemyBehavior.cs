using OctoberStudio.Easing;
using OctoberStudio.Enemy;
using OctoberStudio.Extensions;
using OctoberStudio.Timeline;
using System.Collections.Generic;
using GameScripts;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class EnemyBehavior : MonoBehaviour
    {
        [LabelText("Model类型")] [SerializeField] private EnemyType enemyType = EnemyType.Null;
        
        // 保持原始 Shader ID 拼写 (根据你提供的代码，消融是 _Disolve)
        protected static readonly int _Overlay = Shader.PropertyToID("_Overlay");
        protected static readonly int _Disolve = Shader.PropertyToID("_Disolve");
        private static readonly int HIT_HASH = "Hit".GetHashCode();

        public float Speed { get; protected set; }
        protected float configSpeed;
        protected float configDamage;
        protected float configHp;

        [LabelText("能否被击退")][SerializeField] bool canBeKickedBack = true;
        [LabelText("淡色出现")][SerializeField] bool shouldFadeIn;
        [SerializeField, LabelText("翻转冷却")] private float flipThreshold = 0.1f;

        [Header("References")]
        [SerializeField] Rigidbody2D rb;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] DissolveSettings dissolveSettings;
        [SerializeField] SpriteRenderer shadowSprite;
        [SerializeField] Collider2D enemyCollider;

        public Vector2 Center => enemyCollider.bounds.center;

        [Header("Hit")]
        [SerializeField] float hitScaleAmount = 0.2f;
        [SerializeField] Color hitColor = Color.white;

        public EnemyData Data { get; private set; }
        public WaveOverride WaveOverride { get; protected set; }

        public bool IsVisible => spriteRenderer.isVisible;
        public bool IsAlive => HP > 0;
        public bool IsInvulnerable { get; protected set; }
        
        public float HP { get; private set; }
        public float MaxHP { get; private set; }
        public bool ShouldSpawnChestOnDeath { get; set; }

        // --- 优化缓存变量 ---
        private Transform playerTransform;
        private float finalSpeedMultiplier = 1f;
        private bool isEffectsDirty = true; 
        private float shadowAlpha;
        private Material sharedMaterial;
        private Material effectsMaterial;

        private Dictionary<EffectType, List<Effect>> appliedEffects = new Dictionary<EffectType, List<Effect>>();

        protected bool IsMoving { get; set; }
        public bool IsMovingToCustomPoint { get; protected set; }
        public Vector2 CustomPoint { get; protected set; }

        public float LastTimeDamagedPlayer { get; set; }
        public BehaviorType CurAnimType { get; private set; } = BehaviorType.Default;

        public event UnityAction<EnemyBehavior> onEnemyDied;
        public event UnityAction<float, float> onHealthChanged;

        private float lastTimeSwitchedDirection = 0;
        private IEasingCoroutine fallBackCoroutine;
        private IEasingCoroutine damageCoroutine;
        protected IEasingCoroutine scaleCoroutine;
        private IEasingCoroutine fadeInCoroutine;

        private float damageTextValue;
        private float lastTimeDamageText;
        private static int lastFrameHitSound;
        private float lastTimeHitSound;

        protected virtual void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
            sharedMaterial = spriteRenderer.sharedMaterial;
            // 保持线上原本的实例化材质逻辑，确保 Shader 表现一致
            effectsMaterial = Instantiate(sharedMaterial);
            shadowAlpha = shadowSprite.color.a;
        }

        public void SetConfigBaseData()
        {
            var cfg = GameEntry.DataTable.Sys_ModelDBModel.GetEntity(enemyType);
            configHp = cfg.Hp;
            configSpeed = cfg.Speed;
            configDamage = cfg.Damage;
        }
        
        public void SetData(EnemyData data)
        {
            Data = data;
            ResetEnemyAnimType();
        }
        
        private void OnValidate()
        {
            if (enemyType == EnemyType.Null)
                Debug.LogError($"<color=red>【配置错误】</color> 怪物 <b>{gameObject.name}</b> 未指定 EnemyType！", gameObject);
        }

        public virtual void Play()
        {
            if (PlayerBehavior.Player != null) playerTransform = PlayerBehavior.Player.transform;

            MaxHP = StageController.Stage.EnemyHP * configHp;
            Speed = configSpeed;
            if (WaveOverride != null)
            {
                MaxHP = WaveOverride.ApplyHPOverride(MaxHP);
                Speed = WaveOverride.ApplySpeedOverride(Speed);
            }
            
            HP = MaxHP;
            IsMoving = true;
            isEffectsDirty = true; // 强制重算初始速度

            // 重置视觉与碰撞状态
            enemyCollider.enabled = true;
            shadowSprite.SetAlpha(shadowAlpha);
            spriteRenderer.material = sharedMaterial; 
            
            if (shouldFadeIn)
            {
                spriteRenderer.SetAlpha(0);
                fadeInCoroutine = spriteRenderer.DoAlpha(1, 0.2f);
            } 
        }

        protected virtual void Update()
        {
            if (!IsAlive || !IsMoving || playerTransform == null || CheckIsDoingSpecialBehavoir()) return;

            // 优化1：脏标记处理速度倍率
            if (isEffectsDirty) UpdateSpeedMultiplier();

            Vector3 target = IsMovingToCustomPoint ? (Vector3)CustomPoint : playerTransform.position;
            Vector3 direction = (target - transform.position).normalized;
            
            transform.position += direction * (Time.deltaTime * Speed * finalSpeedMultiplier);
            
            // 优化2：带冷却的翻转逻辑
            HandleSpriteFlip(direction.x);
        }

        private void UpdateSpeedMultiplier()
        {
            finalSpeedMultiplier = 1f;
            if (appliedEffects.TryGetValue(EffectType.Speed, out var speedEffects))
            {
                for (int i = 0; i < speedEffects.Count; i++)
                    finalSpeedMultiplier *= speedEffects[i].Modifier;
            }
            isEffectsDirty = false;
        }

        private void HandleSpriteFlip(float directionX)
        {
            if (scaleCoroutine.ExistsAndActive()) return;

            float currentScaleX = transform.localScale.x;
            if ((directionX > 0 && currentScaleX < 0) || (directionX < 0 && currentScaleX > 0))
            {
                if (Time.unscaledTime - lastTimeSwitchedDirection > flipThreshold)
                {
                    Vector3 scale = transform.localScale;
                    scale.x *= -1;
                    transform.localScale = scale;
                    lastTimeSwitchedDirection = Time.unscaledTime;
                }
            }
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive || IsInvulnerable) return;

            HP -= damage;
            onHealthChanged?.Invoke(HP, MaxHP);

            // 优化3：合并伤害文字
            ProcessDamageText(damage);
            // 优化4：受击音效限频
            ProcessHitSound();

            if (HP <= 0)
            {
                Die(true);
            }
            else
            {
                ApplyHitVisuals();
            }
        }

        private void ApplyHitVisuals()
        {
            if (!damageCoroutine.ExistsAndActive()) 
            {
                FlashHit(true);
            }

            if (!scaleCoroutine.ExistsAndActive())
            {
                float x = transform.localScale.x;
                scaleCoroutine = transform.DoLocalScale(new Vector3(x * (1 - hitScaleAmount), (1 + hitScaleAmount), 1), 0.07f)
                    .SetEasing(EasingType.SineOut)
                    .SetOnFinish(() => {
                        if (this != null) transform.DoLocalScale(new Vector3(x, 1, 1), 0.07f).SetEasing(EasingType.SineInOut);
                    });
            }
        }

        private void ProcessDamageText(float damage)
        {
            damageTextValue += damage;
            if (Time.unscaledTime - lastTimeDamageText > 0.2f && damageTextValue >= 1)
            {
                var text = Mathf.RoundToInt(damageTextValue).ToString();
                // 增加少量偏移防止重叠
                Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.value * 0.1f);
                StageController.WorldSpaceTextManager.SpawnText(spawnPos, text);
                
                damageTextValue = 0;
                lastTimeDamageText = Time.unscaledTime;
            }
        }

        private void ProcessHitSound()
        {
            // 同一帧多个怪物受击只播一次，且0.2秒内不重复播
            if (Time.frameCount != lastFrameHitSound && Time.unscaledTime - lastTimeHitSound > 0.2f)
            {
                GameController.AudioManager.PlaySound(HIT_HASH);
                lastFrameHitSound = Time.frameCount;
                lastTimeHitSound = Time.unscaledTime;
            }
        }

        private void FlashHit(bool resetMaterial, UnityAction onFinish = null)
        {
            spriteRenderer.material = effectsMaterial;
            Color transparent = hitColor;
            transparent.a = 0;

            effectsMaterial.SetColor(_Overlay, transparent);
            damageCoroutine = effectsMaterial.DoColor(_Overlay, hitColor, 0.05f).SetOnFinish(() =>
            {
                damageCoroutine = effectsMaterial.DoColor(_Overlay, transparent, 0.05f).SetOnFinish(() =>
                {
                    // 优化5：安全检查
                    if (this != null)
                    {
                        if (resetMaterial) spriteRenderer.material = sharedMaterial;
                        onFinish?.Invoke();
                    }
                });
            });
        }

        protected virtual void Die(bool flash)
        {
            enemyCollider.enabled = false;
            damageCoroutine.StopIfExists();
            fallBackCoroutine.StopIfExists();
            fadeInCoroutine.StopIfExists();
            onEnemyDied?.Invoke(this);

            rb.simulated = true;
            spriteRenderer.material = effectsMaterial;

            if (flash)
            {
                FlashHit(false, () => {
                    effectsMaterial.SetColor(_Overlay, Color.clear);
                    effectsMaterial.DoColor(_Overlay, dissolveSettings.DissolveColor, dissolveSettings.Duration - 0.1f);
                });
            }
            else
            {
                effectsMaterial.SetColor(_Overlay, Color.clear);
                effectsMaterial.DoColor(_Overlay, dissolveSettings.DissolveColor, dissolveSettings.Duration);
            }

            effectsMaterial.SetFloat(_Disolve, 0);
            effectsMaterial.DoFloat(_Disolve, 1, dissolveSettings.Duration + 0.02f)
                .SetEasingCurve(dissolveSettings.DissolveCurve).SetOnFinish(() =>
                {
                    if (this != null)
                    {
                        effectsMaterial.SetColor(_Overlay, Color.clear);
                        effectsMaterial.SetFloat(_Disolve, 0);
                        gameObject.SetActive(false);
                        spriteRenderer.material = sharedMaterial;
                    }
                });

            shadowSprite.DoAlpha(0, dissolveSettings.Duration);
            appliedEffects.Clear();
            WaveOverride = null;
        }

        public void KickBack(Vector3 position)
        {
            var direction = (transform.position - position).normalized;
            rb.simulated = false;
            fallBackCoroutine.StopIfExists();
            fallBackCoroutine = transform.DoPosition(transform.position + direction * 0.6f, 0.15f)
                .SetEasing(EasingType.ExpoOut).SetOnFinish(() => {
                    if (this != null) rb.simulated = true;
                });
        }

        public void AddEffect(Effect effect)
        {
            if (!appliedEffects.ContainsKey(effect.EffectType))
                appliedEffects.Add(effect.EffectType, new List<Effect>());

            if (!appliedEffects[effect.EffectType].Contains(effect))
            {
                appliedEffects[effect.EffectType].Add(effect);
                isEffectsDirty = true; 
            }
        }

        public void RemoveEffect(Effect effect)
        {
            if (!appliedEffects.ContainsKey(effect.EffectType)) return;
            if (appliedEffects[effect.EffectType].Remove(effect))
            {
                isEffectsDirty = true;
            }
        }

        public void AddEffects(List<Effect> effects)
        {
            if (effects == null) return;
            for (int i = 0; i < effects.Count; i++) AddEffect(effects[i]);
        }

        public void ResetToPlayerDirection()
        {
            if (playerTransform == null) return;
            Vector3 target = IsMovingToCustomPoint ? CustomPoint : playerTransform.position;
            Vector3 direction = (target - transform.position).normalized;
            var scale = transform.localScale;
            if ((direction.x > 0 && scale.x < 0) || (direction.x < 0 && scale.x > 0))
            {
                scale.x *= -1;
                transform.localScale = scale;
            }
        }

        public void SetWaveOverride(WaveOverride waveOverride) => WaveOverride = waveOverride;

        public void SetEnemyAnimType(BehaviorType behaviorType)
        {
            CurAnimType = behaviorType;
            if (CurAnimType != BehaviorType.Default) ResetToPlayerDirection();
        }

        public void ResetEnemyAnimType() => CurAnimType = BehaviorType.Default;

        public bool CheckIsDoingSpecialBehavoir() => CurAnimType != BehaviorType.Default;

        public void Kill() { HP = 0; Die(false); }

        public float GetDamage()
        {
            var damage = this.configDamage;
            if (WaveOverride != null) damage = WaveOverride.ApplyDamageOverride(damage);
            var baseDamage = StageController.Stage.EnemyDamage * damage;

            if (appliedEffects.TryGetValue(EffectType.Damage, out var damageEffects))
            {
                for (int i = 0; i < damageEffects.Count; i++)
                    baseDamage *= damageEffects[i].Modifier;
            }
            return baseDamage;
        }

        public List<EnemyDropData> GetDropData() => WaveOverride != null ? WaveOverride.ApplyDropOverride(Data.EnemyDrop) : Data.EnemyDrop;

        private void OnTriggerEnter2D(Collider2D other)
        {
            ProjectileBehavior projectile = other.GetComponent<ProjectileBehavior>();
            if (projectile != null)
            {
                TakeDamage(PlayerBehavior.Player.GetDamageValue() * projectile.DamageMultiplier);
                if (HP > 0)
                {
                    if (projectile.KickBack && canBeKickedBack) KickBack(PlayerBehavior.CenterPosition);
                    if (projectile.Effects != null) AddEffects(projectile.Effects);
                }
            }
        }
    }

    public enum BehaviorType { Default, StopBehavior, RushBehavior, FlashBehavior, CircleBehavior }
}
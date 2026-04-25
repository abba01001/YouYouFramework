using DG.Tweening;
using Main;
using OctoberStudio.Easing;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Abilities
{
    public class BowProjectileBehavior : ProjectileBehavior
    {
        [SerializeField] Collider2D slashCollider;
        [SerializeField] float duration;

        public float Size { get; set; }

        public UnityAction<BowProjectileBehavior> onFinished;

        private IEasingCoroutine waitingCoroutine;
        private IEasingCoroutine colliderCoroutine;
        private IEasingCoroutine jumpCoroutine;

        public override void Init()
        {
            base.Init();
            transform.localScale = Vector3.one * Size * PlayerBehavior.Player.SizeMultiplier;
            slashCollider.enabled = true;
        }

        public void StartThrowAxe(int index,WoodenBowWeaponAbilityLevel AbilityLevel)
        {
            float offset = 0;
            if (AbilityLevel.ProjectilesCount > 1)
            {
                offset = (index * (spreadAngle / (AbilityLevel.ProjectilesCount - 1))) - (spreadAngle / 2f);
            }
            ThrowArrow(AbilityLevel.Distance,offset);
            
            colliderCoroutine = EasingManager.DoAfter(duration, () => slashCollider.enabled = false);
            waitingCoroutine = EasingManager.DoAfter(duration + 0.3f, () => {
                onFinished?.Invoke(this);
                Disable();
            });
        }
        
        private float spreadAngle = 50;
        public void ThrowArrow(float distance,float angleOffset,int faceValue = 1)
        {
            Vector2 startPos = PlayerBehavior.CenterPosition;
    
            // 1. 确保 LookDirection 是单位向量
            Vector2 baseDir = faceValue * PlayerBehavior.Player.LookDirection.normalized;
    
            // 2. 计算最终发射方向
            Vector2 shootDir = Quaternion.Euler(0, 0, angleOffset) * baseDir;
    
            // 3. 计算目标落点
            Vector2 targetPos = startPos + shootDir * distance;

            transform.position = startPos;
    
            // 4. 设置图片/物体的旋转，使其指向飞行方向
            // transform.right 适用于 Sprite 枪口/箭头朝向右侧的资源
            transform.right = shootDir; 
            
            jumpCoroutine = EasingManager.DoJump(startPos, targetPos, 0, duration, (pos) => {
                transform.position = pos;
            }).SetEasing(EasingType.ExpoOut);
        }

        public void StartThrowAxe(int index,DoubleBowAbilityLevel AbilityLevel,bool isLeft)
        {
            float offset = 0;
            if (AbilityLevel.ProjectilesCount > 1)
            {
                offset = (index * (spreadAngle / (AbilityLevel.ProjectilesCount - 1))) - (spreadAngle / 2f);
            }
            ThrowArrow(AbilityLevel.Distance,offset,isLeft ? 1 : -1);
            
            colliderCoroutine = EasingManager.DoAfter(duration, () => slashCollider.enabled = false);
            waitingCoroutine = EasingManager.DoAfter(duration + 0.3f, () => {
                onFinished?.Invoke(this);
                Disable();
            });
        }
        
        public void Disable()
        {
            waitingCoroutine.StopIfExists();
            colliderCoroutine.StopIfExists();
            jumpCoroutine.StopIfExists();
            gameObject.SetActive(false);
            slashCollider.enabled = true;
        }
    }
}
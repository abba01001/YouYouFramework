using DG.Tweening;
using Main;
using OctoberStudio.Easing;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Abilities
{
    public class AxeProjectileBehavior : ProjectileBehavior
    {
        [SerializeField] Collider2D axeCollider;
        [SerializeField] float duration;

        public float Size { get; set; }

        public UnityAction<AxeProjectileBehavior> onFinished;

        private IEasingCoroutine waitingCoroutine;
        private IEasingCoroutine colliderCoroutine;
        private IEasingCoroutine rotateCoroutine;
        private IEasingCoroutine sizeCoroutine;
        private IEasingCoroutine jumpCoroutine;

        public override void Init()
        {
            base.Init();
            transform.localScale = Vector3.one * Size * PlayerBehavior.Player.SizeMultiplier;
            axeCollider.enabled = true;
        }

        public void StartThrowAxe(int index,WoodenAxeWeaponAbilityLevel AbilityLevel)
        {
            float offset = 0;
            if (AbilityLevel.ProjectilesCount > 1)
            {
                offset = (index * (spreadAngle / (AbilityLevel.ProjectilesCount - 1))) - (spreadAngle / 2f);
            }
            ThrowAxe(offset);
            
            colliderCoroutine = EasingManager.DoAfter(duration, () => axeCollider.enabled = false);
            waitingCoroutine = EasingManager.DoAfter(duration + 0.3f, () => {
                onFinished?.Invoke(this);
                Disable();
            });
        }
        
        public void StartThrowAxe(int index,DestructiveAxeAbilityLevel AbilityLevel)
        {
            float offset = 0;
            if (AbilityLevel.ProjectilesCount > 1)
            {
                offset = (index * (spreadAngle / (AbilityLevel.ProjectilesCount - 1))) - (spreadAngle / 2f);
            }

            ChangeSize(AbilityLevel.SlashSize);
            ThrowDestructiveAxe(offset);
            
            colliderCoroutine = EasingManager.DoAfter(duration, () => axeCollider.enabled = false);
            waitingCoroutine = EasingManager.DoAfter(duration + 0.3f, () => {
                onFinished?.Invoke(this);
                Disable();
            });
        }

        public void ChangeSize(float finalSize)
        {
            sizeCoroutine = EasingManager.DoFloat(Size, finalSize, duration, (s) =>
            {
                transform.localScale = s * Vector3.one;
            });
        }
        
        private float spreadAngle = 45;
        public void ThrowAxe(float angleOffset)
        {
            float distance = 2f;
            float jumpHeight = 1.2f;

            Vector2 startPos = PlayerBehavior.CenterPosition;
            Vector2 baseDir = PlayerBehavior.Player.LookDirection;
            Vector2 shootDir = Quaternion.Euler(0, 0, angleOffset) * baseDir;
            Vector2 targetPos = startPos + shootDir * distance;

            transform.position = startPos;
            jumpCoroutine = EasingManager.DoJump(startPos, targetPos, jumpHeight, duration, (pos) => {
                transform.position = pos;
            });
            
            transform.right = shootDir; 
            
            float startRot = transform.eulerAngles.z;
            rotateCoroutine = EasingManager.DoRotate(startRot, startRot - 720f, duration, (rot) => {
                transform.rotation = Quaternion.Euler(0, 0, rot);
            });
        }

        public void ThrowDestructiveAxe(float angleOffset)
        {
            float distance = 3f;
            float jumpHeight = 0f;

            Vector2 startPos = PlayerBehavior.CenterPosition;
            Vector2 baseDir = PlayerBehavior.Player.LookDirection;
            Vector2 shootDir = Quaternion.Euler(0, 0, angleOffset) * baseDir;
            Vector2 targetPos = startPos + shootDir * distance;

            transform.position = startPos;
            jumpCoroutine = EasingManager.DoJump(startPos, targetPos, jumpHeight, duration, (pos) => {
                transform.position = pos;
            });
            
            transform.right = shootDir; 
            
            float startRot = transform.eulerAngles.z;
            rotateCoroutine = EasingManager.DoRotate(startRot, startRot - 720f, duration, (rot) => {
                transform.rotation = Quaternion.Euler(0, 0, rot);
            });
        }

        
        public void Disable()
        {
            waitingCoroutine.StopIfExists();
            colliderCoroutine.StopIfExists();
            rotateCoroutine.StopIfExists();
            jumpCoroutine.StopIfExists();
            sizeCoroutine.StopIfExists();
            gameObject.SetActive(false);
            axeCollider.enabled = true;
        }
    }
}
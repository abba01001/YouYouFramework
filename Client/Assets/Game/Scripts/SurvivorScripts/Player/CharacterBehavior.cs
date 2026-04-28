using System;
using OctoberStudio.Easing;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace OctoberStudio
{
    public class CharacterBehavior : MonoBehaviour, ICharacterBehavior
    {
        protected static readonly int DEFEAT_TRIGGER = Animator.StringToHash("Defeat");
        protected static readonly int REVIVE_TRIGGER = Animator.StringToHash("Revive");
        protected static readonly int SPEED_FLOAT = Animator.StringToHash("Speed");

        protected static readonly int _Overlay = Shader.PropertyToID("_Overlay");

        [SerializeField] protected SkeletonAnimation _skeletonAnimation;
        [SerializeField] protected SpriteRenderer playerSpriteRenderer;
        [SerializeField] protected Animator animator;
        [SerializeField] protected Color hitColor;
        [SerializeField] protected Transform centerTransform;
        public Transform CenterTransform => centerTransform;

        public Transform Transform => transform;

        protected IEasingCoroutine damageCoroutine;
        private string _currentAnim;
        private bool isDead = false;
        public virtual void SetSpeed(float speed)
        {
            if (isDead) return;
            animator.SetFloat(SPEED_FLOAT, speed);
            string targetAnim = speed > 0.02f ? "Run_Gear" : "Idle";
            if (_currentAnim != targetAnim)
            {
                _skeletonAnimation.AnimationState.SetAnimation(0, targetAnim, true);
                _currentAnim = targetAnim;
            }
        }

        public void PlayDieAnim()
        {
            isDead = true;
            _skeletonAnimation.AnimationState.SetAnimation(0, "Die", false);
            _skeletonAnimation.UnscaledTime = true;
        }

        public virtual void SetLocalScale(Vector3 scale)
        {
            transform.localScale = scale;
        }

        public virtual void PlayReviveAnimation()
        {
            animator.SetTrigger(REVIVE_TRIGGER);
        }

        public virtual void PlayDefeatAnimation()
        {
            animator.SetTrigger(DEFEAT_TRIGGER);
        }

        public virtual void SetSortingOrder(int order) 
        {
            playerSpriteRenderer.sortingOrder = order;
            transform.GetComponentInChildren<SortingGroup>().sortingOrder = order;
        }

        public virtual void FlashHit(UnityAction onFinish = null)
        {
            if (damageCoroutine.ExistsAndActive()) return;

            var transparentColor = hitColor;
            transparentColor.a = 0;

            playerSpriteRenderer.material.SetColor(_Overlay, transparentColor);

            damageCoroutine = playerSpriteRenderer.material.DoColor(_Overlay, hitColor, 0.05f).SetOnFinish(() =>
            {
                damageCoroutine = playerSpriteRenderer.material.DoColor(_Overlay, transparentColor, 0.05f).SetOnFinish(onFinish);
            });
        }
    }
}
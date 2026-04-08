using System;
using UnityEngine;

namespace Watermelon
{
    public class AnimBehavior : MonoBehaviour
    {
        private Animator _animator;
        private void Awake()
        {
            _animator = transform.GetComponent<Animator>();
        }

        private void OnEnable()
        {
            GameEntry.Event.AddEventListener(Constants.EventName.ConsumeMatEvent,OnConsumeMatEvent);
        }

        private void OnConsumeMatEvent(object userdata)
        {
            Recipe recipe = userdata as Recipe;
            Debugger.LogError($"OnConsumeMatEvent==>{recipe.ResultResourceType}");
            if (recipe.ResultResourceType == CurrencyType.Egg)
            {
                if(_animator) _animator.SetTrigger("Play");
            }
        }

        private void OnDisable()
        {
            GameEntry.Event.RemoveEventListener(Constants.EventName.ConsumeMatEvent,OnConsumeMatEvent);
        }
    }
}

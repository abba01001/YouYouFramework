using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class FloatingTextCase
    {
        [SerializeField] string name;

        public string Name
        {
            get => name;
            set => name = value;
        }

        FloatingTextBaseBehavior floatingTextBehavior;
        public FloatingTextBaseBehavior FloatingTextBehavior
        {
            get => floatingTextBehavior;
            set => floatingTextBehavior = value;
        }

        private Pool floatingTextPool;
        public Pool FloatingTextPool => floatingTextPool;

        public void Init()
        {
            floatingTextPool = new Pool(floatingTextBehavior.gameObject);
        }
    }
}
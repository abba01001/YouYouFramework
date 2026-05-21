using UnityEngine;

namespace GameScripts
{
    [ExecuteAlways]
    public class ChaosLaserDrive : MonoBehaviour
    {
        private LineRenderer lr;
        private Material mat;

        [Header("流动速度")]
        public float scrollSpeed = 4f; 

        [Header("宽度控制")]
        public float baseWidth = 0.6f;
        public float pulseAmount = 0.15f;
        public float pulseSpeed = 12f;

        void Start()
        {
            lr = GetComponent<LineRenderer>();
            if (lr != null) mat = lr.material;
        }

        void Update()
        {
            if (mat == null || lr == null) return;

            float time = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;

            // 1. 让噪声贴图滚起来 (产生能量流动感)
            // 注意：Particles/Standard Unlit 的贴图属性通常是 "_MainTex"
            float offset = time * scrollSpeed;
            mat.SetTextureOffset("_MainTex", new Vector2(-offset, 0));

            // 2. 宽度呼吸 (产生不稳定感)
            // 这种高频小幅抖动对捕捉视觉焦点非常有帮助
            float wave = Mathf.Sin(time * pulseSpeed);
            lr.widthMultiplier = baseWidth + (wave * pulseAmount);
        }
    }
}
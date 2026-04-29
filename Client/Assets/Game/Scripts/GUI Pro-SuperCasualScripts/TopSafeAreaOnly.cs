using UnityEngine;
using Main; // 对应你 AndroidHelper 的命名空间

namespace GameScripts
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaTopAdapter : MonoBehaviour
    {
        private RectTransform _rt;

        void Awake()
        {
            _rt = GetComponent<RectTransform>();
            AutoMatchNotch();
        }

        [ContextMenu("Manual Apply")] // 方便你在编辑器里右键测试
        public void AutoMatchNotch()
        {
            int pxHeight = 0;

#if UNITY_EDITOR
            // 在模拟器里，直接利用 Unity 的 safeArea 计算顶部高度
            // yMax 是安全区域的顶部像素位置，Screen.height - yMax 就是刘海高度
            pxHeight = Mathf.RoundToInt(Screen.height - Screen.safeArea.yMax);
#else
            // 在安卓真机上，调用你写的原生工具类
            pxHeight = Main.AndroidHelper.GetNotchHeight();
#endif
            Debug.Log($"[SafeArea] 顶部像素高度: {pxHeight}, 屏幕高度: {Screen.height}");
            if (pxHeight > 0)
            {
                float percent = (float)pxHeight / Screen.height;
                var rt = GetComponent<RectTransform>();

                // 【关键修正】：保留现有的 X 轴锚点，只改 Y 轴顶部
                // 这样如果你的 UI 本来就是左右铺满的，它依然会保持左右铺满
                rt.anchorMax = new Vector2(rt.anchorMax.x, 1 - percent);
        
                // 底部锚点通常保持不变（如果你只想压顶部的话）
                // rt.anchorMin = new Vector2(rt.anchorMin.x, rt.anchorMin.y); 

                // 【必须执行】：清空 Top 的偏移量
                // 因为改了 Anchor 后，Top 偏移如果不归零，UI 位置还是错的
                rt.offsetMax = new Vector2(rt.offsetMax.x, 0); 
                rt.offsetMin = new Vector2(rt.offsetMin.x, rt.offsetMin.y);

                Debug.Log($"[Adapter] 仅适配顶部，下压占比: {percent:P2}，左右锚点已保留");
            }
        }
    }
}
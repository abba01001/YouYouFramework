using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI; // 引入 DOTween 库

public class CardPlacement : MonoBehaviour
{
    public List<BaseCard> cards;  // 存储卡牌的Transform列表
    public float cardWidth = 233f;  // 每张卡牌的宽度
    public float yOffset = 450f;   // y轴下移的偏移量
    public float spacingFactor = 0.5f;  // 卡牌之间的水平间距比例
    public float offset = 85f;     // 额外的Y轴偏移量
    public float enterDuration = 8f;  // 卡牌入场的动画时长（加长时间）
    public float delayBetweenCards = 0.1f;  // 每张卡牌之间的延时（减少延时）
    private RectTransform canvasRectTransform;

    public void StartPlay(List<BaseCard> _cards,Canvas _canvas)
    {
        cards = _cards;
        canvasRectTransform = _canvas.GetComponent<RectTransform>();
        ArrangeCardsInArc();
    }

    private void CreateCardPanel()
    {
        // 创建CardPanel物体
        GameObject cardPanel = new GameObject("CardPanel");
        cardPanel.transform.SetParent(GameObject.Find("UIRoot").transform);  // 将CardPanel设置为当前物体的子物体
        cardPanel.transform.SetLocalPositionAndRotation(Vector3.zero,Quaternion.identity);
        cardPanel.AddComponent<CanvasRenderer>();
        cardPanel.AddComponent<GraphicRaycaster>();
        RectTransform rectTransform = cardPanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(0, 0);
        cardPanel.transform.localScale = Vector3.one;
    }
    
    void Start()
    {
        //CreateCardPanel();
        // canvasRectTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();  // 获取画布的RectTransform
        // ArrangeCardsInArc();  // 初始化时执行卡牌排列
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))  // 可以通过空格键重新排列卡牌
        {
            ArrangeCardsInArc();
        }
    }

    void ArrangeCardsInArc()
    {
        int cardCount = cards.Count;

        if (cardCount == 1)
        {
            cards[0].transform.position = new Vector3(0f, 0f, 0f);
            return;
        }

        float totalWidth = cardCount * cardWidth * spacingFactor;
        float offsetX = totalWidth / 2f;  // 计算偏移量，使中间卡牌 x = 0

        // 逐个动画化每张卡牌，倒序执行卡牌动画
        for (int i = cardCount - 1; i >= 0; i--)  // 倒序处理卡牌
        {
            // 计算当前卡牌的角度
            float angle = 10f - (5f * i); // 角度递减

            // 计算卡牌的 x 坐标
            float x = (i - (cardCount / 2)) * cardWidth * spacingFactor;

            // 计算卡牌的 y 坐标
            float y = -yOffset + offset;  // 初始 y 坐标

            // 根据卡牌的索引调整 y 坐标
            if (i == 1)
                y += 20f;  // 第二张卡牌，y值增加20
            else if (i == 2)
                y += 30f;  // 第三张卡牌，y值增加30
            else if (i == 3)
                y += 20f;  // 第四张卡牌，y值增加20

            // 设置卡牌的旋转角度（Z轴旋转）
            cards[i].transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // 获取画布的宽度和高度，计算左下角的位置
            Vector3 leftBottomCorner = new Vector3(-canvasRectTransform.rect.width / 2, -canvasRectTransform.rect.height / 2, 0f);

            // 使用 localPosition 将卡牌位置设为画布的左下角
            cards[i].transform.position = canvasRectTransform.TransformPoint(leftBottomCorner);

            // 使用 DOTween 让卡牌沿着贝塞尔曲线飞行
            Vector3 targetPos = new Vector3(x, y, 0f);  // 目标位置

            // 贝塞尔曲线的控制点：飞到上方，再向下插入
            Vector3 controlPoint1 = new Vector3(x - 100f, y + 150f, 0f);  // 控制点1：卡牌飞行到的上方位置（加大曲率）
            Vector3 controlPoint2 = new Vector3(x + 100f, y + 150f, 0f);  // 控制点2：卡牌飞行到的另一侧上方位置（加大曲率）

            // 设置动画：卡牌沿贝塞尔曲线飞行并到达最终目标位置
            cards[i].transform.DOPath(new Vector3[] { leftBottomCorner, controlPoint1, controlPoint2, targetPos }, enterDuration, PathType.CatmullRom)
                .SetDelay((cardCount - 1 - i) * delayBetweenCards)  // 逐张卡牌延迟，远的卡牌先飞
                .SetEase(Ease.Linear)  // 使用线性缓动，保证均匀平滑的运动效果
                .OnKill(() => OnCardAnimationComplete(cards[i]));  // 动画结束后调用回调

            // 旋转卡牌到目标角度
            cards[i].transform.DORotate(new Vector3(0f, 0f, angle), enterDuration)
                .SetDelay((cardCount - 1 - i) * delayBetweenCards)  // 逐张卡牌延迟
                .SetEase(Ease.Linear);  // 旋转时使用线性缓动，保持平滑
        }
    }

    // 每张卡牌动画完成后的回调
    private void OnCardAnimationComplete(BaseCard card)
    {
        Debug.Log($"卡牌动画完成: {card.name}");
        card.InitTrans();
        // 在这里执行卡牌动画完成后的其他操作
        // 例如: 启动下一阶段的卡牌行为，或触发UI更新等
    }
}

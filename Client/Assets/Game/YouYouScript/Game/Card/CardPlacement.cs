using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardPlacement : MonoBehaviour
{
    public List<BaseCard> cards;  // 存储卡牌的Transform列表
    private float cardWidth = 233f;  // 每张卡牌的宽度
    private float cardHeight = 330f;  // 每张卡牌的宽度
    private float yOffset = 300f;   // y轴下移的偏移量
    private float spacingFactor = 0.5f;  // 卡牌之间的水平间距比例
    private float offset = 85f;     // 额外的Y轴偏移量
    private float enterDuration = 0.5f;  // 卡牌入场的动画时长（加长时间）
    private float delayBetweenCards = 0.05f;  // 每张卡牌之间的延时（减少延时）
    private RectTransform canvasRectTransform;


    public void Init(Transform trans)
    {
        canvasRectTransform = trans.GetComponent<RectTransform>();
    }

    public void StartPlay(List<BaseCard> _cards)
    {
        cards = _cards;
        ArrangeCardsInArc();
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

    // 如果只有一张卡牌，直接放在中间
    if (cardCount == 1)
    {
        cards[0].transform.position = new Vector3(0f, -canvasRectTransform.rect.height / 2 - cardHeight, 0f); // 屏幕外位置
        cards[0].transform.DOMove(new Vector3(0f, -yOffset, 0f), enterDuration).SetEase(Ease.OutBack);
        return;
    }
    
    

    // 逐个动画化每张卡牌
    for (int i = 0; i < cardCount; i++)
    {
        int index = i;
        float angle = 0;
        float x = (index - (cardCount / 2)) * cardWidth * spacingFactor;
        float y = -yOffset;

        Vector3 targetPos = new Vector3(x, y, 0f);
        Vector3 startPos = new Vector3(0f, -canvasRectTransform.rect.height / 2, 0f);
        cards[index].transform.position = startPos;
        cards[index].transform.localScale = Vector3.zero;

        cards[index].transform.DOMove(targetPos, enterDuration)
            .SetDelay(index * delayBetweenCards)  // 设置延迟时间，顺序飞入
            .SetEase(Ease.OutBack)  // 弹性效果缓动
            .OnKill(() => OnCardAnimationComplete(cards[index])); // 动画结束回调

        cards[index].transform.DORotate(new Vector3(0f, 0f, angle), enterDuration)
            .SetDelay(index * delayBetweenCards)  // 设置旋转延迟
            .SetEase(Ease.OutBack); // 保持与位移动画一致的缓动效果
        
        cards[index].transform.DOScale(Vector3.one * 0.7f, enterDuration)
            .SetDelay(index * delayBetweenCards)  // 设置旋转延迟
            .SetEase(Ease.OutBack); // 保持与位移动画一致的缓动效果
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

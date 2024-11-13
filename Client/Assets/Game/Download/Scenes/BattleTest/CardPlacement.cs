using System;
using UnityEngine;
using System.Collections.Generic;

public class CardPlacement : MonoBehaviour
{
    public List<Transform> cards;  // 存储卡牌的Transform列表
    public float cardWidth = 233f;  // 每张卡牌的宽度
    public float yOffset = 450f;   // y轴下移的偏移量
    public float spacingFactor = 0.5f;  // 卡牌之间的水平间距比例
    public int offset = 85;
    void Start()
    {
        ArrangeCardsInArc();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ArrangeCardsInArc();
        }
    }

    void ArrangeCardsInArc()
    {
        int cardCount = cards.Count;

        if (cardCount == 1)
        {
            cards[0].position = new Vector3(0f, 0f, 0f);
            return;
        }

        float angleStep = 5f;  // 每张卡牌角度的变化量
        float initialAngle = 10f;  // 第一个卡牌的角度

        // 计算卡牌的总宽度，并设置中间卡牌为 0
        float totalWidth = cardCount * cardWidth * spacingFactor;
        float offsetX = totalWidth / 2f;  // 计算偏移量，使中间卡牌 x = 0

        // 计算每个卡牌的位置
        for (int i = 0; i < cardCount; i++)
        {
            // 计算当前卡牌的角度
            float angle = initialAngle - (angleStep * i); // 角度递减

            // 计算卡牌的 x 坐标，最中间的卡牌的 x 坐标为 0
            float x = (i - (cardCount / 2)) * cardWidth * spacingFactor;

            // 计算卡牌的 y 坐标
            float y = -yOffset; // 初始 y 坐标

            // 根据卡牌的索引调整 y 坐标
            if (i == 1)
                y += 20f;  // 第二张卡牌，y值增加20
            else if (i == 2)
                y += 30f;  // 第三张卡牌，y值增加30
            else if (i == 3)
                y += 20f;  // 第四张卡牌，y值增加20
            // 可以继续根据需要为更多卡牌添加y偏移

            // 设置卡牌的旋转角度（Z轴旋转）
            cards[i].rotation = Quaternion.Euler(0f, 0f, angle);

            // 设置卡牌的最终位置
            cards[i].position = new Vector3(x, y + offset, 0f);
            cards[i].localPosition = new Vector3(cards[i].localPosition.x, cards[i].localPosition.y, 0);
        }
    }
}

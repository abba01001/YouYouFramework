using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : BattleCharacter
{
    public void AIDecideCard()
    {
        // AI决定使用哪张卡牌
        // 可以基于敌人的当前精力、卡牌类型等因素来选择
    }

    public override void PlayCard(Card card, BattleCharacter target)
    {
        // 敌人出卡牌的具体行为
        if (Energy >= card.EnergyCost)
        {
            Energy -= card.EnergyCost;  // 消耗精力
            card.UseCard(target);        // 执行卡牌效果
        }
        else
        {
            // AI会选择其他策略（如防御或等待恢复精力等）
        }
    }
}

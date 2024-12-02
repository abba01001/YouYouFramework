using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : BattleCharacter
{
    public void ChooseCard()
    {
        // 玩家选择卡牌的逻辑
        // 比如显示UI界面让玩家选择卡牌
    }

    public override void PlayCard(Card card, BattleCharacter target)
    {
        // 玩家出卡牌的具体行为（如判断是否有足够精力）
        if (Energy >= card.EnergyCost)
        {
            Energy -= card.EnergyCost;  // 消耗精力
            card.UseCard(target);        // 执行卡牌效果
        }
        else
        {
            // 精力不足时的处理逻辑（比如无法出牌）
        }
    }
}

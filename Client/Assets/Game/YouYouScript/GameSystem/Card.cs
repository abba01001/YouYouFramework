using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public string CardName { get; set; }
    public int EnergyCost { get; set; }
    public int Priority { get; set; }
    
    public void UseCard(BattleCharacter target)
    {
        // 执行卡牌的效果
    }
}
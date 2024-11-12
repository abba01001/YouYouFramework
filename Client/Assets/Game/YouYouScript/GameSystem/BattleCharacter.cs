using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCharacter
{
    public string Name { get; set; }           // 角色名称
    public int Health { get; set; }            // 生命值
    public int Energy { get; set; }            // 当前精力值
    public List<Card> CardDeck { get; set; }   // 当前战斗用卡牌组
    public int PrioritySum { get; set; }       // 所有卡牌优先级总和
    public bool IsHuman { get; set; }  // 用于标记是否为玩家
    
    public void DrawCard(List<Card> availableCards)
    {
        // 从卡组中抽取新卡牌
    }

    public virtual void DecideCard()
    {
        
    }
    
    public virtual void PlayCard(Card card, BattleCharacter target)
    {
        // 选择并出牌，交给具体子类实现
        card.UseCard(target);
    }

    public void RecoverEnergy()
    {
        // 恢复精力值
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleParticipants
{
    public List<Player> Players { get; set; }      // 玩家列表
    public List<Enemy> Enemies { get; set; }      // 敌人列表
    public int TurnCount { get; set; }             // 当前回合数
    public BattleCharacter CurrentPlayer { get; set; } // 当前出牌的角色

    public BattleParticipants(List<Player> players,List<Enemy> enemies)
    {
        
        Players = players;
        Enemies = enemies;
    }

    public void PlayerChooseCard()
    {
        foreach (var player in Players)
        {
            player.DecideCard();
        }
    }
    
    public void EnemyChooseCard()
    {
        foreach (var enemy in Enemies)
        {
            enemy.DecideCard();
        }
    }
    
    public void DetermineTurnOrder()
    {
        if(Players.Count == 0 || Enemies.Count == 0) return;
        // 我们将分别根据玩家和敌人的优先级来决定出牌顺序
        var playerPrioritySum = Players.Sum(player => player.PrioritySum);  // 玩家方所有角色的优先级总和
        var enemyPrioritySum = Enemies.Sum(enemy => enemy.PrioritySum);  // 敌方所有角色的优先级总和

        // 判断哪个阵营的优先级总和较高，决定谁先出牌
        if (playerPrioritySum >= enemyPrioritySum)
        {
            // 玩家方先出牌
            CurrentPlayer = Players.OrderBy(player => player.PrioritySum).First();  // 玩家方优先级最高的角色先出牌
        }
        else
        {
            // 敌方先出牌
            CurrentPlayer = Enemies.OrderBy(enemy => enemy.PrioritySum).First();  // 敌方优先级最高的角色先出牌
        }
    }
    
    public void EndTurn()
    {
        // 结束当前回合，恢复精力并更新回合数
        foreach (var player in Players)
        {
            player.RecoverEnergy();
        }

        foreach (var enemy in Enemies)
        {
            enemy.RecoverEnergy();
        }
        TurnCount++;
        CheckVictory();
        SetNextTurn();
    }

    public void SetNextTurn()
    {
        // 轮到谁出牌，确定下一个出牌角色
        if (Players.Contains(CurrentPlayer))
        {
            // 如果是玩家方出牌，下一回合是敌方出牌
            CurrentPlayer = Enemies.OrderBy(enemy => enemy.PrioritySum).First();
        }
        else
        {
            // 如果是敌方出牌，下一回合是玩家方出牌
            CurrentPlayer = Players.OrderBy(player => player.PrioritySum).First();
        }
    }

    public void CheckVictory()
    {
        // 检查是否有一方胜利
        if (Players.All(player => player.Health <= 0))
        {
            // 所有玩家死亡，敌人胜利
            Console.WriteLine("Enemies Win!");
        }
        else if (Enemies.All(enemy => enemy.Health <= 0))
        {
            // 所有敌人死亡，玩家胜利
            Console.WriteLine("Players Win!");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using GameScripts;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class GoldEndgameAbilityBehavior : AbilityBehavior<GoldEndgameAbilityData, GoldEndgameAbilityLevel>
    {
        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            var gold = AbilityLevel.GoldAmount * PlayerBehavior.Player.GoldMultiplier;
            var clampedGold = Mathf.RoundToInt(gold);
            GameEntry.Data.AddProp((int)PropEnum.Coin,clampedGold);
        }
    }
}


using UnityEngine;

namespace OctoberStudio.Drop
{
    public class BombDropBehavior : DropBehavior
    {
        [SerializeField] float damageMultiplier = 100;

        public override void OnPickedUp()
        {
            base.OnPickedUp();
            
            StageController.EnemiesSpawner.DealDamageToAllEnemies(PlayerBehavior.Player.GetDamageValue() * damageMultiplier);

            gameObject.SetActive(false);
        }
    }
}


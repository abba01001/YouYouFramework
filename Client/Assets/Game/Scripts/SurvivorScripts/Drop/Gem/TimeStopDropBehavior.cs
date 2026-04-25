using OctoberStudio.Easing;
using UnityEngine;

namespace OctoberStudio.Drop
{
    public class TimeStopDropBehavior : DropBehavior
    {
        public override void OnPickedUp()
        {
            StageController.PauseDirector(true);
            StageController.EnemiesSpawner.PauseEnemiesBehavior(true);
            onFinished += () =>
            {
                StageController.PauseDirector(false);
                StageController.EnemiesSpawner.PauseEnemiesBehavior(false);
            };
            
            base.OnPickedUp();
            gameObject.SetActive(false);
        }
    }
}


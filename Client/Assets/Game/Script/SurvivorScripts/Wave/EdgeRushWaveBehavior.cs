using System.Collections.Generic;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class EdgeRushWaveBehavior : WaveBehavior
    {
        public EdgeRushWave RushWave;
        private int keepAliveCount = -1;
        private int aliveEnemiesCounter;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            keepAliveCount = EnemiesCount;
            aliveEnemiesCounter = 0;
            RushWave = CurWaveAsset as  EdgeRushWave;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (aliveEnemiesCounter < keepAliveCount)
            {
                List<EnemyBehavior> list = StageController.EnemiesSpawner.Spawn(EnemyType, WaveOverride, CircularSpawn, keepAliveCount - aliveEnemiesCounter, OnEnemyDied);
                aliveEnemiesCounter = keepAliveCount;
                if (list is {Count: > 0})
                {
                    foreach (var enemy in list)
                    {
                        var rush = enemy.gameObject.AddComponent<EnemyRush>();
                        rush.StartRush(RushWave.RushSpeed, RushWave.RushDistance, RushWave.PathColor);
                    }
                }
            }
        }
        
        

        private void OnEnemyDied(EnemyBehavior enemy)
        {
            enemy.onEnemyDied -= OnEnemyDied;
            aliveEnemiesCounter--;
        }
    }
}
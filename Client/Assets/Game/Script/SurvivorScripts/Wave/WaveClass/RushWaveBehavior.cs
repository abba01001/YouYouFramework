using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class RushWaveBehavior : WaveBehavior
    {
        public RushWave RushWave;
        private int keepAliveCount = -1;
        private int aliveEnemiesCounter;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            keepAliveCount = EnemiesCount;
            aliveEnemiesCounter = 0;
            RushWave = CurWaveAsset as  RushWave;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!Application.isPlaying) return;
            if (aliveEnemiesCounter < keepAliveCount)
            {
                List<EnemyBehavior> list = StageController.EnemiesSpawner.Spawn(EnemyType, WaveOverride, CircularSpawn, keepAliveCount - aliveEnemiesCounter, OnEnemyDied);
                aliveEnemiesCounter = keepAliveCount;
                if (list is {Count: > 0})
                {
                    foreach (var enemy in list)
                    {
                        _ = HandleRush(enemy);
                    }
                }
            }
        }

        private async UniTask HandleRush(EnemyBehavior enemyBehavior)
        {
            GameObject obj = await GameEntry.Pool.GameObjectPool.Spawn("Assets/Game/Download/SurvivorAsset/Prefabs/Other/RushComponent.prefab");
            obj.transform.SetParent(enemyBehavior.transform);
            obj.transform.localPosition = Vector3.zero;
            obj.GetComponent<RushComponent>().StartRush(enemyBehavior,RushWave);
        }
        

        private void OnEnemyDied(EnemyBehavior enemy)
        {
            enemy.onEnemyDied -= OnEnemyDied;
            aliveEnemiesCounter--;
        }
    }
}
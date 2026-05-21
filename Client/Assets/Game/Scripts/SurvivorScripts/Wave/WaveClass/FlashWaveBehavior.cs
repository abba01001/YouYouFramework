using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameScripts;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class FlashWaveBehavior : WaveBehavior
    {
        public FlashWave FlashWave;
        private int keepAliveCount = -1;
        private int aliveEnemiesCounter;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            keepAliveCount = EnemiesCount;
            aliveEnemiesCounter = 0;
            FlashWave = CurWaveAsset as  FlashWave;
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
                        _ = HandleFlash(enemy);
                    }
                }
            }
        }

        private async UniTask HandleFlash(EnemyBehavior enemyBehavior)
        {
            GameObject obj = await GameEntry.Pool.GameObjectPool.Spawn("Assets/Game/Download/SurvivorAsset/Prefabs/Other/FlashComponent.prefab");
            obj.transform.SetParent(enemyBehavior.transform);
            obj.transform.localPosition = Vector3.zero;
            obj.GetComponent<FlashComponent>().StartFlash(enemyBehavior,FlashWave);
        }
        

        private void OnEnemyDied(EnemyBehavior enemy)
        {
            enemy.onEnemyDied -= OnEnemyDied;
            aliveEnemiesCounter--;
        }
    }
}
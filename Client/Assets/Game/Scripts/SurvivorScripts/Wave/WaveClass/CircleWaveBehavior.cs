using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameScripts;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class CircleWaveBehavior : WaveBehavior
    {
        public CircleWave CircleWave;
        private int keepAliveCount = -1;
        private int aliveEnemiesCounter;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            keepAliveCount = EnemiesCount;
            aliveEnemiesCounter = 0;
            CircleWave = CurWaveAsset as  CircleWave;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!Application.isPlaying) return;
            if (aliveEnemiesCounter == 0)
            {
                List<EnemyBehavior> list = StageController.EnemiesSpawner.Spawn(EnemyType, WaveOverride, CircularSpawn, keepAliveCount - aliveEnemiesCounter, OnEnemyDied);
                aliveEnemiesCounter = keepAliveCount;
                if (list is {Count: > 0})
                {
                    HandleCircle(list);
                }
            }
        }

        private void HandleCircle(List<EnemyBehavior> list)
        {
            int count = list.Count;
            var player = PlayerBehavior.Player.transform;
            float randomRadius = GameUtil.RandomRange(CircleWave.Radius.x, CircleWave.Radius.y);
            for (int i = 0; i < count; i++)
            {
                var enemy = list[i];
                enemy.SetEnemyAnimType(BehaviorType.CircleBehavior);
                float angle = i * Mathf.PI * 2 / count;
                Vector3 pos = player.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * randomRadius;
                enemy.transform.position = pos;
                Observable.NextFrame().Subscribe(_ => { enemy.ResetEnemyAnimType(); });
            }
        }
        
        
        private void OnEnemyDied(EnemyBehavior enemy)
        {
            enemy.onEnemyDied -= OnEnemyDied;
            aliveEnemiesCounter--;
        }
    }
}
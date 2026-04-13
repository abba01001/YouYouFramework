using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class RushWave : WaveAsset
    {
        [Header("Edge Rush 专属参数")]
        [LabelText("冲锋速度")] public float RushSpeed; // 冲锋速度
        [LabelText("延时冲锋")][Range(0f,3f)] public float RushDelay;
        [LabelText("敌人数量")] [Range(1, 100)] public int enemiesCount = 1;

        public override int EnemiesCount => enemiesCount;

        public RushWaveBehavior template;


        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var wavePlayable = ScriptPlayable<RushWaveBehavior>.Create(graph, template);
            var waveData = wavePlayable.GetBehaviour();

            waveData.EnemyType = EnemyType;
            waveData.CurWaveAsset = this;
            waveData.EnemiesCount = enemiesCount;
            waveData.WaveOverride = waveOverride;
            waveData.CircularSpawn = circularSpawn;

            return wavePlayable;
        }

        private void OnValidate()
        {
            if (enemiesCount < 0) enemiesCount = 0;
        }
    }
}
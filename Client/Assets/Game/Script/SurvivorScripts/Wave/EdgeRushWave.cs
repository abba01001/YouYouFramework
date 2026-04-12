using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class EdgeRushWave : WaveAsset
    {
        [Header("Edge Rush 专属参数")]
        [LabelText("冲锋速度")] public float RushSpeed = 8f; // 冲锋速度
        [LabelText("冲锋距离")] public float RushDistance = 10f; // 冲锋距离
        [LabelText("轨迹颜色")] public Color PathColor = Color.cyan; // 轨迹颜色
        [LabelText("敌人数量")] [SerializeField, Min(1)] int enemiesCount = 1;

        public override int EnemiesCount => enemiesCount;

        public EdgeRushWaveBehavior template;


        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var wavePlayable = ScriptPlayable<EdgeRushWaveBehavior>.Create(graph, template);
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
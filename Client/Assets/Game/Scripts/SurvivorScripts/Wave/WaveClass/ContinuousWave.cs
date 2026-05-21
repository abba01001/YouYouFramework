using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class ContinuousWave : WaveAsset
    {
        [Title("配置说明")][InfoBox("$GetDynamicInfo")][PropertyOrder(-999)]
        [LabelText("生成频率")] public float spawnFrequency = 1;

        private string GetDynamicInfo() 
        {
            if (spawnFrequency <= 0) return "当前不会生成任何敌人。";
            float interval = 1f / spawnFrequency;

            // 如果正好是整数，可以说“每秒X名”
            if (Mathf.Approximately(spawnFrequency % 1, 0))
            {
                return $"生成节奏：每秒生成 {(int)spawnFrequency} 名敌人。";
            }
            // 如果是小数，统一描述为“每隔多久生成一名”
            // 比如 3.91，会显示：“每隔 0.26 秒生成 1 名敌人”
            return $"生成节奏：每隔 {interval:F2} 秒生成 1 名敌人。";
        }

        public override int EnemiesCount => (int)(spawnFrequency * duration / 2);

        public ContinuousWaveBehavior template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var wavePlayable = ScriptPlayable<ContinuousWaveBehavior>.Create(graph, template);
            var waveData = wavePlayable.GetBehaviour();

            waveData.EnemyType = EnemyType;
            waveData.ContinuousSpawnPerSecond = spawnFrequency;

            waveData.WaveOverride = waveOverride;
            waveData.CircularSpawn = circularSpawn;
            return wavePlayable;
        }

        private void OnValidate()
        {
            if (spawnFrequency < 0) spawnFrequency = 0;
        }
    }
}
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class BurstWave : WaveAsset
    {
        [Title("配置说明")][InfoBox("$GetDynamicInfo")][PropertyOrder(-999)]
        [LabelText("单波刷怪数量")] public int enemiesCount = 1;
        [LabelText("总爆发波次")] public int burstCount = 1;
        
        private string GetDynamicInfo() 
        {
            if (burstCount <= 0 || enemiesCount <= 0) return "当前配置不会生成敌人。";
            // 因为逻辑是在 Clip 时长内平分波次
            return $"生成模式：爆发式生成。\n" +
                   $"节奏细节：在当前片段时长内，均匀分成 {burstCount} 波出现，每波瞬间刷出 {enemiesCount} 名敌人。\n" +
                   $"本次爆发总计：{enemiesCount * burstCount} 名敌人。";
        }
        
        public override int EnemiesCount => (int)(enemiesCount * 1.2f);

        public BurstWaveBehavior template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var wavePlayable = ScriptPlayable<BurstWaveBehavior>.Create(graph, template);
            var waveData = wavePlayable.GetBehaviour();

            waveData.EnemyType = EnemyType;
            
            waveData.BurstCount = burstCount;
            waveData.EnemiesCount = enemiesCount;

            waveData.WaveOverride = waveOverride;
            waveData.CircularSpawn = circularSpawn;

            return wavePlayable;
        }

        private void OnValidate()
        {
            if (enemiesCount < 0) enemiesCount = 0;
            if (burstCount < 0) burstCount = 0;
        }
    }
}
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class MaintainWave : WaveAsset
    {
        [Title("配置说明")][InfoBox("$GetDynamicInfo")][PropertyOrder(-999)]
        [LabelText("维持数量")]public int enemiesCount = 1;

        private string GetDynamicInfo() 
        {
            if (enemiesCount <= 0) return "当前不生成敌人。";
            return $"生成模式：动态补位。\n" +
                   $"逻辑细节：场上将始终维持 {enemiesCount} 名敌人。每当有一名敌人死亡，系统会立刻刷出一名新的进行补位。";
        }
        
        public override int EnemiesCount => enemiesCount;

        public MaintainWaveBehavior template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var wavePlayable = ScriptPlayable<MaintainWaveBehavior>.Create(graph, template);
            var waveData = wavePlayable.GetBehaviour();

            waveData.EnemyType = EnemyType;

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
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class CircleWave : WaveAsset
    {
        [Title("配置说明")] [InfoBox("$GetDynamicInfo")] [PropertyOrder(-999)]
        [LabelText("每圈敌人数量")] [Range(1, 100)] public int enemiesCount = 1;
        [LabelText("环形半径范围"), MinMaxSlider(1f, 5f)] [Min(1f)]  public Vector2 Radius = new Vector2(1f, 5f);
        public override int EnemiesCount => enemiesCount;

        public CircleWaveBehavior template;


        private string GetDynamicInfo() 
        {
            if (enemiesCount <= 0) return "当前不生成敌人。";
            return $"生成模式：环形包围。\n" +
                   $"逻辑细节：以玩家为圆心，在半径 {Radius.x}~{Radius.y} 范围内围圈。一组敌人（{enemiesCount}个）全部死亡后，才会触发下一波环形刷新。";
        }
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var wavePlayable = ScriptPlayable<CircleWaveBehavior>.Create(graph, template);
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
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class CircleWave : WaveAsset
    {
        [Header("Circle 专属参数")]
        [LabelText("敌人数量")] [Range(1, 100)] public int enemiesCount = 1;
        [LabelText("圈圈半径大小"), MinMaxSlider(1f, 5f)] [Min(1f)]  public Vector2 Radius = new Vector2(1f, 5f);
        public override int EnemiesCount => enemiesCount;

        public CircleWaveBehavior template;


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
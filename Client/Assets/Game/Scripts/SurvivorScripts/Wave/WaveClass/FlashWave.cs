using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class FlashWave : WaveAsset
    {
        [Header("Flash 专属参数")]
        [LabelText("闪现到玩家周边的距离(区间随机值)"), MinMaxSlider(0f, 2f)] public Vector2 SpawnDistanceRange;
        [LabelText("达到此距离触发闪现(区间随机值)"), MinMaxSlider(0f, 5f)] public Vector2 FlashDistance;
        [LabelText("闪现前的僵直时间(区间随机值)"), MinMaxSlider(0f, 2f)] [Min(0)]  public Vector2 FlashDelay;
        
        [LabelText("轨迹/残影颜色")] public Color PathColor;
        [LabelText("敌人数量")] [Range(1, 100)] public int enemiesCount;

        public override int EnemiesCount => enemiesCount;

        public FlashWaveBehavior template;


        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var wavePlayable = ScriptPlayable<FlashWaveBehavior>.Create(graph, template);
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
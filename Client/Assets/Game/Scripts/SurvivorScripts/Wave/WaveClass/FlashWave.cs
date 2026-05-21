using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class FlashWave : WaveAsset
    {
        [Title("配置说明")][InfoBox("$GetDynamicInfo")][PropertyOrder(-999)]
        [LabelText("闪现落点半径(区间随机值)"), MinMaxSlider(0f, 2f)] public Vector2 SpawnDistanceRange =  new Vector2(0f, 2f);
        [LabelText("触发闪现距离(区间随机值)"), MinMaxSlider(2f, 5f)] public Vector2 FlashDistance =  new Vector2(2f, 5f);
        [LabelText("闪现前摇时长(区间随机值)"), MinMaxSlider(0f, 2f)] [Min(0)]  public Vector2 FlashDelay =  new Vector2(0f, 2f);
        
        [LabelText("轨迹/残影颜色")] public Color PathColor;
        [LabelText("场上常驻数量")] [Range(1, 100)] public int enemiesCount = 1;

        public override int EnemiesCount => enemiesCount;
        public FlashWaveBehavior template;

        private string GetDynamicInfo() 
        {
            if (enemiesCount <= 0) return "当前不生成敌人。";
            return $"生成模式：闪现补位。\n" +
                   $"1. 数量维持：死一个补一个，场上始终保持 {enemiesCount} 个敌人。\n" +
                   $"2. 入场方式：敌人生成后，当玩家与其距离达到 {FlashDistance.x}~{FlashDistance.y} 时，" +
                   $"会在等待 {FlashDelay.x:F1}~{FlashDelay.y:F1}s 僵直后，闪现到玩家周边 {SpawnDistanceRange.x}~{SpawnDistanceRange.y} 的位置。";
        }

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
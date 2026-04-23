using Sirenix.OdinInspector;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class WaveBehavior : PlayableBehaviour
    {
        public WaveAsset CurWaveAsset { get; set; }
        public EnemyType EnemyType { get; set; }
        public int EnemiesCount { get; set; }

        public WaveOverride WaveOverride { get; set; }
        public bool CircularSpawn { get; set; }
    }
}

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(MusicSource))]
    public class DefaultMusicController : MonoBehaviour
    {
        public static MusicSource MusicSource { get; private set; }

        public async UniTask Initialise()
        {
            MusicSource = GetComponent<MusicSource>();
            MusicSource.Init();
            await UniTask.NextFrame();
        }

        public static void ActivateMusic()
        {
            MusicSource.Activate();
            MusicSource.SetAsDefault();
        }
    }
}

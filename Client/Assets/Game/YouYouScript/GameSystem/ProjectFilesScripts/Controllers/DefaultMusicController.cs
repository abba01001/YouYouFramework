using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(MusicSource))]
    public class DefaultMusicController
    {
        private static DefaultMusicController _instance;
        public static DefaultMusicController Instance => _instance ??= new DefaultMusicController();
        public  MusicSource MusicSource { get; private set; }

        public async UniTask Initialise(Transform transform)
        {
            MusicSource = transform.GetComponent<MusicSource>();
            MusicSource.Init();
            await UniTask.NextFrame();
        }

        public  void ActivateMusic()
        {
            MusicSource.Activate();
            MusicSource.SetAsDefault();
        }
    }
}

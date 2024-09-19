using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace YouYou
{
    [System.Serializable]
    public class PlaySoundEventArgs
    {
        /// <summary>
        /// Ŀ���
        /// </summary>
        [Header("Ŀ���")]
        public DynamicTarget Target;

        /// <summary>
        /// �����ļ�
        /// </summary>
        [Header("�����ļ�")]
        public AudioClip AudioClip;
    }
    public class PlaySoundPlayable : BasePlayableAsset<PlaySoundPlayableBehaviour, PlaySoundEventArgs>
    {
    }
    public class PlaySoundPlayableBehaviour : BasePlayableBehaviour<PlaySoundEventArgs>
    {
        protected override void OnYouYouBehaviourPlay(Playable playable, FrameData info)
        {
            CurrTimelineCtrl.PlaySound?.Invoke(CurrArgs);
        }
        protected override void OnYouYouBehaviourStop(Playable playable, FrameData info)
        {

        }
    }
}
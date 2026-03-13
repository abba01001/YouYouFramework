using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;


    [System.Serializable]
    public class PlayAnimEventArgs
    {
        [Header("Ŀ���")]
        public DynamicTarget Target;

        [Header("������Դ")]
        public AnimationClip AnimationClip;

        [Header("��������")]
        public int Param = 0;

    }
    public class PlayAnimPlayable : BasePlayableAsset<PlayAnimPlayableBehaviour, PlayAnimEventArgs>
    {
    }
    public class PlayAnimPlayableBehaviour : BasePlayableBehaviour<PlayAnimEventArgs>
    {
        protected override void OnYouYouBehaviourPlay(Playable playable, FrameData info)
        {
            CurrTimelineCtrl.PlayAnim?.Invoke(CurrArgs);
        }

        protected override void OnYouYouBehaviourStop(Playable playable, FrameData info)
        {

        }
    }

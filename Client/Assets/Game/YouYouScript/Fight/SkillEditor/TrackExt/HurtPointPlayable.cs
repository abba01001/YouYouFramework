using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

    [System.Serializable]
    public class HurtPointEventArgs
    {
        [Header("�˺���Χ")]
        public int hurtRange = 5;

        [Header("�˺�ֵ")]
        public int hurtValue = 10;

        [Header("Buff���")]
        public BuffCategory buffCategory;

        [Header("Buffֵ, ����ѣ��1.2�� ����1.2")]
        public float buffValue;
    }
    public class HurtPointPlayable : BasePlayableAsset<HurtPointPlayableBehaviour, HurtPointEventArgs>
    {
    }
    public class HurtPointPlayableBehaviour : BasePlayableBehaviour<HurtPointEventArgs>
    {
        protected override void OnYouYouBehaviourPlay(Playable playable, FrameData info)
        {
            CurrTimelineCtrl.HurtPoint?.Invoke(CurrArgs);
        }

        protected override void OnYouYouBehaviourStop(Playable playable, FrameData info)
        {

        }
}
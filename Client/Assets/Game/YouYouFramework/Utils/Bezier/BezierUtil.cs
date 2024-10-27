using System;
using UnityEngine;
using UnityEngine.UI;


namespace BezierAnimation
{
    public class BezierUtil
    {
        public static class BezierAnimUtility
        {
            public static Gradient GradientFadeOut()
            {
                GradientColorKey[] colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(Color.white, 0),
                    new GradientColorKey(Color.black, 1),
                };
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1, 0),
                    new GradientAlphaKey(0, 1),
                };

                Gradient gradient = new Gradient();
                gradient.SetKeys(colorKeys, alphaKeys);
                return gradient;
            }
            public static Gradient GradientFadeIn()
            {
                GradientColorKey[] colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(Color.black, 0),
                    new GradientColorKey(Color.white, 1),
                };
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0, 0),
                    new GradientAlphaKey(1, 1),
                };

                Gradient gradient = new Gradient();
                gradient.SetKeys(colorKeys, alphaKeys);
                return gradient;
            }
        }
        
        [Serializable]
        public class ColorSegment : BezierAnimSegment
        {
            [SerializeField] private Gradient m_Gradient = BezierAnimUtility.GradientFadeOut();

            public Gradient gradient
            {
                get { return m_Gradient; }
                set { m_Gradient = value; }
            }

            public Color Evaluate(float time)
            {
                return gradient.Evaluate(time);
            }
        }
    }
    
    [Serializable]
    public class BezierAnimSegment
    {
        [SerializeField]
        private float m_Duration = 1f;
        public float duration { get { return m_Duration; } set { m_Duration = value >= 0 ? value : 0; } }
        [SerializeField]
        private AnimationCurve m_Curve = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve curve { get { return m_Curve; } set { m_Curve = value; } }
    }
    
    // [Serializable]
    // public class FloatSegment : BezierAnimSegment
    // {
    //     [SerializeField]
    //     private float m_StartValue = 0;
    //     public float startValue { get { return m_StartValue; } set { m_StartValue = value; } }
    //
    //     [SerializeField]
    //     private float m_EndValue = 1;
    //     public float endValue { get { return m_EndValue; } set { m_EndValue = value; } }
    //
    //     public float Evaluate(float time)
    //     {
    //         return Mathf.Lerp(startValue, endValue, time);
    //     }
    // }
    
    [Serializable]
    public class TransSegment : BezierAnimSegment
    {
        [SerializeField]
        private TransPathPoint m_StartPoint;
        public TransPathPoint startPoint { get { return m_StartPoint; } set { m_StartPoint = value; } }
        [SerializeField]
        private TransPathPoint m_EndPoint;
        public TransPathPoint endPoint { get { return m_EndPoint; } set { m_EndPoint = value; } }
    }
    
    public class GraphicColorAnim : BezierAnim<Graphic, BezierUtil.ColorSegment>
    {
        protected override void OnSegmentUpdate()
        {
            target.color = activeSegment.Evaluate(segmentProcess);
        }
    }
}
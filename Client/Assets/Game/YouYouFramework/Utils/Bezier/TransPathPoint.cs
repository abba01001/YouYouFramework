/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-07-02 17:15:10
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using UnityEngine;

namespace BezierAnimation
{
    [DisallowMultipleComponent]
    public class TransPathPoint : MonoBehaviour
    {
        [Header("切线独立")]
        [SerializeField]
        private bool m_BrokenTangent;
        public bool brokenTangent { get { return m_BrokenTangent; } }

        [Header("开始切线")]
        [SerializeField]
        private Vector3 m_StartTangent = Vector3.back;
        public Vector3 startTangent { get { return m_StartTangent; } set { m_StartTangent = value; } }

        [Header("终点切线(相对于上一个连接)")]
        [SerializeField]
        private Vector3 m_EndTangent = Vector3.forward;
        public Vector3 endTangent { get { return m_EndTangent; } set { m_EndTangent = value; } }

        public Vector3 position { get { return transform.position; } }
        public Vector3 startTangentPosition { get { return transform.position + transform.TransformDirection(startTangent); } }
        public Vector3 endTangentPosition { get { return transform.position + transform.TransformDirection(endTangent); } }

        private void OnValidate()
        {
            if (!m_BrokenTangent)
            {
                m_EndTangent = -m_StartTangent;
            }
        }
    }
}

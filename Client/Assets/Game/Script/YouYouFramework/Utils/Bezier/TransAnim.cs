using System;
using Unity.VisualScripting;
using UnityEngine;

namespace BezierAnimation
{
    [DisallowMultipleComponent]
    public class TransAnim : BezierAnim<Transform, TransSegment>
    {
        public enum PathMode
        {
            Linear,
            Bezier,
        }

        [Header("运动曲线")] [SerializeField] private PathMode m_PathMode = PathMode.Linear;

        public PathMode pathMode
        {
            get { return m_PathMode; }
            set { m_PathMode = value; }
        }

        protected override void OnSegmentUpdate()
        {
            if (target != null)
            {
                Vector3 position = target.position;
                Quaternion rotation = target.rotation;
                Vector3 scale = target.lossyScale;
                if (GetPoint(ref position, ref rotation, ref scale, segmentIndex, segmentProcess,
                        loopMode == LoopMode.LoopAnimation))
                {
                    target.position = position;
                    target.rotation = rotation;
                    SetLossyScale(target, scale);
                }
            }
        }

        public bool GetPoint(ref Vector3 position, ref Quaternion rotation, ref Vector3 scale, int section,
            float progress, bool loop)
        {
            if (section < 0)
            {
                Debug.LogException(new ArgumentOutOfRangeException("section"));
                return false;
            }

            if (section >= segments.Count && !loop)
            {
                TransSegment segment = segments[segments.Count - 1];
                if (segment.endPoint != null)
                {
                    TransPathPoint pathPoint = segment.endPoint;
                    position = pathPoint.position;
                    rotation = pathPoint.transform.rotation;
                    scale = pathPoint.transform.lossyScale;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                TransSegment segment = segments[section % segments.Count];
                if (segment.startPoint != null && segment.endPoint != null)
                {
                    TransPathPoint startPoint = segment.startPoint;
                    TransPathPoint endPoint = segment.endPoint;
                    if (pathMode == PathMode.Bezier)
                    {
                        position = CalcBezierPoint(startPoint, endPoint, progress);
                    }
                    else
                    {
                        position = Vector3.Lerp(startPoint.position, endPoint.position, progress);
                    }

                    rotation = Quaternion.Lerp(startPoint.transform.rotation, endPoint.transform.rotation, progress);
                    scale = Vector3.Lerp(startPoint.transform.lossyScale, endPoint.transform.lossyScale, progress);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static Vector3 CalcBezierPoint(TransPathPoint p1, TransPathPoint p2, float progress)
        {
            return CalcBezierPoint(p1.position, p1.startTangentPosition, p2.endTangentPosition, p2.position, progress);
        }

        public static Vector3 CalcBezierPoint(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float progress)
        {
            float t1 = progress;
            float t2 = 1 - progress;
            return p1 * t2 * t2 * t2
                   + p2 * t2 * t2 * t1 * 3
                   + p3 * t2 * t1 * t1 * 3
                   + p4 * t1 * t1 * t1;
        }

        public const float minScale = 1e-5f;

        public static void SetLossyScale(Transform t, Vector3 scale)
        {
            if (t.parent == null)
            {
                t.localScale = scale;
                return;
            }

            Vector3 parentScale = t.parent.lossyScale;
            scale.x = parentScale.x < minScale ? 0 : (scale.x / parentScale.x);
            scale.y = parentScale.y < minScale ? 0 : (scale.y / parentScale.y);
            scale.z = parentScale.z < minScale ? 0 : (scale.z / parentScale.z);
            t.localScale = scale;
        }

        public void DuplicateObject()
        {
            GameObject duplicatedObject = Instantiate(transform.gameObject, transform.position + Vector3.right,
                transform.rotation); // 保持原物体的旋转
            duplicatedObject.transform.SetParent(transform.parent, false); // 传入 false，保持世界坐标
            Debug.LogError("克隆的物体: " + duplicatedObject.name);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.grey;
            DrawGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            DrawGizmos();
        }

        private void DrawGizmos()
        {
            switch (pathMode)
            {
                case PathMode.Linear:
                    DrawGizmos(DrawLinearGizmos);
                    break;
                case PathMode.Bezier:
                    DrawGizmos(DrawBezierGizmos);
                    break;
            }
        }

        private void DrawGizmos(Action<TransPathPoint, TransPathPoint> drawer)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                TransSegment segment = segments[i];
                if (segment.startPoint != null && segment.endPoint != null)
                {
                    drawer(segment.startPoint, segment.endPoint);
                }
            }
        }

        private void DrawLinearGizmos(TransPathPoint p1, TransPathPoint p2)
        {
            UnityEditor.Handles.DrawLine(p1.position, p2.position);
        }

        private void DrawBezierGizmos(TransPathPoint p1, TransPathPoint p2)
        {
            UnityEditor.Handles.DrawBezier(p1.position, p2.position, p1.startTangentPosition, p2.endTangentPosition,
                Gizmos.color, null, 1f);
        }
#endif
    }
}
using UnityEditor;
using UnityEngine;

namespace BezierAnimation
{
    [CustomEditor(typeof(TransAnim), true), CanEditMultipleObjects]
    public class TransAnimEditor : BezierAnimEditor
    {
        protected TransAnim transformAnimation;
        protected SerializedProperty m_PathMode;

        protected override void GetOtherProperties()
        {
            transformAnimation = target as TransAnim;
            m_PathMode = serializedObject.FindProperty("m_PathMode");
        }
        protected override void DrawPropertiesAboveSegments()
        {
            EditorGUILayout.PropertyField(m_PathMode);
        }

        protected override void DrawSegmentListHeader(Rect rect)
        {
            rect.x += headerIndent; rect.width -= headerIndent;
            float width = rect.width / 2; rect.width -= horizontalSpace;
            EditorGUI.LabelField(rect, "Start Point");
            rect.x += width;
            EditorGUI.LabelField(rect, "End Point");
        }
        protected override float GetSegmentListElementHeight(int index)
        {
            return base.GetSegmentListElementHeight(index) * 2;
        }
        protected override Rect OnSegmentProperty(Rect rect, SerializedProperty segment)
        {
            SerializedProperty startPoint = segment.FindPropertyRelative("m_StartPoint");
            SerializedProperty endPoint = segment.FindPropertyRelative("m_EndPoint");
            float width = rect.width / 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, width - horizontalSpace, singleLineHeight), startPoint, GUIContent.none);
            EditorGUI.PropertyField(new Rect(rect.x + width, rect.y, width - horizontalSpace, singleLineHeight), endPoint, GUIContent.none);
            rect.y += singleLineHeight + verticalSpace;
            rect.height /= 2;
            return rect;
        }

        private void OnSceneGUI()
        {
            if (transformAnimation.pathMode == TransAnim.PathMode.Bezier)
            {
                for (int i = 0; i < transformAnimation.segments.Count; i++)
                {
                    TransSegment segment = transformAnimation.segments[i];
                    if (segment.startPoint != null)
                    {
                        TransPathPointEditor.DrawTangentHandles(segment.startPoint);
                    }
                    if (segment.endPoint != null)
                    {
                        TransPathPointEditor.DrawTangentHandles(transformAnimation.segments[i].endPoint);
                    }
                }
            }
        }
    }
}
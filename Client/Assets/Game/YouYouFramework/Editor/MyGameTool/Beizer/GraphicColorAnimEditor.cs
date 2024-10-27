using UnityEditor;
using UnityEngine;

namespace BezierAnimation
{
    [CustomEditor(typeof(GraphicColorAnim), true), CanEditMultipleObjects]
    public class GraphicColorAnimEditor : BezierAnimEditor
    {
        protected override float GetSegmentListElementHeight(int index)
        {
            return base.GetSegmentListElementHeight(index) * 2;
        }
        protected override Rect OnSegmentProperty(Rect rect, SerializedProperty segment)
        {
            SerializedProperty gradient = segment.FindPropertyRelative("m_Gradient");
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, singleLineHeight), gradient, GUIContent.none);
            rect.y += singleLineHeight + verticalSpace;
            rect.height /= 2;
            return rect;
        }
    }
}
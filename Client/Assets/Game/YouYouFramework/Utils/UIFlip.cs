using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("UI/MeshEffectForTextMeshPro/UIFlip", 102)]
public class UIFlip : BaseMeshEffect
{
    [Tooltip("Flip horizontally.")]
    [SerializeField] private bool m_Horizontal = false;

    [Tooltip("Flip vertically.")]
    [SerializeField] private bool m_Vertical = false;

    public bool horizontal { get { return this.m_Horizontal; } set { this.m_Horizontal = value; graphic?.SetAllDirty();  } }
    public bool vertical { get { return this.m_Vertical; } set { this.m_Vertical = value; graphic?.SetAllDirty();  } }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || graphic == null || vh.currentVertCount == 0) return;

        RectTransform rt = graphic.rectTransform;
        Vector2 center = rt.rect.center;

        UIVertex vt = default(UIVertex);
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vt, i);
            vt.position = new Vector3(
                m_Horizontal ? 2 * center.x - vt.position.x : vt.position.x,
                m_Vertical ? 2 * center.y - vt.position.y : vt.position.y
            );
            vh.SetUIVertex(vt, i);
        }
    }
}
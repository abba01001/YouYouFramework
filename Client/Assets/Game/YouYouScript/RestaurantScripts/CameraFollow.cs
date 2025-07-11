using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class CameraFollow : MonoBehaviour
{
    public float distance;
    public float height;
    public float smoothness;

    public Vector3 offset;

    Vector3 velocity;

    void LateUpdate()
    {
        if (!GameEntry.Instance.PlayerController)
            return;

        Vector3 pos = Vector3.zero;
        pos.x = GameEntry.Instance.PlayerController.transform.position.x;
        pos.y = GameEntry.Instance.PlayerController.transform.position.y + height;
        pos.z = GameEntry.Instance.PlayerController.transform.position.z - distance;

        transform.position = Vector3.SmoothDamp(transform.position, pos+offset, ref velocity, smoothness);
    }

    //public Material m;
    //public float TransparencyLevel;

    //private void Awake()
    //{
    //    m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
    //    m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
    //    m.SetInt("_ZWrite", 0);
    //    m.DisableKeyword("_ALPHATEST_ON");
    //    m.DisableKeyword("_ALPHABLEND_ON");
    //    m.EnableKeyword("_ALPHAPREMULTIPLY_ON");
    //    m.renderQueue = 3000;

    //}

}

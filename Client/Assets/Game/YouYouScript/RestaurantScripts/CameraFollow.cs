using System.Collections.Generic;
using DG.Tweening;
using Protocols;
using UnityEngine;
using YouYou;

public class CameraFollow : MonoBehaviour
{
    public float distance;
    public float height;
    public float smoothness;

    public Vector3 offset;
    public Transform inputTrans;
    Vector3 velocity;

    public Vector3 recordPos;
    public Vector3 recordRota;
    public bool isTest = false;
    void LateUpdate()
    {
        if (isTest) return;
        if (BuildingSystem.Instance.PlayerController != null)
        {
            Vector3 pos = Vector3.zero;
            pos.x = BuildingSystem.Instance.PlayerController.transform.position.x;
            pos.y = BuildingSystem.Instance.PlayerController.transform.position.y + height;
            pos.z = BuildingSystem.Instance.PlayerController.transform.position.z - distance;
            transform.position = Vector3.SmoothDamp(transform.position, pos+offset, ref velocity, smoothness);
        }

        if (inputTrans != null)
        {
            Vector3 pos = Vector3.zero;
            pos.x = inputTrans.transform.position.x;
            pos.y = inputTrans.position.y + height;
            pos.z = inputTrans.position.z - distance;
            transform.position = Vector3.SmoothDamp(transform.position, pos+offset, ref velocity, smoothness);
            recordPos = transform.position;
            recordRota = transform.rotation.eulerAngles;
        }

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

    public void Test()
    {
        isTest = true;
    }
    
    public void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Q))
        // {
        //     Test();
        //     transform.DOMove(new Vector3(-9, 5, -1), 1f);
        //     transform.DORotate(new Vector3(0, -90, 0), 1f);
        // }
        // if (Input.GetKeyDown(KeyCode.W))
        // {
        //     Test();
        //     
        //     Vector3 pos = Vector3.zero;
        //     pos.x = inputTrans.transform.position.x;
        //     pos.y = inputTrans.position.y + height;
        //     pos.z = inputTrans.position.z - distance;
        //     
        //     transform.DOMove(pos+offset, 1f);
        //     transform.DORotate(recordRota, 1f).OnComplete(() =>
        //     {
        //         isTest = false;
        //     });
        // }
    }
}

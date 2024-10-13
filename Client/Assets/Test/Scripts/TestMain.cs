using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Protocols;
using UnityEngine;

public class TestMain : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (NetManager.Instance == null)
        {
            GameObject netGameObj = new GameObject("Net");
            netGameObj.AddComponent<NetManager>();
        }
        NetManager.Instance.ConnectServer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}

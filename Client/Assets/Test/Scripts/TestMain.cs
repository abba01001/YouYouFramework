using System.Collections;
using System.Collections.Generic;
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

        NetManager.Instance.ConnectServer("127.0.0.1", 8080);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}

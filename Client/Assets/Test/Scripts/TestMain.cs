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

        
        // 序列化
        NetworkMessage serializeMsg = new Protocols.NetworkMessage();
        serializeMsg.Content = "你好";
        var json = serializeMsg.ToString();              // 转 json
        var byteStr = serializeMsg.ToByteString();       // 转 byte String
        var byteArr = serializeMsg.ToByteArray();        // 转 byte Array

        GameUtil.LogError(json);
        GameUtil.LogError(byteStr);
        GameUtil.LogError(byteArr);
        
        NetManager.Instance.ConnectServer("127.0.0.1", 8080);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}

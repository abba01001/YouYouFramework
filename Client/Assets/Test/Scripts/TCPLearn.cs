using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine.UI;

public class TCPLearn : MonoBehaviour
{
    public Text msgText;
    // Start is called before the first frame update
    void Start()
    {
        //创建socket
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //连接服务器
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("123.207.203.230"),17777);
        try
        {
            socket.Connect(endpoint);
        }catch(SocketException e)
        {
            Debug.Log("服务器连接失败:" + e.ErrorCode);//10061是服务器拒绝连接
        }


        //收发消息
        //收消息
        byte[] msg = new byte[1024];
        int msgLength = socket.Receive(msg);
        string msgStr = Encoding.UTF8.GetString(msg, 0, msgLength);
        Debug.Log("收到服务器发送的消息：" + msgStr);
        msgText.text = msgStr;
        //发消息
        socket.Send(Encoding.UTF8.GetBytes("这里是unity客户端，over！"));

        //释放连接
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}

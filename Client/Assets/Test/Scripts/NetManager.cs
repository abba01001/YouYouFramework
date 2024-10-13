using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System;

public class NetManager : MonoBehaviour
{
    public static NetManager Instance => instance;
    private static NetManager instance;

    private Socket socket;

    private bool isConnect;

    private Queue<byte[]> sendQueue = new Queue<byte[]>();
    private Queue<byte[]> receiveQueue = new Queue<byte[]>();

    private byte[] receiveBytes = new byte[1024 * 1024];

    private ChatPanel chatPanel;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        GameObject chatPanelObj = GameObject.Find("ChatPanel");
        if (chatPanel != null)
        {
            chatPanel = chatPanelObj.GetComponent<ChatPanel>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(receiveQueue.Count > 0)
        {
            ReadingMsgType(receiveQueue.Dequeue());
        }
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="ip">服务器IP地址</param>
    /// <param name="port">服务器程序端口号</param>
    public void ConnectServer(string ip, int port)
    {
        //如果在连接状态，就不执行连接逻辑了
        if (isConnect)
            return;

        //避免重复创建socket
        if(socket == null)
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //连接服务器
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

        try
        {
            socket.Connect(ipEndPoint);
        }
        catch (SocketException e)
        {
            print(e.ErrorCode + e.Message);
            return;
        }
        
        isConnect = true;

        //开启发送消息线程
        ThreadPool.QueueUserWorkItem(SendMsg_Thread);
        //开启接收消息线程
        ThreadPool.QueueUserWorkItem(ReceiveMsg_Thread);
    }

    //发送消息
    public void Send<T>(T msg) where T : BaseMsg
    {
        //将消息放入到消息队列中
        sendQueue.Enqueue(msg.SerializeData());
    }

    private void SendMsg_Thread(object obj)
    {
        while (isConnect)
        {
            //如果消息队列中有消息，则发送消息
            if(sendQueue.Count > 0)
            {
                socket.Send(sendQueue.Dequeue());
            }
        }
    }

    //接收消息
    private void ReceiveMsg_Thread(object obj)
    {
        print("持续监听是否收到消息");
        int msgLength;
        while (isConnect)
        {
            if(socket.Available > 0)
            {
                msgLength = socket.Receive(receiveBytes);
                print("接收到消息，长度为" + msgLength);
                receiveQueue.Enqueue(receiveBytes);
            }
        }
    }

    private void ReadingMsgType(byte[] msg)
    {
        int msgId = BitConverter.ToInt32(msg, 0);
        Debug.Log("msgId = " + msgId);
        switch (msgId)
        {
            case 1001:
                ReadingChatMsg(msg);
                break;
        }
    }

    private void ReadingChatMsg(byte[] msg)
    {
        ChatMsg chatMsg = new ChatMsg();
        chatMsg.ReadingData(msg, 4);
        chatPanel.UpdateChatInfo(chatMsg);
    }

    public void Close()
    {
        if (socket != null && isConnect)
        {

            string disconnectMsg = "客户端断开连接";
            byte[] buffer = Encoding.UTF8.GetBytes(disconnectMsg);
            socket.Send(buffer);

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            isConnect = false;
        }
    }

    private void OnDestroy()
    {
        Close();
    }
}

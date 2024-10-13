using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System;
using System.Linq;
using Google.Protobuf;
using Protocols;

public class NetManager : MonoBehaviour
{
    public static NetManager Instance => instance;
    private static NetManager instance;

    private Socket socket;
    private Queue<byte[]> sendQueue = new Queue<byte[]>();
    private Queue<byte[]> receiveQueue = new Queue<byte[]>();
    private byte[] receiveBytes = new byte[1024 * 1024];


    private Coroutine heartbeatCoroutine;
    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    private const float HeartbeatInterval = 5f; // 心跳包发送间隔
    private const float HeartbeatTimeout = 3f; // 心跳包确认超时
    private const int HeartbeatMsgId = 2001; // 心跳包消息ID

    private enum ConnectionStatus { Connected, Disconnected, Unknown }
    private ConnectionStatus connectionStatus = ConnectionStatus.Unknown;

    private int reconnectCount = 0; // 当前重连次数
    private const int maxReconnectAttempts = 3; // 最大重连次数
    private int currentMessageId = 1; // 消息ID计数器

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
    }

    void Update()
    {
        ProcessReceivedMessages();

        // 检查连接状态
        if (connectionStatus == ConnectionStatus.Disconnected)
        {
            Debug.LogWarning("连接已断开，请尝试重新连接。");
        }
    }

    public void ConnectServer()
    {
        const string ip = "127.0.0.1"; // 使用本地测试
        const int port = 8080;

        if (connectionStatus == ConnectionStatus.Connected) return;

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            connectionStatus = ConnectionStatus.Connected;
            reconnectCount = 0;

            Task.Run(SendMessagesAsync);
            Task.Run(ReceiveMessagesAsync);

            heartbeatCoroutine = StartCoroutine(SendHeartbeat());
        }
        catch (SocketException e)
        {
            HandleConnectionError(e);
        }
    }

    // 发送消息
    public void SendMessage(MsgType messageType, byte[] data, string senderId)
    {
        var message = new BaseMessage
        {
            MessageId = currentMessageId++, // 获取唯一消息ID并递增
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), // 获取当前时间戳
            SenderId = senderId, // 设置发送者ID
            Type = messageType,
            Data = ByteString.CopyFrom(data)
        };

        byte[] messageBytes = message.ToByteArray();

        lock (sendQueue)
        {
            sendQueue.Enqueue(messageBytes);
        }
    }

    private async Task SendMessagesAsync()
    {
        while (connectionStatus == ConnectionStatus.Connected)
        {
            byte[] messageToSend = null;
            lock (sendQueue)
            {
                if (sendQueue.Count > 0)
                    messageToSend = sendQueue.Dequeue();
            }

            if (messageToSend != null)
                await socket.SendAsync(new ArraySegment<byte>(messageToSend), SocketFlags.None);
        }
    }

    private async Task ReceiveMessagesAsync()
    {
        while (connectionStatus == ConnectionStatus.Connected)
        {
            int msgLength = await socket.ReceiveAsync(new ArraySegment<byte>(receiveBytes), SocketFlags.None);
            if (msgLength > 0)
            {
                lock (receiveQueue)
                {
                    receiveQueue.Enqueue(receiveBytes.Take(msgLength).ToArray());
                }
            }
            else
            {
                HandleDisconnected();
            }
        }
    }

    private void ProcessReceivedMessages()
    {
        lock (receiveQueue)
        {
            while (receiveQueue.Count > 0)
            {
                ReadingMsgType(receiveQueue.Dequeue());
            }
        }
    }

    private void ReadingMsgType(byte[] msg)
    {
        // 这里假设消息ID在前4个字节
        int msgId = BitConverter.ToInt32(msg, 0);
        Debug.Log("msgId = " + msgId);
        switch (msgId)
        {
            case HeartbeatMsgId:
                Debug.Log("收到心跳包确认响应");
                connectionStatus = ConnectionStatus.Connected; // 更新连接状态
                break;
            // 添加其他消息类型处理
        }
    }

    private IEnumerator SendHeartbeat()
    {
        while (connectionStatus == ConnectionStatus.Connected)
        {
            sendQueue.Enqueue(BitConverter.GetBytes(HeartbeatMsgId));
            Debug.Log("发送心跳包");

            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(HeartbeatTimeout));
            yield return new WaitForSeconds(HeartbeatInterval);
        }
    }

    private void HandleConnectionError(SocketException e)
    {
        Debug.LogError($"连接错误: {e.ErrorCode} {e.Message}");
        connectionStatus = ConnectionStatus.Disconnected;
    }

    private void HandleDisconnected()
    {
        connectionStatus = ConnectionStatus.Disconnected;
        Debug.LogWarning("连接已断开");
    }

    public void Close()
    {
        if (socket == null || connectionStatus != ConnectionStatus.Connected) return;

        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        connectionStatus = ConnectionStatus.Disconnected;

        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        Close();
        cancellationTokenSource.Cancel();
    }
}

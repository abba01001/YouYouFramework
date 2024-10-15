using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
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

    private const float heartbeatInterval = 10f; // 心跳包发送间隔
    private const float heartbeatTimeout = 3f; // 心跳包确认超时

    private enum ConnectionStatus { Connected, Disconnected, Unknown }
    private ConnectionStatus connectionStatus = ConnectionStatus.Unknown;

    private int reconnectCount = 0; // 当前重连次数
    private const int maxReconnectAttempts = 3; // 最大重连次数
    private int currentMessageId = 1; // 消息ID计数器

    public NetLogger Logger;
    public RequestHandler Requset;
    public ResponseHandler Response;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Logger = new NetLogger(TimeSpan.FromSeconds(5));
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        ProcessReceivedMessages();
        // 检查连接状态
        if (connectionStatus == ConnectionStatus.Disconnected)
        {
            Debug.LogWarning("连接已断开，请尝试重新连接。");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Requset.c2s_request_item_info();
        }
    }

    public void ConnectServer()
    {
        const string ip = "127.0.0.1"; // 使用本地测试
        const int port = 8080;

        if (connectionStatus == ConnectionStatus.Connected) return;
        reconnectCount = 0; // 重置重连次数
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Requset = new RequestHandler(socket);
        Response = new ResponseHandler(socket,this.Requset);

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
                ReceiveMsg(receiveQueue.Dequeue());
            }
        }
    }

    public void EnqueueMsg(byte[] messageBytes)
    {
        lock (sendQueue)
        {
            sendQueue.Enqueue(messageBytes);
        }
    }

// 接收消息
    public void ReceiveMsg(byte[] msgBytes)
    {
        if (socket == null || msgBytes == null || msgBytes.Length == 0) return;
        int msgLength = msgBytes.Length;

        if (msgLength > 0)
        {
            byte[] tempMsg = new byte[msgLength];
            Array.Copy(msgBytes, tempMsg, msgLength); // 将接收到的数据复制到 tempMsg
            try
            {
                BaseMessage receivedMsg = BaseMessage.Parser.ParseFrom(tempMsg); // 解析收到的消息
                Response.HandleResponse(receivedMsg);
            }
            catch (Exception e)
            {
                GameUtil.LogError($"ReceiveClientMsg Exception: {e.Message}");
            }
        }
    }


    private IEnumerator SendHeartbeat()
    {
        while (connectionStatus == ConnectionStatus.Connected)
        {
            Requset.c2s_request_heart_beat();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(heartbeatTimeout));
            yield return new WaitForSeconds(heartbeatInterval);
        }
    }

    private void HandleConnectionError(SocketException e)
    {
        Logger.LogMessage(socket, $"连接错误: {e.ErrorCode} {e.Message}");
        connectionStatus = ConnectionStatus.Disconnected;
    }

    private async void HandleDisconnected()
    {
        connectionStatus = ConnectionStatus.Disconnected;
        Logger.LogMessage(socket, "连接已断开");

        if (reconnectCount < maxReconnectAttempts)
        {
            reconnectCount++;
            Debug.Log($"尝试重连... 第 {reconnectCount} 次");
        
            await Task.Delay(2000); // 等待 2 秒后重连
            ConnectServer();
        }
        else
        {
            Debug.LogWarning("已达到最大重连次数。");
        }
    }


    private void Close()
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

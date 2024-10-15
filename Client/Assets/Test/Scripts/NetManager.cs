using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protocols;
using Protocols.Item;

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

    private const float HeartbeatInterval = 10f; // 心跳包发送间隔
    private const float HeartbeatTimeout = 3f; // 心跳包确认超时
    private const int HeartbeatMsgId = 2001; // 心跳包消息ID

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
            Requset = new RequestHandler(socket);
            Response = new ResponseHandler(socket);
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
    
    public void ReceiveMsg(byte[] msg)
    {
        if (socket == null) return;

        try
        {
            if (msg.Length > 0)
            {
                BaseMessage receivedMsg = BaseMessage.Parser.ParseFrom(msg);
                HandleMessage(receivedMsg);
                Response.HandleResponse(receivedMsg);
            }
            else
            {
                // 如果 msg 为空或长度为0，可以选择记录日志或处理连接关闭
                Logger.LogMessage(socket,"收到空消息，关闭连接。");
                Close();
            }
        }
        catch (SocketException e)
        {
            Logger.LogMessage(socket,$"ReceiveMsg SocketException: {e.Message}");
            Close();
        }
        catch (Exception e)
        {
            Logger.LogMessage(socket,$"ReceiveMsg Exception: {e.Message}");
        }
    }

    
    private void HandleMessage(BaseMessage message)
    {

        Logger.LogMessage(socket,$"收到的消息类型: {message.Type}");
        // 根据消息类型解包成不同的数据结构
        switch (message.Type)
        {
            case MsgType.Hello:
                // 假设 Hello 消息是 ItemData 类型
                ProtocolHelper.UnpackData<ItemData>(message, (itemData) =>
                {
                    Logger.LogMessage(socket,$"解包成功: Item ID: {itemData.ItemId}, Item Name: {itemData.ItemName}");
                });
                break;
            case MsgType.Exit:
                Logger.LogMessage(socket,$"收到EXIT消息: {message.MessageId}");
                break;
            default:
                Logger.LogMessage(socket,$"收到未知消息类型: {message.Type}");
                break;
        }
    }
    
    private IEnumerator SendHeartbeat()
    {
        while (connectionStatus == ConnectionStatus.Connected)
        {
            Requset.c2s_request_heart_beat();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(HeartbeatTimeout));
            yield return new WaitForSeconds(HeartbeatInterval);
        }
    }

    private void HandleConnectionError(SocketException e)
    {
        Logger.LogMessage(socket,$"连接错误: {e.ErrorCode} {e.Message}");
        connectionStatus = ConnectionStatus.Disconnected;
    }

    private void HandleDisconnected()
    {
        connectionStatus = ConnectionStatus.Disconnected;
        Logger.LogMessage(socket,"连接已断开");
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

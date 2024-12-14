using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using Main;
using Protocols;
using YouYou;

public class NetManager
{
    private Socket socket;
    private Queue<byte[]> sendQueue = new Queue<byte[]>();
    private Queue<byte[]> receiveQueue = new Queue<byte[]>();
    private byte[] receiveBytes = new byte[1024 * 1024];

    private TimeAction heartbeatAction;
    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    
    private int CurReconnectCount; // 当前重连次数
    private bool IsReconnect;
    private int HeartInterval => MainEntry.ParamsSettings.GetGradeParamData("HeartInterval");
    private int ConnectInterval => MainEntry.ParamsSettings.GetGradeParamData("ConnectInterval");
    private int MaxReconnect => MainEntry.ParamsSettings.GetGradeParamData("MaxReconnect");
    
    private enum ConnectionStatus { Connected, Disconnected, Unknown }
    private ConnectionStatus connectionStatus = ConnectionStatus.Unknown;

    
    public NetLogger Logger;
    public NetRequestHandler Requset;
    public NetResponseHandler NetResponse;

    public void Init()
    {
        //Logger = new NetLogger(TimeSpan.FromSeconds(5));
        //Main.MainEntry.ParamsSettings.GetGradeParamData("MobileDebug")

    }

    public void OnUpdate()
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

    public async Task ConnectServerAsync()
    {
        const string ip = "127.0.0.1"; // 使用本地测试
        const int port = 8080;

        if (connectionStatus == ConnectionStatus.Connected) return;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Requset = new NetRequestHandler(socket);
        NetResponse = new NetResponseHandler(socket, this.Requset);

        try
        {
            await socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port)); // 异步连接
            GameEntry.Log(LogCategory.NetWork, "连接服务器成功");
            Requset.UpdateSenderId();
            connectionStatus = ConnectionStatus.Connected;
            CurReconnectCount = 0;
            Task.Run(SendMessagesAsync);
            Task.Run(ReceiveMessagesAsync);
            heartbeatAction?.Stop();
            heartbeatAction = GameEntry.Time.CreateTimerLoop(this, 1f, -1, SendHeartbeat);
        }
        catch (SocketException e)
        {
            HandleConnectionError(e);
        }
    }


    public void DisConnectServer()
    {
        // 检查是否已经连接
        if (connectionStatus != ConnectionStatus.Connected)
        {
            GameUtil.LogError("未连接到服务器，无需断开。");
            return;
        }
        try
        {
            // 更新连接状态
            connectionStatus = ConnectionStatus.Disconnected;
            // 停止心跳协程（如果有）
            heartbeatAction?.Stop();
            // 关闭 Socket
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both); // 停止发送和接收数据
                socket.Close(); // 释放 Socket 资源
                socket = null;
            }
            // 日志提示
            GameUtil.LogError("服务器连接已断开。");
        }
        catch (Exception e)
        {
            GameUtil.LogError($"断开服务器时发生异常: {e.Message}");
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
                {
                    messageToSend = sendQueue.Dequeue();
                }
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
                NetResponse.HandleResponse(receivedMsg);
            }
            catch (Exception e)
            {
                GameUtil.LogError($"ReceiveClientMsg Exception: {e.Message}");
            }
        }
    }


    private void SendHeartbeat(int second)
    {
        if (second % HeartInterval == 0)
        {
            Requset.c2s_request_heart_beat();
        }
    }


    private void HandleConnectionError(SocketException e)
    {
        //Logger.LogMessage(socket, $"连接错误: {e.ErrorCode} {e.Message}");
        connectionStatus = ConnectionStatus.Disconnected;
    }

    public async void HandleDisconnected()
    {
        connectionStatus = ConnectionStatus.Disconnected;

        while (CurReconnectCount < MaxReconnect)
        {
            if (!IsReconnect)
            {
                IsReconnect = true;
                //弹出重新连接转圈UI
                GameEntry.UI.OpenUIForm<FormCircle>();
            }
            CurReconnectCount++;
            GameUtil.LogError($"尝试重连... 第 {CurReconnectCount} 次");
            await ConnectServerAsync(); // 异步连接
            // 如果连接成功，就退出重连逻辑
            if (connectionStatus == ConnectionStatus.Connected)
            {
                IsReconnect = false;
                //关闭转圈UI
                GameEntry.UI.CloseUIForm<FormCircle>();
                GameUtil.LogError("连接成功，退出重连逻辑");
                return;
            }
            await UniTask.Delay(ConnectInterval * 1000);
        }
        Debug.LogWarning("已达到最大重连次数，停止重连。");
    }



    private void Close()
    {
        if (socket == null || connectionStatus != ConnectionStatus.Connected) return;

        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        connectionStatus = ConnectionStatus.Disconnected;
        heartbeatAction?.Stop();
    }

    private void OnDestroy()
    {
        Close();
        cancellationTokenSource.Cancel();
    }
}

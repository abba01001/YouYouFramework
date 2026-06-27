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
using GameScripts;
using Main;
using Protocols;
using UniRx;

namespace GameScripts
{
    [MonoSingletonPath("[Singleton]/NetManager")]
    public class NetManager: MonoBehaviour, ISingleton
    {
        public static NetManager Instance => MonoSingletonProperty<NetManager>.Instance;
        public void OnSingletonInit()
        {
            Init();
        }
        public void OnSingletonDispose() { }
        
        private Socket socket;
        private Queue<byte[]> sendQueue = new Queue<byte[]>();
        private Queue<byte[]> receiveQueue = new Queue<byte[]>();
        private byte[] receiveBytes = new byte[Constants.ProtocalTotalLength];
    
        private System.Threading.Timer heartBeatTimer;
        private System.Threading.Timer netTimeTimer;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource reconnectTokenSource;

        private int CurReconnectCount; // 当前重连次数
        private bool IsReconnect;
        private int HeartTimeout => GameEntry.ParamsSettings.GetGradeParamData("HeartTimeout");
        private int HeartInterval => GameEntry.ParamsSettings.GetGradeParamData("HeartInterval");
        private int ConnectInterval => GameEntry.ParamsSettings.GetGradeParamData("ConnectInterval");
        private int MaxReconnect => GameEntry.ParamsSettings.GetGradeParamData("MaxReconnect");
    
        private enum ConnectionStatus
        {
            Connected,
            Disconnected,
            Unknown
        }
    
        private ConnectionStatus connectionStatus = ConnectionStatus.Unknown;
    
        private string _token = string.Empty;
    
        public string Token
        {
            get => _token;
            set { _token = value; }
        }
    
        public bool IsConnectServer
        {
            get { return connectionStatus == ConnectionStatus.Connected && Constants.IsEntryGame; }
        }
    
        public NetLogger Logger;
        public NetRequestHandler Requset;
        public NetResponseHandler NetResponse;
        public HandleSubPack handleSubPack = new HandleSubPack();
    
        public void Init()
        {
            //Logger = new NetLogger(TimeSpan.FromSeconds(5));
        }
    
        private static long lastSendHeartAckTime;
        private static long lastReceiveHeartAckTime;
        private static long serverStartTimestamp;
        private static long clientStartTimestamp;
    
        public static long CurrentServerTimestamp =>
            serverStartTimestamp + (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - clientStartTimestamp);
    
        public void InitData(string token, long time)
        {
            Token = token;
            serverStartTimestamp = time;
            clientStartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    
        public long NetDelay => Mathf.Min(Mathf.Abs((int)lastReceiveHeartAckTime - (int)lastSendHeartAckTime), 460);
    
        public long GetNetDelay()
        {
            return lastReceiveHeartAckTime - lastSendHeartAckTime;
        }
    
        public void RefreshLastHeartAckTime(long time)
        {
            lastReceiveHeartAckTime = time;
        }
    
        public void RefreshLastSendHeartAckTime()
        {
            lastSendHeartAckTime = CurrentServerTimestamp;
        }
    
        private void CheckHeartTimeout()
        {
            if (!IsConnectServer || lastReceiveHeartAckTime <= 0) return;
            long now = CurrentServerTimestamp;
            if (now - lastReceiveHeartAckTime >= HeartTimeout && !IsReconnect)
            {
                Debugger.LogError("心跳超时，断线重连");
                HandleDisconnected();
            }
        }
    
        private float lastLogTime = 0f;
        private const float logInterval = 1.0f; // 打印间隔：1秒
        public void Update()
        {
            ProcessReceivedMessages();
            CheckHeartTimeout();
            // 检查连接状态
            if (connectionStatus == ConnectionStatus.Disconnected)
            {
                // 使用 Time.time 判断是否达到 1 秒的间隔
                if (Time.time - lastLogTime >= logInterval)
                {
                    Debugger.LogWarning("连接已断开，请尝试重新连接。");
                    lastLogTime = Time.time; // 重置计时器
                }
            }
    
            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     Requset.c2s_request_item_info();
            // }
        }
    
        public void DisConnectServer()
        {
            // 检查是否已经连接
            if (connectionStatus != ConnectionStatus.Connected)
            {
                Debugger.Log("未连接到服务器，无需断开。");
                return;
            }
    
            try
            {
                // 更新连接状态
                connectionStatus = ConnectionStatus.Disconnected;
                heartBeatTimer?.Dispose();
                if (socket != null)
                {
                    socket.Shutdown(SocketShutdown.Both); // 停止发送和接收数据
                    socket.Close(); // 释放 Socket 资源
                    socket = null;
                    Debugger.Log("服务器连接已断开。");
                }
            }
            catch (Exception e)
            {
                Debugger.LogError($"断开服务器时发生异常: {e.Message}");
            }
        }
    
        private async Task SendMessagesAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && connectionStatus == ConnectionStatus.Connected)
            {
                byte[] messageToSend = null;
    
                lock (sendQueue)
                {
                    if (sendQueue.Count > 0)
                        messageToSend = sendQueue.Dequeue();
                }
    
                if (messageToSend != null)
                {
                    await socket.SendAsync(new ArraySegment<byte>(messageToSend), SocketFlags.None);
                }
            }
        }
    
        private async Task ReceiveMessagesAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && connectionStatus == ConnectionStatus.Connected)
            {
                int msgLength = await socket.ReceiveAsync(
                    new ArraySegment<byte>(receiveBytes), SocketFlags.None);
    
                if (msgLength > 0)
                {
                    lock (receiveQueue)
                        receiveQueue.Enqueue(receiveBytes.Take(msgLength).ToArray());
                }
                else
                {
                    Debugger.LogError("收到消息为空的内容");
                    // HandleDisconnected();
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
                    Protocol receivedMsg = Protocol.Parser.ParseFrom(tempMsg); // 解析收到的消息
                    // 处理接收到的消息并返回最终的完整消息
                    BaseMessage finalMessage = handleSubPack.ProcessSubPack(receivedMsg);
                    if (finalMessage != null)
                    {
                        NetResponse.HandleResponse(finalMessage); // 处理完整的消息
                    }
                }
                catch (Exception e)
                {
                    Debugger.LogError($"ReceiveClientMsg Exception: {e.Message}");
                }
            }
        }
    
        private void HandleConnectionError(SocketException e)
        {
            connectionStatus = ConnectionStatus.Disconnected;
        }
    
        public async Task ConnectServerAsync(bool isReconnect)
        {
            string url = HotfixManager.Instance.GetServerIP();
            string[] urls = url.Split(Converter.FifthSeparator);
    
            if (connectionStatus == ConnectionStatus.Connected) return;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Requset = new NetRequestHandler(socket);
            NetResponse = new NetResponseHandler(socket, this.Requset);
    
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(urls[0]), int.Parse(urls[1]));
                Debugger.Log(isReconnect ? "重新连接服务器地址" : "开始连接服务器地址", "===>", ipEndPoint);
                await socket.ConnectAsync(ipEndPoint); // 异步连接
                Requset.UpdateSenderId();
                connectionStatus = ConnectionStatus.Connected;
                CurReconnectCount = 0;
                RefreshLastHeartAckTime(0);
                cancellationTokenSource = new CancellationTokenSource();
                Task.Run(() => SendMessagesAsync(cancellationTokenSource.Token));
                Task.Run(() => ReceiveMessagesAsync(cancellationTokenSource.Token));
    
                heartBeatTimer?.Dispose();
                heartBeatTimer = new System.Threading.Timer(_ => { Requset.c2s_request_heart_beat(); }, null, 0,
                    HeartInterval * 1000);
    
                Debugger.Log("连接服务器地址成功===>",ipEndPoint.ToString());
            }
            catch (SocketException e)
            {
                HandleConnectionError(e);
            }
        }

        public async UniTask HandleDisconnected()
        {
            // 1. 重入保护
            if (reconnectTokenSource != null) return;

            connectionStatus = ConnectionStatus.Disconnected;
            reconnectTokenSource = new CancellationTokenSource();
            var token = reconnectTokenSource.Token;

            try
            {
                while (!token.IsCancellationRequested && CurReconnectCount < MaxReconnect)
                {
                    if (!IsReconnect)
                    {
                        IsReconnect = true;
                        GameEntry.UI.OpenUIForm<FormCircle>();
                    }

                    CurReconnectCount++;
                    Debugger.LogError($"尝试重连... 第 {CurReconnectCount} 次");

                    // 2. 绑定 Token，确保能被取消
                    await ConnectServerAsync(true);

                    if (connectionStatus == ConnectionStatus.Connected)
                    {
                        Debugger.LogError("连接成功，退出重连逻辑");
                        return; // 跳转到 finally 处理收尾
                    }

                    // 3. 绑定 Token，确保延时期间也能被取消
                    await UniTask.Delay(ConnectInterval * 1000, cancellationToken: token);
                }

                if (CurReconnectCount >= MaxReconnect)
                    Debugger.LogWarning("已达到最大重连次数，停止重连。");
            }
            catch (OperationCanceledException)
            {
                Debugger.Log("重连逻辑已取消");
            }
            finally
            {
                // 4. 统一收尾（无论成功、失败还是取消）
                IsReconnect = false;
                GameEntry.UI.CloseUIForm<FormCircle>();

                reconnectTokenSource?.Dispose();
                reconnectTokenSource = null;
            }
        }
 
        private void Close()
        {
            try
            {
                reconnectTokenSource?.Cancel();
                cancellationTokenSource?.Cancel();

                heartBeatTimer?.Dispose();
                heartBeatTimer = null;

                if (socket != null)
                {
                    // 💡 核心修改：检查 Connected 属性并增加 Socket 状态判断
                    // 注意：socket.Connected 在 Socket 断开后不一定会立即返回 false，
                    // 但如果 socket 处于断开状态，Shutdown 会报错。
                    try 
                    {
                        // 只有当 Socket 确实还处于连接状态时才执行 Shutdown
                        if (socket.Connected)
                        {
                            socket.Shutdown(SocketShutdown.Both);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debugger.LogWarning($"Socket Shutdown failed: {ex.Message}");
                    }
                    finally
                    {
                        socket.Close();
                        socket.Dispose();
                        socket = null;
                    }
                }

                connectionStatus = ConnectionStatus.Disconnected;
            }
            catch (Exception e)
            {
                Debugger.LogError($"Close Exception: {e.Message}");
            }
        }
    
        public void OnDestroy()
        {
            Close();
        }
    }
}
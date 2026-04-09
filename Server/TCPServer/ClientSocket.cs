using Google.Protobuf;
using Protocols;
using System.Net.Sockets;
using System.Threading.Tasks;
using System;
using System.Timers;
using TCPServer.Core.Services;
using System.Net;
using TCPServer.Core;

public class ClientSocket
{
    public static int CLIENT_BEGIN_ID = 1;
    public static int CLIENT_COUNT = 0;
    public int clientID;
    public Socket socket;
    public DateTime LastHeartbeatTime { get; set; } = DateTime.UtcNow;
    public RequestHandler Request;
    public ResponseHandler Response;
    public int heartbeatTimeout = 15;
    private byte[] msgBytes; // 将缓冲区作为类的成员变量
    private Timer heartbeatTimer; // 定时器
    public string UserAccount { get; set; } = string.Empty;
    public string UserUUID { get; set; } = string.Empty;
    public ClientSocket(Socket clientSocket)
    {
        this.socket = clientSocket;
        this.msgBytes = new byte[Constants.ProtocalTotalLength]; // 初始化缓冲区
        this.Request = new RequestHandler(this);
        this.Response = new ResponseHandler(this, this.Request);
        this.clientID = CLIENT_BEGIN_ID++;
        CLIENT_COUNT++;

        // 初始化心跳检测定时器
        heartbeatTimer = new Timer(1000); // 每秒检查一次
        heartbeatTimer.Elapsed += (sender, e) => CheckConnect();
        heartbeatTimer.Start();
    }

    public async Task SendMsg(byte[] msg)
    {
        if (socket == null)
        {
            return; // 直接返回，避免继续执行
        }
        try
        {
            await socket.SendAsync(new ArraySegment<byte>(msg), SocketFlags.None);
        }
        catch (SocketException e)
        {
            LoggerHelper.Instance.Error($"发送消息异常: {e.Message}");
        }
        catch (Exception e)
        {
            LoggerHelper.Instance.Error($"发送消息异常: {e.Message}");
        }
    }

    // 发送消息
    public async Task SendMessage<T>(MsgType messageType, T data) where T : IMessage<T>
    {
        // 将数据对象序列化为字节数组
        byte[] byteArrayData = data.ToByteArray();
        var message = new BaseMessage
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), // 获取当前时间戳
            SenderId = socket.RemoteEndPoint.ToString(), // 设置发送者ID
            MsgType = messageType,
            Type = typeof(T).Name,
            Data = ByteString.CopyFrom(byteArrayData) // 直接将序列化后的字节数组放入 Data
        };
        byte[] messageBytes = message.ToByteArray();
        await SendMsg(messageBytes);
    }

    // 关闭连接时移除用户
    public void Close()
    {
        try
        {
            CLIENT_COUNT--;
            // 从 OnlineUsers 中移除玩家信息
            if (!string.IsNullOrEmpty(UserAccount))
            {
                AccountService.RefreshOnlineUsers(2, UserAccount);
            }
            if(socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
            }
        }
        catch (SocketException) { /* 处理已关闭的socket */ }
        finally
        {
            if(socket != null)
            {
                socket.Close();
                socket = null;
                ServerSocket.CleanClient(this);
            }
        }
    }

    // 接收消息
    public async Task ReceiveMsgAsync()
    {
        if (socket == null || !socket.Connected) return;
        try
        {
            Array.Clear(msgBytes, 0, msgBytes.Length);
            int msgLength = await socket.ReceiveAsync(new ArraySegment<byte>(msgBytes), SocketFlags.None);
            if (msgLength > 0)
            {
                byte[] tempMsg = new byte[msgLength];
                Array.Copy(msgBytes, tempMsg, msgLength);
                try
                {
                    Protocol receivedMsg = Protocol.Parser.ParseFrom(tempMsg); // 解析收到的消息
                    BaseMessage finalMessage = ServerSocket.handleSubPack.ProcessSubPack(receivedMsg);
                    LoggerHelper.Instance.Debug($"接收消息id==={receivedMsg.MessageId}==包索引{receivedMsg.PacketIndex}==包数{receivedMsg.PacketTotal}==协议长度{msgLength}");
                    if (finalMessage != null)
                    {
                        Response.HandleResponse(finalMessage); // 处理完整的消息
                    }
                }
                catch (Exception e)
                {
                    LoggerHelper.Instance.Error($"接收消息异常: {e.Message}");
                }
            }
        }
        catch (Exception e)
        {
            LoggerHelper.Instance.Error($"接收消息异常: {e.Message}");
        }
    }

    public void CheckConnect()
    {
        lock (this)
        {
            if ((DateTime.UtcNow - LastHeartbeatTime).TotalSeconds > heartbeatTimeout)
            {
                if(socket != null) LoggerHelper.Instance.Info($"客户端{socket.RemoteEndPoint.ToString()}超时断开");
                Close(); // 超时处理时关闭连接
                heartbeatTimer.Stop(); // 停止定时器
                heartbeatTimer.Dispose(); // 释放定时器资源
            }
        }
    }
}
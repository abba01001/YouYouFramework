using Google.Protobuf;
using Protocols;
using System.Net.Sockets;
using System.Threading.Tasks;
using System;
using System.Timers;

public class ClientSocket
{
    public static int CLIENT_BEGIN_ID = 1;
    public static int CLIENT_COUNT = 0;
    public int clientID;
    private Socket socket;
    public DateTime LastHeartbeatTime { get; private set; } = DateTime.UtcNow;
    public RequestHandler Request;
    public ResponseHandler Response;
    public int heartbeatTimeout = 60;
    private byte[] msgBytes; // 将缓冲区作为类的成员变量
    private const int BufferSize = 1024; // 定义缓冲区大小
    private Timer heartbeatTimer; // 定时器

    public ClientSocket(Socket clientSocket)
    {
        this.socket = clientSocket;
        this.msgBytes = new byte[BufferSize]; // 初始化缓冲区
        this.Request = new RequestHandler(socket);
        this.Response = new ResponseHandler(socket, this.Request);
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
            Console.WriteLine($"SendMsg SocketException: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"SendMsg Exception: {e.Message}");
        }
    }

    // 发送消息
    public async Task SendMessage<T>(MsgType messageType, T data) where T : IMessage<T>
    {
        ServerSocket.Logger.LogMessage(this.socket, $"{data.ToString()}");
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

    public void Close()
    {
        try
        {
            CLIENT_COUNT--;
            Console.WriteLine("IP:{0}断开...已连接IP数{1}", socket.RemoteEndPoint.ToString(), CLIENT_COUNT);
            socket.Shutdown(SocketShutdown.Both);
        }
        catch (SocketException) { /* 处理已关闭的socket */ }
        finally
        {
            socket.Close();
            socket = null;
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
                    BaseMessage receivedMsg = BaseMessage.Parser.ParseFrom(tempMsg); // 解析收到的消息
                    if (receivedMsg.MsgType == MsgType.HeartBeat)
                    {
                        LastHeartbeatTime = DateTime.UtcNow; // 更新心跳时间
                        //return; // 心跳消息无需进一步处理
                    }
                    Response.HandleResponse(receivedMsg);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ReceiveClientMsg Exception: {e.Message}");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"ReceiveMsg Exception: {e.Message}");
        }
    }

    public void CheckConnect()
    {
        if (socket == null || !socket.Connected) return;
        lock (this)
        {
            if ((DateTime.UtcNow - LastHeartbeatTime).TotalSeconds > heartbeatTimeout)
            {
                ServerSocket.Logger.LogMessage(this.socket, $"客户端{socket.RemoteEndPoint.ToString()}超时断开");
                Close();
            }
        }
    }
}
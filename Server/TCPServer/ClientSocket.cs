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
    public int clientID;
    public Socket socket;
    public long LastHeartbeatTime { get; set; }
    public RequestHandler Request;
    public ResponseHandler Response;
    public int HeartTimeout = 15;
    private byte[] msgBytes;
    public string UserAccount { get; set; } = string.Empty;
    public string UserUUID { get; set; } = string.Empty;

    // 标记是否已销毁，防止重复调用
    private bool _isDestroyed = false;

    public ClientSocket(Socket clientSocket)
    {
        this.socket = clientSocket;
        this.msgBytes = new byte[Constants.ProtocalTotalLength];
        this.Request = new RequestHandler(this);
        this.Response = new ResponseHandler(this, this.Request);
    }

    // 关闭连接时移除用户
    public void Close()
    {
        // 已销毁则直接返回
        if (_isDestroyed) return;

        try
        {
            if (socket != null && socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
            }
        }
        catch (SocketException) { }
        finally
        {
            // 统一调用销毁方法（核心）
            OnDestroy();
        }
    }

    /// <summary>
    /// 销毁当前类：释放所有资源 + 清空引用 + 销毁实例
    /// </summary>
    public async Task OnDestroy()
    {
        // 防止重复销毁
        if (_isDestroyed) return;
        _isDestroyed = true;

        try
        {
            // 2. 关闭并清空Socket（释放网络资源）
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }

            // 3. 从服务器客户端列表中移除自己（断开引用）
            ServerSocket.CleanClient(this);
            await PlayerService.OnLogout(UserUUID);
            
            // 4. 清空所有成员变量/引用（让GC回收内存）
            clientID = 0;
            LastHeartbeatTime = 0;
            UserAccount = string.Empty;
            UserUUID = string.Empty;
            msgBytes = null;

            // 清空业务类引用
            Request = null;
            Response = null;

            LoggerHelper.Instance.Info($"客户端实例已销毁：{this.GetHashCode()}");
        }
        catch (Exception e)
        {
            LoggerHelper.Instance.Error($"销毁客户端异常: {e.Message}");
        }
    }

    // 接收消息
    public async Task ReceiveMsgAsync()
    {
        // 已销毁直接退出
        if (_isDestroyed || socket == null || !socket.Connected) return;

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
                    Protocol receivedMsg = Protocol.Parser.ParseFrom(tempMsg);
                    BaseMessage finalMessage = ServerSocket.handleSubPack.ProcessSubPack(receivedMsg);
                    // LoggerHelper.Instance.Debug($"接收消息id==={receivedMsg.MessageId}==包索引{receivedMsg.PacketIndex}==包数{receivedMsg.PacketTotal}==协议长度{msgLength}");
                    if (finalMessage != null)
                    {
                        Response.HandleResponse(finalMessage);
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
            // 接收异常直接销毁当前类
            Close();
        }
    }

    public void CheckConnect()
    {
        // 已销毁直接退出
        if (_isDestroyed || LastHeartbeatTime == 0) return;

        lock (this)
        {
            if (ServerSocket.CurrentServerTimestamp - LastHeartbeatTime > HeartTimeout)
            {
                if (socket != null)
                    LoggerHelper.Instance.Info($"客户端{socket.RemoteEndPoint.ToString()}超时断开");

                // 超时后销毁当前实例
                Close();
            }
        }
    }
}
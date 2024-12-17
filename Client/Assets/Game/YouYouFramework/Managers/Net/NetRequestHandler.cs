using System;
using System.Net.Sockets;
using Google.Protobuf;
using Protocols;
using Protocols.Guild;
using YouYou;


public class NetRequestHandler
{
    private Socket socket;
    private string senderId;
    public NetRequestHandler(Socket socket)
    {
        this.socket = socket;
    }

    public void UpdateSenderId()
    {
        this.senderId = socket.RemoteEndPoint.ToString();
    }
    
    private void SendMessage<T>(T data) where T : IMessage<T>
    {
        // 检查连接是否有效
        byte[] messageBytes = HandleMessage(data);
        if (socket == null || !socket.Connected)
        {
            GameUtil.LogError("服务器已断开,无法发送心跳包");
            GameEntry.Net.HandleDisconnected(); // 处理连接断开
            GameEntry.Net.EnqueueMsg(messageBytes);
            return; // 退出心跳发送循环
        }
        GameEntry.Net.EnqueueMsg(messageBytes);
    }

    private byte[] HandleMessage<T>(T data) where T : IMessage<T>
    {
        byte[] byteArrayData = data.ToByteArray();
        BaseMessage message = new BaseMessage();
        message.Type = typeof(T).Name;
        message.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();// 获取当前时间戳
        message.SenderId = this.senderId; // 设置发送者ID
        message.Data = ByteString.CopyFrom(byteArrayData); // 直接将序列化后的字节数组放入 Data
        GameUtil.LogError($"发送内容{message.ToJson()}");
        byte[] messageBytes = message.ToByteArray();
        return messageBytes;
    }

    #region 发送协议

    // 示例：心跳包请求，处理心跳数据的逻辑，返回消息对象
    public void c2s_request_heart_beat()
    {
        SendMessage(new HeartBeatMsg());
    }

    public void c2s_request_guild_list()
    {
        SendMessage(new GuildListMsg());
    }
    
    public void c2s_request_item_info()
    {
        Protocols.Item.ItemData data = new Protocols.Item.ItemData()
        {
            ItemId = "1",
            ItemDescription = "物品B",
            ItemName = "物品名字",
            ItemType = 3,
            Quantity = 5,
        };
        SendMessage(data);
    }
    #endregion
}

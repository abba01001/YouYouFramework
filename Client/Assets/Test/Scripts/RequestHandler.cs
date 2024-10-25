using System;
using System.Net.Sockets;
using Google.Protobuf;
using Protocols;
using Protocols.Item;


public class RequestHandler
{
    private Socket socket;
    public RequestHandler(Socket socket)
    {
        this.socket = socket;
    }
    private void SendMessage<T>(T data) where T : IMessage<T>
    {
        string json = data.ToJson();
        byte[] byteArrayData = data.ToByteArray();
        BaseMessage message = new BaseMessage();
        message.Type = typeof(T).Name;
        message.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();// 获取当前时间戳
        message.SenderId = socket.RemoteEndPoint.ToString(); // 设置发送者ID
        message.Data = ByteString.CopyFrom(byteArrayData); // 直接将序列化后的字节数组放入 Data
        NetManager.Instance.Logger.LogMessage(socket,$"发送内容:{json}");
        byte[] messageBytes = message.ToByteArray();
        NetManager.Instance.EnqueueMsg(messageBytes);
    }

    #region 发送协议

    // 示例：心跳包请求，处理心跳数据的逻辑，返回消息对象
    public void c2s_request_heart_beat()
    {
        HeartBeatMsg heartBeatMsg = new HeartBeatMsg
        {
            /* 初始化具体业务数据 */
        };
        Console.WriteLine("心跳包数据准备好了...");
        SendMessage(heartBeatMsg);
    }

    public void c2s_request_item_info()
    {
        ItemData data = new ItemData()
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

using System;
using System.Net.Sockets;
using System.Xml;
using Google.Protobuf;
using Newtonsoft.Json;
using Protocols;
using Protocols.Guild;
using Protocols.Item;
using TCPServer;
using TCPServer.Core.Services;

public class RequestHandler
{
    private Socket socket;
    public RequestHandler(Socket socket)
    {
        this.socket = socket;
    }
    private void SendMessage<T>(T data) where T : IMessage<T>
    {
        byte[] byteArrayData = data.ToByteArray();
        BaseMessage message = new BaseMessage();
        string typeName = typeof(T).Name;
        message.Type = typeof(T).Name;
        message.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();// 获取当前时间戳
        message.Data = ByteString.CopyFrom(byteArrayData); // 直接将序列化后的字节数组放入 Data

        string messageJson = JsonConvert.SerializeObject(message);
        ServerSocket.Logger.LogMessage(socket,$"发送内容{messageJson}");
        if (socket == null) return;
        message.SenderId = socket.RemoteEndPoint.ToString(); // 设置发送者ID
        byte[] messageBytes = message.ToByteArray();
        socket.Send(messageBytes);
    }

    #region 发送协议

    // 示例：心跳包请求，处理心跳数据的逻辑，返回消息对象
    public void c2s_request_heart_beat()
    {
        SendMessage(new HeartBeatMsg());
    }

    public async void c2s_request_guild_list()
    {
        GuildListMsg data = new GuildListMsg();
        data.GuildList = await GuildService.GetGuildList(1, 100);
        SendMessage(data);
    }

    public void c2s_request_login(int state,string user_uuid)
    {
        LoginMsg data = new LoginMsg();
        data.State = state;
        data.UserUuid = user_uuid;
        SendMessage(data);
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

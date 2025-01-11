using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml;
using Google.Protobuf;
using Newtonsoft.Json;
using Protocols;
using Protocols.Guild;
using Protocols.Item;
using Protocols.Player;
using TCPServer;
using TCPServer.Core;
using TCPServer.Core.Services;
using TCPServer.Utils;

public class RequestHandler
{
    private Socket socket;
    public RequestHandler(Socket socket)
    {
        this.socket = socket;
    }

    public async Task SendMessage<T>(T data) where T : IMessage<T>
    {
        byte[] byteArrayData = data.ToByteArray();
        BaseMessage message = new BaseMessage();
        string typeName = typeof(T).Name;
        message.Type = typeName;
        message.Timestamp = ServerSocket.CurrentServerTimestamp;
        message.Data = ByteString.CopyFrom(byteArrayData); // 直接将序列化后的字节数组放入 Data
        string messageJson = JsonConvert.SerializeObject(message);
        Console.WriteLine($"发送内容{messageJson}");

        if (socket == null) return;

        message.SenderId = socket.RemoteEndPoint.ToString(); // 设置发送者ID
        byte[] messageBytes = message.ToByteArray();

        // 异步发送数据
        await Task.Run(() => socket.Send(messageBytes)); // 使用 Task.Run 包装同步的 Send 操作
    }


    #region 发送协议

    // 示例：心跳包请求，处理心跳数据的逻辑，返回消息对象
    public void c2s_request_heart_beat()
    {
        SendMessage(new HeartBeatMsg());
    }

    public async void c2s_request_guild_list(int currentPage, int pageSize)
    {
        GuildListMsg data = new GuildListMsg();
        data.GuildList = await GuildService.GetGuildList(currentPage, pageSize);
        SendMessage(data);
    }

    public async void c2s_request_get_suspend_reward(SuspendTimeMsg data)
    {
        SendMessage(data);
    }

    public void c2s_request_login(int state,string user_uuid, byte[] save_data)
    {
        LoginMsg data = new LoginMsg();
        data.State = state;
        data.UserUuid = user_uuid;
        if(user_uuid != string.Empty)
        {
            data.Token = JwtHelper.GenerateToken(data.UserUuid, "测试");
        }
        data.SaveData = ByteString.CopyFrom(save_data);
        Console.WriteLine($"返回数据{Convert.ToBase64String(save_data)}");
        SendMessage(data);
    }

    public void c2s_request_register(int state,string user_uuid)
    {
        RegisterMsg data = new RegisterMsg();
        data.State = state;
        data.UserUuid = user_uuid;
        if (user_uuid != string.Empty)
        {
            data.Token = JwtHelper.GenerateToken(data.UserUuid, "测试");
        }
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

    public void c2s_request_update_role_info(OperationResult result, Dictionary<string, object> values)
    {
        UpdateUserResponse data = new UpdateUserResponse();
        data.Success = result == OperationResult.Success;
        foreach (var item in values)
        {
            if(item.Value.GetType() == typeof(string))
            {
                UpdatedField field = new UpdatedField() { FieldName = item.Key, NewValue = (string)item.Value };
                data.UpdatedFields.Add(field);
            }
        }
        SendMessage(data);
    }
    #endregion
}

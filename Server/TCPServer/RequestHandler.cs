using System;
using System.Buffers;
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
    private ClientSocket clinetSocket;
    public RequestHandler(ClientSocket socket)
    {
        this.clinetSocket = socket;
    }

    public async Task SendMessage<T>(T data) where T : IMessage<T>
    {


        byte[] byteArrayData = data.ToByteArray();
        BaseMessage message = new BaseMessage();
        string typeName = typeof(T).Name;
        message.MsgType = MsgType.Server;
        message.Type = typeName;
        message.Timestamp = ServerSocket.CurrentServerTimestamp;
        message.Data = ByteString.CopyFrom(byteArrayData); // 直接将序列化后的字节数组放入 Data
        string messageJson = JsonConvert.SerializeObject(message);

        if (clinetSocket == null || clinetSocket.socket == null) return;
        if(!clinetSocket.socket.Connected)
        {
            clinetSocket.Close();
            return;
        }
        message.SenderId = clinetSocket.socket.RemoteEndPoint.ToString(); // 设置发送者ID
        byte[] messageBytes = message.ToByteArray();
        await HandleSubPakce(messageBytes);
    }

    private async Task HandleSubPakce(byte[] data)
    {
        byte[] tempDatas = ServerSocket.handleSubPack.CompressData(data);
        string messageId = Guid.NewGuid().ToString("N");
        int realMaxPacketSize = Constants.ProtocalTotalLength - Constants.ProtocalHeadLength;
        int packetTotal = Math.Max((int)Math.Ceiling((double)tempDatas.Length / realMaxPacketSize), 1);

        MemoryPool<byte> memoryPool = MemoryPool<byte>.Shared;
        IMemoryOwner<byte> memoryOwner = memoryPool.Rent(realMaxPacketSize);
        byte[] packetData = memoryOwner.Memory.Slice(0, realMaxPacketSize).Span.ToArray();


        List<byte[]> allPackets = new List<byte[]>();

        // 创建 Protocol 消息
        Protocol protocol = ServerSocket.ProtocolPool.Rent();
        protocol.MessageId = messageId;
        protocol.PacketTotal = packetTotal;

        for (int packetIndex = 1; packetIndex <= packetTotal; packetIndex++)
        {
            int startIndex = (packetIndex - 1) * realMaxPacketSize;
            int length = Math.Min(realMaxPacketSize, tempDatas.Length - startIndex);

            // 复制数据到当前包
            Array.Copy(tempDatas, startIndex, packetData, 0, length);

            protocol.PacketIndex = packetIndex;
            protocol.Data = ByteString.CopyFrom(packetData, 0, length);

            byte[] protocolBytes = protocol.ToByteArray();

            LoggerHelper.Instance.Debug($"发送消息id==={messageId}==包索引{packetIndex}==包数{packetTotal}==协议长度{protocolBytes.Length}");
            await clinetSocket.socket.SendAsync(new ArraySegment<byte>(protocolBytes), SocketFlags.None);// 异步发送数据
        }
        ServerSocket.ProtocolPool.Return(protocol);
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

    public async void c2s_request_join_guild(string memberId, string guildId)
    {
        JoinGuildRequest data = new JoinGuildRequest();
        data.State = await GuildService.JoinGuild(memberId, "玩家名字",guildId,0,1);
        SendMessage(data);
    }

    public async void c2s_request_exit_guild(string memberId, string guildId)
    {
        ExitGuildRequest data = new ExitGuildRequest();
        data.State = await GuildService.ExitGuild(memberId, guildId);
        SendMessage(data);
    }

    public async void c2s_request_delete_guild(int currentPage, int pageSize)
    {
        //GuildListMsg data = new GuildListMsg();
        //data.GuildList = await GuildService.GetGuildList(currentPage, pageSize);
        //SendMessage(data);
    }

    public async void c2s_request_get_suspend_reward(SuspendTimeMsg data)
    {
        SendMessage(data);
    }

    public async void c2s_request_public_channel_chat(ChatMsgList data)
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
            LoggerHelper.Instance.Info($"生成Token: {data.Token}");
        }
        data.SaveData = ByteString.CopyFrom(save_data);
        SendMessage(data);
    }

    public async Task c2s_request_synrous_role_attrs()
    {
        
    }

    public void c2s_request_entry_game()
    {
        
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

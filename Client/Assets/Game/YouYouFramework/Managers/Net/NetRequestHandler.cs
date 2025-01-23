using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Main;
using Protocols;
using Protocols.Game;
using Protocols.Guild;
using Protocols.Player;
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
        List<byte[]> list = HandleMessage(data);
        if (socket == null || !socket.Connected)
        {
            GameUtil.LogError("服务器已断开,无法发送心跳包");
            GameEntry.Net.HandleDisconnected(); // 处理连接断开
            foreach (var messageBytes in list)
            {
                GameEntry.Net.EnqueueMsg(messageBytes);
            }
            return; // 退出心跳发送循环
        }
        foreach (var messageBytes in list)
        {
            GameEntry.Net.EnqueueMsg(messageBytes);
        }
    }

    private List<byte[]> HandleMessage<T>(T data) where T : IMessage<T>
    {
        byte[] byteArrayData = data.ToByteArray();
        BaseMessage message = new BaseMessage();
        message.MsgType = MsgType.Client;
        message.Type = typeof(T).Name;
        message.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();// 获取当前时间戳
        message.SenderId = this.senderId; // 设置发送者ID
        message.Data = ByteString.CopyFrom(byteArrayData); // 直接将序列化后的字节数组放入 Data
        message.Token = GameEntry.Net.Token;
        byte[] messageBytes = message.ToByteArray();
        return HandleSubPakce(messageBytes);
    }

    private List<byte[]> HandleSubPakce(byte[] datas)
    {
        string messageId = Guid.NewGuid().ToString("N");
        int realMaxPacketSize = Constants.ProtocalTotalLength - Constants.ProtocalHeadLength;
        int packetTotal = Math.Max((int)Math.Ceiling((double)datas.Length / realMaxPacketSize), 1);

        byte[] packetData = new byte[realMaxPacketSize];
        List<byte[]> allPackets = new List<byte[]>();

        // 创建 Protocol 消息
        Protocol protocol = new Protocol();
        protocol.MessageId = messageId;
        protocol.PacketTotal = packetTotal;
        for (int packetIndex = 1; packetIndex <= packetTotal; packetIndex++)
        {
            int startIndex = (packetIndex - 1) * realMaxPacketSize;
            int length = Math.Min(realMaxPacketSize, datas.Length - startIndex);

            // 复制数据到当前包
            Array.Copy(datas, startIndex, packetData, 0, length);

            protocol.PacketIndex = packetIndex;
            protocol.Data = ByteString.CopyFrom(packetData, 0, length);
            byte[] protocolBytes = protocol.ToByteArray();
            GameUtil.LogError($"发送消息id==={messageId}==包索引{packetIndex}==包数{packetTotal}==协议长度{protocolBytes.Length}");
            allPackets.Add(protocolBytes);
        }
        return allPackets;
    }
    #region 发送协议

    // 示例：心跳包请求，处理心跳数据的逻辑，返回消息对象
    public void c2s_request_heart_beat()
    {
        if (Constants.IsLoginGame)
        {
            SendMessage(new HeartBeatMsg());
        }
    }

    //请求公会列表
    public void c2s_request_guild_list(int pageIndex,int pageSize)
    {
        GuildListMsg data = new GuildListMsg();
        data.GuildList = new GuildList();
        data.GuildList.CurrentPage = pageIndex;
        data.GuildList.PageSize = pageSize;
        SendMessage(data);
    }

    //请求加入公会
    public void c2s_request_join_guild()
    {
        
    }

    public void c2s_request_chat(int channel_type,string content = "",string receive_user_uuid = "",bool requestPublic = false)
    {
        ChatMsg data = MainEntry.ClassObjectPool.Dequeue<ChatMsg>();//new ChatMsg();
        data.Message = content;
        data.ChannelType = channel_type;
        data.ReceiverId = receive_user_uuid;
        data.SenderId = GameEntry.Data.UserId;
        data.IsRequestPublic = requestPublic;
        SendMessage(data);
    }

    //请求物品
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

    //请求登录
    public void c2s_request_login(string account, string password)
    {
        account = "a123";
        password = "99999";
        LoginMsg data = new LoginMsg()
        {
            UserAccount = account,
            UserPassword = SecurityUtil.ConvertBase64Key(password)
        };
        SendMessage(data);
    }

    //请求注册
    public void c2s_request_register(string account, string password)
    {
        account = "a1234";
        password = "99999";
        RegisterMsg data = new RegisterMsg()
        {
            UserAccount = account,
            UserPassword = SecurityUtil.ConvertBase64Key(password)
        };
        SendMessage(data);
    }
    
    //请求挂机时间 type1正常领取   2快速游历
    public void c2s_request_get_suspend_reward(int type)
    {
        SuspendTimeMsg data = new SuspendTimeMsg()
        {
            UserUuid = GameEntry.Data.UserId,
            Type = type
        };
        SendMessage(data);
    }
    
        
    //更新玩家数据
    public void c2s_request_update_role_info(Dictionary<string,string> values)
    {
        UpdateUserRequest data = new UpdateUserRequest()
        {
            UserUuid = GameEntry.Data.UserId,
        };
        foreach (var kvp in values)
        {
            data.UpdatedAttrs.Add(kvp.Key, kvp.Value);
        }
        SendMessage(data);
    }
    #endregion
}

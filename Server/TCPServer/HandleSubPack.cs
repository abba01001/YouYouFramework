using System.Collections.Generic;
using System.Reflection.Metadata;
using Protocols;
using TCPServer.Core;

public class HandleSubPack
{
    private Dictionary<string, MessageReceiver> messageReceivers = new Dictionary<string, MessageReceiver>();

    // 处理接收到的分包数据
    public BaseMessage ProcessSubPack(Protocol receivedMsg)
    {
        string messageId = receivedMsg.MessageId;
        byte[] dataBytes = receivedMsg.Data.ToByteArray();
        
        // 如果是第一次接收到这个 messageId，初始化接收器
        if (!messageReceivers.ContainsKey(messageId))
        {
            messageReceivers[messageId] =
                new MessageReceiver(messageId, receivedMsg.PacketTotal);
        }

        // 获取该消息的接收器
        var receiver = messageReceivers[messageId];

        // 将当前包的数据存储到接收器
        receiver.AddData(receivedMsg.PacketIndex, dataBytes);

        // 如果所有包接收完毕，获取完整的消息
        if (receiver.IsMessageComplete())
        {
            byte[] completeMessage = receiver.GetCompleteMessage();
            // 解析为 finalMessage
            return BaseMessage.Parser.ParseFrom(completeMessage);
        }

        // 如果还没有接收完整消息，返回 null
        return null;
    }
}

// 用于接收和拼接每个消息的类
public class MessageReceiver
{
    public string MessageId { get; private set; }
    public int PacketTotal { get; private set; }
    private byte[] completeMessage;
    private Dictionary<int, byte[]> packetsReceived;

    public MessageReceiver(string messageId, int packetTotal)
    {
        MessageId = messageId;
        PacketTotal = packetTotal;
        completeMessage = new byte[packetTotal * (Constants.ProtocalTotalLength - Constants.ProtocalHeadLength)]; // 假设每包最大数据大小为 1024 - 42
        packetsReceived = new Dictionary<int, byte[]>();
    }

    // 添加接收到的数据
    public void AddData(int packetIndex, byte[] data)
    {
        packetsReceived[packetIndex] = data;
    }

    // 判断是否所有包都已经接收到
    public bool IsMessageComplete()
    {
        return packetsReceived.Count == PacketTotal;
    }

    // 获取完整的消息
    public byte[] GetCompleteMessage()
    {
        List<byte> message = new List<byte>();
        for (int i = 1; i <= PacketTotal; i++)
        {
            message.AddRange(packetsReceived[i]);
        }
        return message.ToArray();
    }
}


using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Protocols;
using Protocols.Item;

public class ResponseHandler
{
    private Socket socket;
    private RequestHandler request;
    private readonly Dictionary<string, Action<BaseMessage>> _handlers = new Dictionary<string, Action<BaseMessage>>();

    public ResponseHandler(Socket socket, RequestHandler request)
    {
        this.socket = socket;
        this.request = request;
        InitializeHandlers();
    }

    public void InitializeHandlers()
    {
        // 注册心跳包处理器
        RegisterHandler(nameof(HeartBeatMsg), s2c_handle_request_heart_beat);
    }
    
    public void RegisterHandler(string messageType, Action<BaseMessage> handler)
    {
        if (!_handlers.ContainsKey(messageType))
        {
            _handlers.Add(messageType, handler);
        }
    }

    // 处理响应的分发逻辑
    public void HandleResponse(BaseMessage message)
    {
        if (_handlers.TryGetValue(message.Type, out var handler)) handler(message);
    }


    #region 协议

    // 示例处理方法，接收 BaseMessage 作为参数
    private void s2c_handle_request_heart_beat(BaseMessage message)
    {
        ProtocolHelper.UnpackData<ItemData>(message, (itemData) =>
        {
            //NetManager.Instance.Logger.LogMessage(socket,$"解包成功: Item ID: {itemData.ItemId}, Item Name: {itemData.ItemName}");
        });
        // 如果当前 socketA 的 ID 和消息的发送者 ID 相同，说明是自己发出的消息，不处理
        if (message.SenderId == socket.LocalEndPoint.ToString())
        {
            Console.WriteLine("收到自己发送的消息，忽略处理。");
            return;
        }
        request.c2s_request_heart_beat();
    }

    private void s2c_handle_other(BaseMessage message)
    {
        Console.WriteLine("处理其他请求...");
    }

    #endregion
}
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Protocols;
using Protocols.Item;

public class ResponseHandler
{
    private Socket socket;
    private RequestHandler request;
    private readonly Dictionary<int, Action<BaseMessage>> _handlers = new Dictionary<int, Action<BaseMessage>>();

    public ResponseHandler(Socket socket, RequestHandler request)
    {
        this.socket = socket;
        this.request = request;
        InitializeHandlers();
    }

    public void InitializeHandlers()
    {
        // 注册心跳包处理器
        RegisterHandler((int)MsgType.HeartBeat, s2c_handle_request_heart_beat);
        RegisterHandler(2, s2c_handle_other);
    }
    
    public void RegisterHandler(int messageType, Action<BaseMessage> handler)
    {
        if (!_handlers.ContainsKey(messageType))
        {
            _handlers.Add(messageType, handler);
        }
    }

    // 处理响应的分发逻辑
    public void HandleResponse(BaseMessage message)
    {
        if (_handlers.TryGetValue((int) message.Type, out var handler)) handler(message);
    }


    #region 协议

    // 示例处理方法，接收 BaseMessage 作为参数
    private void s2c_handle_request_heart_beat(BaseMessage message)
    {
        ProtocolHelper.UnpackData<ItemData>(message, (itemData) =>
        {
            //NetManager.Instance.Logger.LogMessage(socket,$"解包成功: Item ID: {itemData.ItemId}, Item Name: {itemData.ItemName}");
        });
        request.c2s_request_heart_beat();
    }

    private void s2c_handle_other(BaseMessage message)
    {
        Console.WriteLine("处理其他请求...");
    }

    #endregion
}
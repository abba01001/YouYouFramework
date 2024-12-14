using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Protocols;
using Protocols.Item;
using UnityEngine;

public class NetResponseHandler
{
    private Socket socket;
    private NetRequestHandler _netRequest;
    private readonly Dictionary<string, Action<BaseMessage>> _handlers = new Dictionary<string, Action<BaseMessage>>();

    public NetResponseHandler(Socket socket, NetRequestHandler netRequest)
    {
        this.socket = socket;
        this._netRequest = netRequest;
        InitializeHandlers();
    }

    public void InitializeHandlers()
    {
        // 注册心跳包处理器
        RegisterHandler(nameof(HeartBeatMsg), s2c_handle_request_heart_beat);
    }
    
    public void RegisterHandler(string type, Action<BaseMessage> handler)
    {
        if (!_handlers.ContainsKey(type))
        {
            _handlers.Add(type, handler);
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
        GameUtil.LogError($"收到心跳回应");
        ProtocolHelper.UnpackData<Protocols.Item.ItemData>(message, (itemData) =>
        {
            //NetManager.Instance.Logger.LogMessage(socket,$"解包成功: Item ID: {itemData.ItemId}, Item Name: {itemData.ItemName}");
        });
    }

    private void s2c_handle_other(BaseMessage message)
    {
        Console.WriteLine("处理其他请求...");
    }

    #endregion
}
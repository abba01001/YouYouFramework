using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Protocols;
using Protocols.Guild;
using Protocols.Item;
using TCPServer.Core.DataAccess;
using TCPServer.Core.Services;
using TCPServer.Utils;

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
        RegisterHandler(nameof(GuildListMsg), s2c_handle_request_guild_list);
        RegisterHandler(nameof(LoginMsg), s2c_handle_request_login);
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
        if(message.Type != nameof(LoginMsg))
        {
            //验证token
            if (JwtHelper.ValidateToken(message.Token) == null)
            {
                Console.WriteLine("用户Token验证失败");
                return;
            }
        }
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
        request.c2s_request_heart_beat();
    }

    private void s2c_handle_request_guild_list(BaseMessage message)
    {
        //处理业务逻辑
        ProtocolHelper.UnpackData<Protocols.Guild.GuildList>(message, (itemData) =>
        {
            //NetManager.Instance.Logger.LogMessage(socket,$"解包成功: Item ID: {itemData.ItemId}, Item Name: {itemData.ItemName}");
        });
        request.c2s_request_guild_list();
    }

    private void s2c_handle_request_login(BaseMessage message)
    {
        ProtocolHelper.UnpackData<LoginMsg>(message, async (data) =>
        {
            (OperationResult state,string user_uuid) = await RoleService.LoginAsync(data.UserAccount, data.UserPassword);
            request.c2s_request_login((int)state, user_uuid);
        });
    }
    private void s2c_handle_other(BaseMessage message)
    {
        Console.WriteLine("处理其他请求...");
    }

    #endregion
}
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Protocols;
using Protocols.Guild;
using Protocols.Item;
using Protocols.Player;
using TCPServer.Core;
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
        RegisterHandler(nameof(RegisterMsg), s2c_handle_request_register);
        RegisterHandler(nameof(UpdateUserRequest), s2c_handle_request_update_role_info);
        RegisterHandler(nameof(ChatMsg), s2c_handle_request_chat);
        RegisterHandler(nameof(SuspendTimeMsg), s2c_handle_get_suspend_time_msg);
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
        //验证token
        if (NeedValidateMessage(message.Type) && JwtHelper.ValidateToken(message.Token) == null)
        {
            Console.WriteLine($"用户Token验证失败========{message.Type}");
            return;
        }
        if (_handlers.TryGetValue(message.Type, out var handler)) handler(message);
    }

    private bool NeedValidateMessage(string msgType)
    {
        if (msgType == nameof(LoginMsg) || msgType == nameof(RegisterMsg)) return false;
        return true;
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
        ProtocolHelper.UnpackData<Protocols.Guild.GuildListMsg>(message, (data) =>
        {
            request.c2s_request_guild_list(data.GuildList.CurrentPage,data.GuildList.PageSize);
            //NetManager.Instance.Logger.LogMessage(socket,$"解包成功: Item ID: {itemData.ItemId}, Item Name: {itemData.ItemName}");
        });
    }

    private void s2c_handle_request_login(BaseMessage message)
    {
        ProtocolHelper.UnpackData<LoginMsg>(message, async (data) =>
        {
            Console.WriteLine($"{nameof(LoginMsg)}: {data}");
            (OperationResult state,string user_uuid, byte[] save_data) = await RoleService.LoginAsync(data.UserAccount, data.UserPassword);
            request.c2s_request_login((int)state, user_uuid, save_data);
        });
    }

    private void s2c_handle_request_register(BaseMessage message)
    {
        ProtocolHelper.UnpackData<RegisterMsg>(message, async (data) =>
        {
            Console.WriteLine($"{nameof(RegisterMsg)}: {data}");
            (OperationResult state,string uuid) = await RoleService.CreateUserAsync(data.UserAccount, data.UserPassword);
            request.c2s_request_register((int)state, uuid);
        });
    }

    private void s2c_handle_request_chat(BaseMessage message)
    {
        ProtocolHelper.UnpackData<ChatMsg>(message, async (data) =>
        {
            Console.WriteLine($"{nameof(ChatMsg)}: {data}");
            OperationResult state = await ChatService.HandleChatMsg(data);
            if(state == OperationResult.Success)
            {
                ServerSocket.BroadcastMsg<ChatMsg>(data);
            }
        });
    }

    private void s2c_handle_get_suspend_time_msg(BaseMessage message)
    {
        ProtocolHelper.UnpackData<SuspendTimeMsg>(message, async (data) =>
        {
            Console.WriteLine($"{nameof(SuspendTimeMsg)}: {data}");
            if(data.Type == 0)
            {
                int timeStamp = await RoleService.GetSuspendTimeInHoursAsync(data.UserUuid);
                request.c2s_request_get_suspend_reward(data.Type,false,0,timeStamp);
            }
            else
            {
                (OperationResult state, int hour) = await RoleService.CanClaimRewardAsync(data.UserUuid, data.Type);
                if (state == OperationResult.Success)
                {
                    request.c2s_request_get_suspend_reward(data.Type,state == OperationResult.Success, hour);
                }
            }
        });
    }

    private void s2c_handle_other(BaseMessage message)
    {
        Console.WriteLine("处理其他请求...");
    }

    //修改玩家属性
    private void s2c_handle_request_update_role_info(BaseMessage message)
    {
        ProtocolHelper.UnpackData<UpdateUserRequest>(message, async (data) =>
        {
            Dictionary<string, string> updatedAttrs = new Dictionary<string, string>(data.UpdatedAttrs);
            (OperationResult result, Dictionary<string, object> updatedValues) = await RoleService.UpdateUserPropertyAsync(data.UserUuid, updatedAttrs);
            request.c2s_request_update_role_info(result, updatedValues);
        });
    }
    #endregion
}
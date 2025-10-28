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
using static Google.Protobuf.Reflection.FieldOptions.Types;

public class ResponseHandler
{
    private ClientSocket socket;
    private RequestHandler request;
    private readonly Dictionary<string, Action<BaseMessage>> _handlers = new Dictionary<string, Action<BaseMessage>>();

    public ResponseHandler(ClientSocket socket, RequestHandler request)
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
        RegisterHandler(nameof(JoinGuildRequest), s2c_handle_request_join_guild);
        RegisterHandler(nameof(ExitGuildRequest), s2c_handle_request_exit_guild);
        RegisterHandler(nameof(DeleteGuildRequest), s2c_handle_request_delete_guild);

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
        if (message == null) return;
        if (message.MsgType == MsgType.Server) return;
        if (NeedValidateMessage(message.Type) && JwtHelper.ValidateToken(message.Token) == null)
        {
            LoggerHelper.Instance.Info($"用户Token验证失败========{message.Type}");
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
            socket.LastHeartbeatTime = DateTime.UtcNow;
            if (socket.UserAccount != string.Empty)
            {
                RoleService.RefreshOnlineUsers(3,socket.UserAccount);
            }
            //NetManager.Instance.Logger.LogMessage(socket,$"解包成功: Item ID: {itemData.ItemId}, Item Name: {itemData.ItemName}");
        });
        //request.c2s_request_heart_beat();
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

    private void s2c_handle_request_join_guild(BaseMessage message)
    {
        //处理业务逻辑
        ProtocolHelper.UnpackData<Protocols.Guild.JoinGuildRequest>(message, async (data) =>
        {
            request.c2s_request_join_guild(data.MemberId, data.GuildId);
        });
    }

    private void s2c_handle_request_exit_guild(BaseMessage message)
    {
        //处理业务逻辑
        ProtocolHelper.UnpackData<Protocols.Guild.ExitGuildRequest>(message, async (data) =>
        {
            request.c2s_request_exit_guild(data.MemberId, data.GuildId);
        });
    }

    private void s2c_handle_request_delete_guild(BaseMessage message)
    {
        ////处理业务逻辑
        //ProtocolHelper.UnpackData<Protocols.Guild.DeleteGuildRequest>(message, async (data) =>
        //{
        //    request.c2s_request_delete_guild(data.MemberId, data.GuildId);
        //});
    }

    private void s2c_handle_request_login(BaseMessage message)
    {
        ProtocolHelper.UnpackData<LoginMsg>(message, async (data) =>
        {
            (OperationResult state,string user_uuid, byte[] save_data) = await RoleService.LoginAsync(data.UserAccount, data.UserPassword);
            if (state == OperationResult.Success)
            {
                socket.UserAccount = data.UserAccount;
                socket.UserUUID = user_uuid;
            }
            request.c2s_request_login((int)state, user_uuid, save_data);
        });
    }

    private void s2c_handle_request_register(BaseMessage message)
    {
        ProtocolHelper.UnpackData<RegisterMsg>(message, async (data) =>
        {
            (OperationResult state,string uuid) = await RoleService.CreateUserAsync(data.UserAccount, data.UserPassword);
            request.c2s_request_register((int)state, uuid);
        });
    }

    private void s2c_handle_request_chat(BaseMessage message)
    {
        ProtocolHelper.UnpackData<ChatMsg>(message, async (data) =>
        {
            if(data.IsRequestPublic)
            {
                ChatMsgList msgList = new ChatMsgList();
                msgList.PublicChat.AddRange(await ChatService.GetChatMessages(true, data.SenderId, 1, 1, 200));
                request.c2s_request_public_channel_chat(msgList);

            }
            else
            {
                OperationResult state = await ChatService.HandleChatMsg(data);
                if (state == OperationResult.Success)
                {
                    await ServerSocket.BroadcastMsg<ChatMsg>(data);
                }
            }
        });
    }

    private void s2c_handle_get_suspend_time_msg(BaseMessage message)
    {
        ProtocolHelper.UnpackData<SuspendTimeMsg>(message, async (data) =>
        {
            LoggerHelper.Instance.Info($"{nameof(SuspendTimeMsg)}: {data}");
            SuspendTimeMsg s = new SuspendTimeMsg();
            if(data.Type == 0)
            {
                (s.Timestamp,s.QuickGetSuspendRewardIndex,s.QuickGetSuspendRewardLimit) = await RoleService.GetSuspendTimeParamsAsync(data.UserUuid);
                s.Type = data.Type;
                request.c2s_request_get_suspend_reward(s);
            }
            else
            {
                (OperationResult state, SuspendTimeMsg msg) = await RoleService.CanClaimRewardAsync(data.UserUuid, data.Type);
                if (state == OperationResult.Success && msg != null)
                {
                    s.Type = data.Type;
                    s.CanGetReward = true;
                    s.Hour = msg.Hour;
                    request.c2s_request_get_suspend_reward(s);
                }
            }
        });
    }

    private void s2c_handle_other(BaseMessage message)
    {
        LoggerHelper.Instance.Info("处理其他请求...");
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
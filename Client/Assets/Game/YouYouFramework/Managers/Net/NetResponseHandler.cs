using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Google.Protobuf;
using Protocols;
using Protocols.Guild;
using Protocols.Item;
using Protocols.Player;
using Sirenix.Utilities;
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

    // 注册响应
    public void InitializeHandlers()
    {
        RegisterHandler(nameof(ExitGameMsg), s2c_handle_exit_game);
        RegisterHandler(nameof(HotUpdateMsg), s2c_handle_hot_update);
        RegisterHandler(nameof(HeartBeatMsg), s2c_handle_request_heart_beat);
        RegisterHandler(nameof(GuildListMsg), s2c_handle_request_guild_list);
        RegisterHandler(nameof(LoginMsg), s2c_handle_request_login);
        RegisterHandler(nameof(RegisterMsg), s2c_handle_request_register);
        RegisterHandler(nameof(UpdateUserResponse), s2c_handle_request_update_role_info);
        RegisterHandler(nameof(ChatMsg), s2c_handle_chat_msg);
        RegisterHandler(nameof(ChatMsgList), s2c_handle_request_public_channel_chat);
        RegisterHandler(nameof(SuspendTimeMsg), s2c_handle_get_suspend_time_msg);
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
        ProtocolHelper.UnpackData<HeartBeatMsg>(message, (itemData) =>
        {
            //NetManager.Instance.Logger.LogMessage(socket,$"解包成功: Item ID: {itemData.ItemId}, Item Name: {itemData.ItemName}");
        });
    }
    
    //公会列表
    private void s2c_handle_request_guild_list(BaseMessage message)
    {
        ProtocolHelper.UnpackData<Protocols.Guild.GuildListMsg>(message, (data) =>
        {
            GameUtil.LogError($"公会数量{data.GuildList.TotalCount}");
        });
    }
    
    //服务器下发热更信息
    private void s2c_handle_hot_update(BaseMessage message)
    {
        ProtocolHelper.UnpackData<HotUpdateMsg>(message, (data) =>
        {
            GameUtil.LogError($"服务器下发资源信息");
        });
    }
    
    //服务器下发退出游戏
    private void s2c_handle_exit_game(BaseMessage message)
    {
        ProtocolHelper.UnpackData<ExitGameMsg>(message, (data) =>
        {
            GameUtil.LogError($"服务器下发资源信息");
            GameEntry.Procedure.ChangeState(ProcedureState.Preload);
        });
    }

    //登录
    private void s2c_handle_request_login(BaseMessage message)
    {
        ProtocolHelper.UnpackData<LoginMsg>(message, (data) =>
        {
            if (data.State == 0)
            {
                GameUtil.LogError("账号不存在");
            }
            else if (data.State == 1)
            {
                GameEntry.Data.UserId = data.UserUuid;
                byte[] binaryData = data.SaveData.ToByteArray();
                GameEntry.Data.InitGameData(binaryData);
                GameEntry.Net.Token = data.Token;
                GameEntry.Time.InitNetTime(message.Timestamp);
                GameEntry.Event.Dispatch(Constants.EventName.LoginSuccess);
                Constants.IsEntryGame = true;
            }
            else
            {
                GameUtil.LogError("密码错误");
            }
        });
    }
    
    //注册
    private void s2c_handle_request_register(BaseMessage message)
    {
        ProtocolHelper.UnpackData<RegisterMsg>(message, (data) =>
        {
            if (data.State == 1)
            {
                GameUtil.LogError("注册成功");
                GameEntry.Data.UserId = data.UserUuid;
                GameEntry.Data.InitGameData(null);//data.SaveData.ToByteArray());
                GameEntry.Net.Token = data.Token;
                GameEntry.Event.Dispatch(Constants.EventName.LoginSuccess);
                Constants.IsEntryGame = true;
            }
            else
            {
                GameUtil.LogError("注册失败");
            }
        });
    }

    //修改玩家属性
    private void s2c_handle_request_update_role_info(BaseMessage message)
    {
        ProtocolHelper.UnpackData<UpdateUserResponse>(message, (data) =>
        {
            if (data.Success)
            {
                foreach (var VARIABLE in data.UpdatedFields)
                {
                    GameUtil.LogError($"{VARIABLE.FieldName}==={VARIABLE.NewValue}");
                }
            }
            else
            {
                
            }
        });
    }

    private void s2c_handle_chat_msg(BaseMessage message)
    {
        ProtocolHelper.UnpackData<ChatMsg>(message, (data) =>
        {
            GameEntry.Event.Dispatch(Constants.EventName.UpdateChatText,new ChatMsg()
            {
                ChannelType = data.ChannelType,
                Message = data.Message
            });
            GameUtil.LogError($"收到服务器下发消息:=====>{data.Message}");
        });
    }

    private void s2c_handle_request_public_channel_chat(BaseMessage message)
    {
        ProtocolHelper.UnpackData<ChatMsgList>(message, (data) =>
        {
            var t1 = data.PublicChat.OrderBy(msg => msg.Timestamp).ToList();
            GameEntry.Data.TempChatMsgs.Add(t1);
            if (t1.Count > 0)
            {
                GameEntry.Event.Dispatch(Constants.EventName.UpdateChatText,new ChatMsg()
                {
                    ChannelType = t1[^1].ChannelType,
                    Message = t1[^1].Message
                });
            }
        });
    }

    private void s2c_handle_get_suspend_time_msg(BaseMessage message)
    {
        ProtocolHelper.UnpackData<SuspendTimeMsg>(message, (data) =>
        {
            GameEntry.Event.Dispatch(Constants.EventName.GetSuspendReward,data);                
        });
    }
    
    private void s2c_handle_other(BaseMessage message)
    {
        Console.WriteLine("处理其他请求...");
    }

    #endregion
}
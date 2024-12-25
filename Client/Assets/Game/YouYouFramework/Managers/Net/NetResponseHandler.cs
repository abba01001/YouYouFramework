using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Google.Protobuf;
using Protocols;
using Protocols.Guild;
using Protocols.Item;
using Protocols.Player;
using UnityEngine;
using YouYou;

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
        RegisterHandler(nameof(HeartBeatMsg), s2c_handle_request_heart_beat);
        RegisterHandler(nameof(GuildListMsg), s2c_handle_request_guild_list);
        RegisterHandler(nameof(LoginMsg), s2c_handle_request_login);
        RegisterHandler(nameof(UpdateUserResponse), s2c_handle_request_update_role_info);
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
    
    private void s2c_handle_request_guild_list(BaseMessage message)
    {
        GameUtil.LogError($"收到request_guild_list回应");
        ProtocolHelper.UnpackData<Protocols.Guild.GuildListMsg>(message, (data) =>
        {
            GameUtil.LogError($"解包成功:{data.GuildList.Guilds.Count}=={data.GuildList.CurrentPage}");
            foreach (var pair in data.GuildList.Guilds)
            {
                foreach (var property in pair.GetType().GetProperties())
                {
                    var value = property.GetValue(pair);
                    GameUtil.LogError($"键{property.Name}====值{value}");
                }
            }
        });
    }

    //登录
    private void s2c_handle_request_login(BaseMessage message)
    {
        GameUtil.LogError($"收到request_login回应");
        ProtocolHelper.UnpackData<LoginMsg>(message, (data) =>
        {
            if (data.State == 0)
            {
                GameUtil.LogError("账号不存在");
            }
            else if (data.State == 1)
            {
                GameEntry.Data.UserId = data.UserUuid;
                GameUtil.LogError("数据=====》",data.SaveData.ToBase64());
                byte[] byteArray = data.SaveData.ToByteArray();
                
                string base64String = Convert.ToBase64String(byteArray);
                byte[] binaryData = Convert.FromBase64String(base64String);
                GameUtil.LogError($"登录回调数据{base64String}");
                
                GameEntry.Data.InitGameData(binaryData);//data.SaveData.ToByteArray());
                Constants.IsLoginGame = true;
                Constants.Token = data.Token;
                GameEntry.Event.Dispatch(Constants.EventName.LoginSuccess);
            }
            else
            {
                GameUtil.LogError("密码错误");
            }
        });
    }

    //修改玩家属性
    private void s2c_handle_request_update_role_info(BaseMessage message)
    {
        GameUtil.LogError($"收到request_update_role_info回应");
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
    
    private void s2c_handle_other(BaseMessage message)
    {
        Console.WriteLine("处理其他请求...");
    }

    #endregion
}
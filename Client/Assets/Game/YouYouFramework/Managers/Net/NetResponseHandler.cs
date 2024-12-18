using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Protocols;
using Protocols.Guild;
using Protocols.Item;
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

        LoginMsg msg = new LoginMsg();
        ProtocolHelper.UnpackData<LoginMsg>(message, (data) =>
        {
            foreach (var property in data.GetType().GetProperties())
            {
                var value = property.GetValue(data);
                GameUtil.LogError($"键{property.Name}====值{value}");
            }

            if (data.State == 0)
            {
                GameUtil.LogError("账号不存在");
            }
            else if (data.State == 1)
            {
                GameEntry.Data.UserId = data.UserUuid;
                GameEntry.SDK.DownloadGameData(GameEntry.Data.UserId);
                Constants.Token = data.Token;
            }
            else
            {
                GameUtil.LogError("密码错误");
            }
        });
    }

    private void s2c_handle_other(BaseMessage message)
    {
        Console.WriteLine("处理其他请求...");
    }

    #endregion
}
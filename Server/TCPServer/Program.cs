using NLog;
using Protocols;
using Protocols.Player;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TCPServer.Core;
using TCPServer.Core.DataAccess;
using TCPServer.Core.Services;
using TCPServer.Utils;

class Program
{
    static void Main(string[] args)
    {
        NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("nlog.config");

        // 提供功能选择
        while (true)
        {
            LoggerHelper.Instance.Debug("请选择操作:");
            LoggerHelper.Instance.Debug("1. 开启服务器");
            LoggerHelper.Instance.Debug("2. 查询某个玩家的游戏数据");
            LoggerHelper.Instance.Debug("请输入数字选择操作（或者输入Quit退出）：");

            string inputStr = Console.ReadLine();
            if (inputStr == "Quit")
            {
                break;
            }
            else if (inputStr == "1")
            {
                StartServer();
            }
            else if (inputStr == "2")
            {
                SqlManager.Initialize($"Server={KeyUtils.GetSqlKey(SqlKey.Server)};Database={KeyUtils.GetSqlKey(SqlKey.Database)};" +
  $"UserId={KeyUtils.GetSqlKey(SqlKey.UserId)};Password={KeyUtils.GetSqlKey(SqlKey.Password)};Port = {KeyUtils.GetSqlKey(SqlKey.Port)}");
                QueryPlayerData();
            }
            else
            {
                LoggerHelper.Instance.Debug("无效的选择，请重新输入。");
            }
        }
    }

    // 开启服务器
    private static void StartServer()
    {
        ServerSocket.Start("0.0.0.0", 17888, 1024); //10.0.28.15
        LoggerHelper.Instance.Info("服务器已启动，输入Quit以退出服务器。");

        // 持续监听服务器指令
        while (true)
        {
            string inputStr = Console.ReadLine();
            if (inputStr == "Quit")
            {
                ServerSocket.Close();
                LoggerHelper.Instance.Info("服务器已关闭。");
                break;
            }
            else
            {
                HandleServerCommand(inputStr);
            }
        }
    }

    // 处理服务器命令
    private static void HandleServerCommand(string inputStr)
    {
        if (inputStr == "K")
        {
            HotUpdateMsg msg = new HotUpdateMsg();
            ServerSocket.BroadcastMsg(msg);
        }
        else if (inputStr == "A")
        {
            PlayerData data = new PlayerData();
            LoggerHelper.Instance.Info(JwtHelper.GenerateToken("a123", "测试人"));
        }
        else if (inputStr == "Z")
        {
            RoleService.CreateUserAsync("a1", "132");
            RoleService.CreateUserAsync("a2", "132");
            RoleService.CreateUserAsync("a3", "132");
            RoleService.CreateUserAsync("a4", "132");
        }
        else if (inputStr == "B")
        {
            GuildService.CreateGuild("6816a8da-12d5-412d-a484-329250dc059a", "测试公会", "测试名字", "测试描述");
        }
        else if (inputStr == "Q")
        {
            FriendService.AddFriendAsync("50a757cd-932e-4338-b0ae-1d3eb2ed9530", "b23673d6-831f-4263-a1c6-5b6ade37f3ea");
        }
        else if (inputStr == "W")
        {
            FriendService.AcceptFriendRequestAsync("50a757cd-932e-4338-b0ae-1d3eb2ed9530", "b23673d6-831f-4263-a1c6-5b6ade37f3ea");
        }
        else if (inputStr == "E")
        {
            FriendService.BlockFriendAsync("b23673d6-831f-4263-a1c6-5b6ade37f3ea", "50a757cd-932e-4338-b0ae-1d3eb2ed9530", true);
        }
        else if (inputStr == "R")
        {
            TestAsync1();
        }
    }

    // 查询某个玩家的游戏数据
    private static void QueryPlayerData()
    {
        while (true)
        {
            Console.WriteLine("请输入玩家账号查询游戏数据：");
            string playerAccount = Console.ReadLine();

            LoggerHelper.Instance.Debug($"开始查询玩家 {playerAccount} 的游戏数据");

            // 模拟查询玩家数据（此处根据需求进行替换）
            var result = RoleService.GetUserByAccountAsync(playerAccount);
            LoggerHelper.Instance.Debug($"查询请求已发出，正在等待结果...");

            // 等待查询完成（假设你会从 RoleService 返回查询结果）
            if (result != null)
            {
                LoggerHelper.Instance.Debug($"玩家 {playerAccount} 的数据查询成功。");
                Console.WriteLine($"玩家 {playerAccount} 的数据查询成功。");
            }
            else
            {
                LoggerHelper.Instance.Debug($"未找到该玩家的数据: {playerAccount}");
                Console.WriteLine("未找到该玩家数据。");
            }

            // 提问用户是否继续查询
            Console.WriteLine("是否继续查询其他玩家数据？(Y/N)");
            string continueChoice = Console.ReadLine();
            if (continueChoice.ToUpper() != "Y")
            {
                LoggerHelper.Instance.Debug("停止查询，返回主菜单。");
                break; // 退出查询循环，返回主菜单
            }
            else
            {
                LoggerHelper.Instance.Debug("用户选择继续查询。");
            }
        }
    }

    private static async Task TestAsync1()
    {
        for (int i = 0; i < 10; i++)
        {
            ChatMsg chatMsg1 = new ChatMsg();
            chatMsg1.SenderId = "b23673d6-831f-4263-a1c6-5b6ade37f3ea";
            chatMsg1.Message = $"噶尔挨个tsetse商业热带水果条幅放突发========={i}";
            chatMsg1.ChannelType = 1;
            OperationResult state = await ChatService.HandleChatMsg(chatMsg1);
            if (state == OperationResult.Success)
            {
                await ServerSocket.BroadcastMsg<ChatMsg>(chatMsg1);
            }
        }
    }
}

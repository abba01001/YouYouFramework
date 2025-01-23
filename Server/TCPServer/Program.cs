using Google.Protobuf;
using NLog;
using Protocols;
using Protocols.Player;
using System;
using System.Collections.Generic;
using System.Linq;
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
                HandleSubPakce(new byte[1024]);
                LoggerHelper.Instance.Debug("无效的选择，请重新输入。");
            }
        }
    }

    private static void HandleSubPakce(byte[] datas)
    {
        const int MaxPacketSize = 1024;
        const int PerPacketOffset = 3; // 每个分包的偏移

        string messageId = Guid.NewGuid().ToString("N");

        // 只需计算一次协议头的字节长度
        Protocol test = new Protocol();
        test.PacketIndex = 127; // 包索引最大值
        test.PacketTotal = 127; // 包长度最大值
        test.MessageId = messageId;
        byte[] protoHeaderBytes = test.ToByteArray();
        int protoHeaderLength = protoHeaderBytes.Length + PerPacketOffset;


        int realMaxPacketSize = MaxPacketSize - protoHeaderLength;
        int packetTotal = Math.Max((int)Math.Ceiling((double)datas.Length / realMaxPacketSize), 1);

        byte[] packetData = new byte[realMaxPacketSize];
        List<byte[]> allPackets = new List<byte[]>();

        // 创建 Protocol 消息
        Protocol protocol = new Protocol
        {
            MessageId = messageId,
            PacketTotal = packetTotal
        };

        LoggerHelper.Instance.Info($"协议头长度 {protoHeaderLength}===真实数据长度{datas.Length}");
        for (int packetIndex = 1; packetIndex <= packetTotal; packetIndex++)
        {
            int startIndex = (packetIndex - 1) * realMaxPacketSize;
            int length = Math.Min(realMaxPacketSize, datas.Length - startIndex);

            // 复制数据到当前包
            Array.Copy(datas, startIndex, packetData, 0, length);

            protocol.PacketIndex = packetIndex;
            protocol.Data = ByteString.CopyFrom(packetData, 0, length);

            byte[] protocolBytes = protocol.ToByteArray();

            // 输出每部分的长度
            LoggerHelper.Instance.Info($"协议头长度: {protoHeaderLength}, 数据长度: {length}, 协议总长度: {protocolBytes.Length}");
            string hexString = BitConverter.ToString(protocolBytes);
            //LoggerHelper.Instance.Info($"Protocol字节数据 (包 {packetIndex}/{packetTotal}): {hexString}");
            LoggerHelper.Instance.Info($"协议长度: {protocolBytes.Length} === 真实数据长度: {length}");

            allPackets.Add(protocolBytes);
        }
    }



    // 开启服务器
    private static void StartServer()
    {
        ServerSocket.Start("0.0.0.0", 17888, 1024); //10.0.28.15
        LoggerHelper.Instance.Debug("服务器已启动，输入Quit以退出服务器。");

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
            LoggerHelper.Instance.Debug(JwtHelper.GenerateToken("a123", "测试人"));
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
            if (result != null)
            {
                LoggerHelper.Instance.Debug($"玩家 {playerAccount} 的数据查询成功。");
            }
            else
            {
                LoggerHelper.Instance.Debug($"未找到该玩家的数据: {playerAccount}");
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

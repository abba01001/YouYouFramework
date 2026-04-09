using Google.Protobuf;
using NLog;
using NLog.Config;
using Protocols;
using Protocols.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TCPServer;
using TCPServer.Core;
using TCPServer.Core.DataAccess;
using TCPServer.Core.Services;
using TCPServer.Utils;

class Program
{
    static void Main(string[] args)
    {
        // 设置工作目录为 exe 所在目录
        string exeDir = AppContext.BaseDirectory;
        Directory.SetCurrentDirectory(exeDir);

        // 加载 NLog 配置
        string configPath = Path.Combine(exeDir, "NLog.config");
        LogManager.Configuration = new XmlLoggingConfiguration(configPath);

        // 提供功能选择
        while (true)
        {
            LoggerHelper.Instance.Debug("请选择操作:");
            LoggerHelper.Instance.Debug("1. 开启服务器");
            LoggerHelper.Instance.Debug("2. 查询某个玩家的游戏数据");
            LoggerHelper.Instance.Debug("3. 测试服务端mysql");
            LoggerHelper.Instance.Debug("4. 测试Redis");
            LoggerHelper.Instance.Debug("请输入数字选择操作（或者输入Quit退出）：");

            string inputStr = Console.ReadLine();
            if (inputStr == "Quit")
            {
                break;
            }
            else if (inputStr == "1")
            {
                RedisManager.Instance.Init("127.0.0.1:6379");
                var flush = new DBFlushService();
                flush.Start();
                
                StartServer();
            }
            else if (inputStr == "2")
            {
                SqlManager.Initialize($"Server={"43.134.133.178"};Database={"unitygamedata"};" +
                                      $"UserId={"pengjunwei"};Password={"pengjunwei"};Port = {"5001"}");
  //               SqlManager.Initialize($"Server={KeyUtils.GetSqlKey(SqlKey.Server)};Database={KeyUtils.GetSqlKey(SqlKey.Database)};" +
  // $"UserId={KeyUtils.GetSqlKey(SqlKey.UserId)};Password={KeyUtils.GetSqlKey(SqlKey.Password)};Port = {KeyUtils.GetSqlKey(SqlKey.Port)}");
                QueryPlayerData();
            }
            else if (inputStr == "3")
            {
                SqlManager.Initialize($"Server={"43.134.133.178"};Database={"unitygamedata"};" +
$"UserId={"pengjunwei"};Password={"pengjunwei"};Port = {"5001"}");
                QueryPlayerData();
                //GuildService.GetGuildById("82");

                //GuildService.ExitGuild("1c1341de-ac5c-463e-b920-5a072dec40a9", "83");

            }
            else if (inputStr == "4")
            {
                TestConnectRedis();
                
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

    private static async Task TestConnectRedis()
    {
        RedisManager.Instance.Init("127.0.0.1:6379");
        var flush = new DBFlushService();
        flush.Start();
        
        // 写入字符串
        string uid = "88888888";
        await RedisHelper.ListRightPushAsync(RedisKey.PlayerBag(uid), "Sword");
        await RedisHelper.ListRightPushAsync(RedisKey.PlayerBag(uid), "Shield");

        var items = await RedisHelper.ListRangeAsync(RedisKey.PlayerBag(uid));
        LoggerHelper.Instance.Error($"List Inventory: {string.Join(",", items)}");

        await RedisHelper.HashSetAsync(RedisKey.PlayerCurrency(uid), "Gold", "500");
        await RedisHelper.HashSetAsync(RedisKey.PlayerCurrency(uid), "Gems", "50");

        var gold = await RedisHelper.HashGetAsync(RedisKey.PlayerCurrency(uid), "Gold");
        LoggerHelper.Instance.Error($"Gold: {gold}");
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
            AccountService.CreateUserAsync("a1", "132");
            AccountService.CreateUserAsync("a2", "132");
            AccountService.CreateUserAsync("a3", "132");
            AccountService.CreateUserAsync("a4", "132");
        }
        else if (inputStr == "B")
        {
            //GuildService.CreateGuild("76181cf6-8a74-49e6-af25-5824d98f125f", "测试公会", "测试名字", "测试描述");
            GuildService.ExitGuild("1c1341de-ac5c-463e-b920-5a072dec40a9", "83");
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
            LoggerHelper.Instance.Info("请输入玩家账号查询游戏数据：");
            string playerAccount = Console.ReadLine();

            LoggerHelper.Instance.Debug($"开始查询玩家 {playerAccount} 的游戏数据");

            // 模拟查询玩家数据（此处根据需求进行替换）
            var result = AccountService.GetUserByAccountAsync(playerAccount);
            if (result != null)
            {
                byte[] datas = new byte[0];
                foreach (var kvp in result.Result)
                {
                    LoggerHelper.Instance.Debug($"列名: {kvp.Key} === 值: {kvp.Value}");
                    if(kvp.Key == "save_data")
                    {
                        datas = (byte[])kvp.Value;

                        string hex = BitConverter.ToString(datas).Replace("-", " ");
                        LoggerHelper.Instance.Debug($"玩家数据==={BitConverter.ToString(datas)}");
                    }
                }
                byte[] d = ServerSocket.handleSubPack.DecompressData(datas);
                DataManager.Instance.InitGameData(d);
                LoggerHelper.Instance.Debug($"玩家 {playerAccount} 的数据查询成功。");
            }
            else
            {
                LoggerHelper.Instance.Debug($"未找到该玩家的数据: {playerAccount}");
            }

            // 提问用户是否继续查询
            LoggerHelper.Instance.Info("是否继续查询其他玩家数据？(Y/N)");
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

// 定义一个类来存放每条房源信息
public class HouseInfo
{
    public string Column1 { get; set; }
    public string Column2 { get; set; }
    // 根据实际的表格列数和含义添加更多字段
}


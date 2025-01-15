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
        ServerSocket.Start("0.0.0.0", 17888, 1024);//10.0.28.15

        while (true)
        {
            string inputStr = Console.ReadLine();
            if (inputStr == "Quit")
            {
                ServerSocket.Close();
                break;
            }
            else if (inputStr == "K")
            {
                HotUpdateMsg msg = new HotUpdateMsg();
                ServerSocket.BroadcastMsg(msg);
            }
            else if (inputStr == "A")
            {
                PlayerData data = new PlayerData();

                //            RoleService.UpdateUserPropertyAsync("a123", new Dictionary<string, object>
                //{
                //    { nameof(data.IsOnline), false },      // 更新 IsOnline 属性为 false
                //    { nameof(data.ChargeMoney), 200 }      // 更新 ChargeMoney 属性为 100
                //});


                Console.WriteLine(JwtHelper.GenerateToken("a123", "测试人"));
            }
            else if(inputStr == "Z")
            {
                //RoleService.GetUserByAccountAsync("a123");
                RoleService.CreateUserAsync("a1", "132");
                RoleService.CreateUserAsync("a2", "132");
                RoleService.CreateUserAsync("a3", "132");
                RoleService.CreateUserAsync("a4", "132");
            }
            else if (inputStr == "B")
            {
                GuildService.CreateGuild("6816a8da-12d5-412d-a484-329250dc059a", "测试公会", "测试名字", "测试描述");
            }
            else if (inputStr == "Q") {

                //RoleService.LoginAsync("a123", "99999");
                RoleService.AddFriendAsync("50a757cd-932e-4338-b0ae-1d3eb2ed9530", "b23673d6-831f-4263-a1c6-5b6ade37f3ea");
                //TestAsync();
                //RoleService.CreateUserAsync("a123", "123456");
                //RoleService.ChangePasswordAsync("a123", "123456","66666");
            }
            else if (inputStr == "W")
            {
                RoleService.AcceptFriendRequestAsync("50a757cd-932e-4338-b0ae-1d3eb2ed9530", "b23673d6-831f-4263-a1c6-5b6ade37f3ea");
            }
            else if (inputStr == "E")
            {
                //RoleService.DeleteFriendAsync("50a757cd-932e-4338-b0ae-1d3eb2ed9530", "b23673d6-831f-4263-a1c6-5b6ade37f3ea");
                RoleService.BlockFriendAsync("b23673d6-831f-4263-a1c6-5b6ade37f3ea", "50a757cd-932e-4338-b0ae-1d3eb2ed9530",true);
            }
            else if (inputStr == "R")
            {
                //RoleService.GetFriendListAsync("b23673d6-831f-4263-a1c6-5b6ade37f3ea");
                //EmailService.SendEmail("root", "b23673d6-831f-4263-a1c6-5b6ade37f3ea", "测试标题", "测试内容");
                //EmailService.SendEmailToAllPlayersAsync("root", "测试全服邮件标题", "全服测试内容123124125151254");

                //ChatService.GetAnnouncementMessages(1, 200); // 获取公告频道的最新30条消息
                //ChatService.ClearPublicChannelMessagesAsync();

                TestAsync1();
            }
        }
    }

    private static async Task TestAsync1()
    {
        for (int i = 0; i < 10; i++)
        {
            ChatMsg chatMsg1 = new ChatMsg();
            //chatMsg1.ReceiverId = "870d7675-ca81-11ef-848d-20906fc57f0e";
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

    private static async Task TestAsync()
    {
        int numPlayers = 100;  // 假设模拟 50 名玩家

        List<Task> tasks = new List<Task>();
        // 记录整个测试的开始时间
        DateTime testStartTime = DateTime.Now;
        Console.WriteLine($"测试开始时间: {testStartTime:HH:mm:ss.fff}");

        for (int i = 0; i < numPlayers; i++)
        {
            string userUuid = Guid.NewGuid().ToString();
            string token = JwtHelper.GenerateToken(userUuid, "测试");

            // 在每个请求开始时记录开始时间
            DateTime requestStartTime = DateTime.Now;

            // 模拟登录请求
            tasks.Add(RoleService.LoginAsync("a123", "123456"));
        }

        // 等待所有任务完成
        await Task.WhenAll(tasks);

        // 记录整个测试的结束时间
        DateTime testEndTime = DateTime.Now;
        TimeSpan testDuration = testEndTime - testStartTime;
        Console.WriteLine($"测试结束时间: {testEndTime:HH:mm:ss.fff}");
        Console.WriteLine($"整个测试持续时间: {testDuration.TotalSeconds} 秒");

        Console.WriteLine("所有登录请求已完成");
    }

}
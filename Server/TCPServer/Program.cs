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
        ServerSocket.Start("0.0.0.0", 8080, 1024);//10.0.28.15

        while (true)
        {
            string inputStr = Console.ReadLine();
            if (inputStr == "Quit")
            {
                ServerSocket.Close();
                break;
            }
            //else if (inputStr.Substring(0, 2) == "B:")
            //{
            //    BaseMessage msg = new BaseMessage();
            //    msg.SenderId = "-1";
            //    ServerSocket.BroadcastMsg(msg);
            //}
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
                RoleService.GetUserByAccountAsync("a123");
            }
            else if (inputStr == "B")
            {
                GuildService.CreateGuild("11111", "测试公会", "测试名字", "测试描述");
            }
            else if (inputStr == "Q") {

                //RoleService.LoginAsync("a123", "123456");

                TestAsync();

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
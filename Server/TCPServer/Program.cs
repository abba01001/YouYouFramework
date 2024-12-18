using Protocols;
using Protocols.Player;
using System;
using System.Collections.Generic;
using System.Text;
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

                RoleService.LoginAsync("a123", "123456");
            }
        }
    }
}
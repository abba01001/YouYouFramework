using Protocols;
using System;
using System.Collections.Generic;
using System.Text;
using TCPServer.Core;
using TCPServer.Core.DataAccess;
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
            else if(inputStr == "A")
            {
                string query = "SELECT * FROM register_data";
                List<Dictionary<string, object>> result = SqlManager.Instance.ExecuteQuery(query);

                foreach (var row in result)
                {
                    Console.WriteLine($"ID: {row["id"]}, UUID: {row["user_uuid"]}, Account: {row["user_account"]}, Password: {row["user_password"]}, RegisterTime: {row["register_time"]}");
                }
                RequestHandler request = new RequestHandler(null);
                request.c2s_request_guild_list();

            }
        }
    }
}
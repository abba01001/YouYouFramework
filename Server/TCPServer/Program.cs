using Protocols;
using System;
using System.Text;

namespace TCPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerSocket.Start("0.0.0.0", 8080, 1024);//10.0.28.15

            while (true)
            {
                string inputStr = Console.ReadLine();
                if(inputStr == "Quit")
                {
                    ServerSocket.Close();
                    break;
                }
                else if (inputStr.Substring(0, 2) == "B:")
                {
                    BaseMessage msg = new BaseMessage();
                    msg.SenderId = "-1";
                    ServerSocket.BroadcastMsg(msg);
                }
            }
        }
    }
}

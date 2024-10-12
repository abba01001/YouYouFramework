using System;
using System.Text;

namespace TCPServer
{
    class Program
    {
        private static ServerSocket serverSocket;
        static void Main(string[] args)
        {
            serverSocket = new ServerSocket();
            serverSocket.Start("127.0.0.1", 8080, 1024);//10.0.28.15

            while (true)
            {
                string inputStr = Console.ReadLine();
                if(inputStr == "Quit")
                {
                    serverSocket.Close();
                    break;
                }
                else if (inputStr.Substring(0, 2) == "B:")
                {
                    serverSocket.BroadcastMsg(Encoding.UTF8.GetBytes(inputStr.Substring(2)), -1);
                }
            }
        }
    }
}

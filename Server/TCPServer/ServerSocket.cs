using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TCPServer
{
    public class ServerSocket
    {
        private Socket socket;
        private bool isClose;
        //private Dictionary<int, ClientSocket> clientDic = new Dictionary<int, ClientSocket>();
        private List<ClientSocket> clientList = new List<ClientSocket>();

        public void Start(string ip, int port, int clientNum)
        {
            isClose = false;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Bind(iPEndPoint);
            Console.WriteLine("服务器启动成功...IP:{0},端口:{1}", ip, port);
            Console.WriteLine("开始监听客户端连接...");
            socket.Listen(clientNum);

            ThreadPool.QueueUserWorkItem(AcceptClientConnect);
            ThreadPool.QueueUserWorkItem(ReceiveMsg);
        }

        //等待客户端连接
        private void AcceptClientConnect(object obj)
        {
            Console.WriteLine("等待客户端连入...");
            while (!isClose)
            {
                Socket clientSocket = socket.Accept();
                ClientSocket client = new ClientSocket(clientSocket, this);
                Console.WriteLine("客户端{0}连入...", clientSocket.RemoteEndPoint.ToString());
                //client.SendMsg("欢迎连入服务端...");
                //clientDic.Add(client.clientID, client);
                clientList.Add(client);
            }
        }

        //接收消息
        private void ReceiveMsg(object obj)
        {
            int i;
            while (!isClose)
            {
                //if(clientDic.Count > 0)
                if (clientList.Count > 0)
                {
                    //foreach(ClientSocket client in clientDic.Values)
                    for(i = 0; i < clientList.Count; i++)
                    {
                        try
                        {
                            //client.ReceiveClientMsg();
                            if (clientList[i] != null)
                            {
                                clientList[i].ReceiveClientMsg();
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
            }
        }

        //广播消息
        public void BroadcastMsg(byte[] msg, int clientID)
        {
            if (isClose)
                return;
            //foreach(ClientSocket client in clientDic.Values)
            for(int i = 0; i < clientList.Count; i++)
            {
                //client.SendMsg(msg);
                if(clientList[i].clientID != clientID)
                {
                    clientList[i].SendMsg(msg);
                }
            }
        }

        //释放连接
        public void Close()
        {
            isClose = true;
            //foreach (ClientSocket client in clientDic.Values)
            for(int i = 0; i < clientList.Count; i++)
            {
                //client.Close();
                clientList[i].Close();
            }

            //clientDic.Clear();
            clientList.Clear();

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket = null;
        }
    }
}

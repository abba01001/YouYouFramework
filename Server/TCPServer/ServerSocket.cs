using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Google.Protobuf;
using Protocols;

namespace TCPServer
{
    public class ServerSocket
    {
        private Socket socket;
        private bool isClose;
        private List<ClientSocket> clientList = new List<ClientSocket>();
        private object lockObj = new object(); // 用于线程安全的锁

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
            ThreadPool.QueueUserWorkItem(ReceiveClientMsg);
        }

        private void AcceptClientConnect(object obj)
        {
            Console.WriteLine("等待客户端连入...");
            while (!isClose)
            {
                try
                {
                    Socket clientSocket = socket.Accept();
                    ClientSocket client = new ClientSocket(clientSocket, this);
                    Console.WriteLine("IP:{0}连入...已连接IP数{1}", clientSocket.RemoteEndPoint.ToString(), ClientSocket.CLIENT_COUNT);
                    lock (lockObj) // 确保线程安全
                    {
                        clientList.Add(client);
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"SocketException: {e.Message}");
                }
            }
        }

        private void ReceiveClientMsg(object obj)
        {
            while (!isClose)
            {
                lock (lockObj)
                {
                    for (int i = 0; i < clientList.Count; i++)
                    {
                        try
                        {
                            clientList[i]?.ReceiveClientMsg();
                        }
                        catch (SocketException e)
                        {
                            Console.WriteLine($"SocketException: {e.Message}");
                            clientList[i].Close();
                            clientList.RemoveAt(i);
                            i--;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Exception: {e.Message}");
                        }
                    }
                }
            }
        }

        private void RemoveClient(int index)
        {
            clientList[index].Close();
            clientList.RemoveAt(index);
        }

        //广播信息
        public void BroadcastMsg(BaseMessage message)
        {
            if (isClose)
                return;

            byte[] msgBytes = message.ToByteArray(); // 使用 Protobuf 序列化消息
            lock (lockObj) // 确保线程安全
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    if (clientList[i].clientID != int.Parse(message.SenderId))
                    {
                        clientList[i].SendMsg(msgBytes);
                    }
                }
            }
        }

        public void Close()
        {
            isClose = true;
            lock (lockObj) // 确保线程安全
            {
                foreach (var client in clientList)
                {
                    client.Close();
                }
                clientList.Clear();
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket = null;
        }
    }
}

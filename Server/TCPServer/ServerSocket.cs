using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Protocols;
using TCPServer.Core;
using TCPServer.Core.DataAccess;
using TCPServer.Utils;
public static class ServerSocket
{
    private static Socket socket;
    private static bool isClose;
    private static ConcurrentBag<ClientSocket> clientList = new ConcurrentBag<ClientSocket>();
    public static NetLogger Logger;

    private static CancellationTokenSource cancellationTokenSource;
    private static Task acceptClientTask;
    private static Task receiveClientTask;

    public static async Task Start(string ip, int port, int clientNum)
    {
        isClose = false;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Logger = new NetLogger(TimeSpan.FromSeconds(10));
        IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        socket.Bind(iPEndPoint);
        Logger.LogMessage(socket, $"服务器启动成功...IP:{ip},端口:{port}");
        Logger.LogMessage(socket, "开始监听客户端连接...");
        socket.Listen(clientNum);

        // 保留您原始的字符串格式，添加连接池参数

    //    SqlManager.Initialize(
    //$"Server={KeyUtils.GetSqlKey(SqlKey.Server)};" +
    //$"Database={KeyUtils.GetSqlKey(SqlKey.Database)};" +
    //$"UserId={KeyUtils.GetSqlKey(SqlKey.UserId)};" +
    //$"Password={KeyUtils.GetSqlKey(SqlKey.Password)};" +
    //$"Port={KeyUtils.GetSqlKey(SqlKey.Port)};" +
    //"Pooling=true;MinPoolSize=20;MaxPoolSize=600;ConnectionTimeout=30;"
//);


        SqlManager.Initialize($"Server={KeyUtils.GetSqlKey(SqlKey.Server)};Database={KeyUtils.GetSqlKey(SqlKey.Database)};" +
           $"UserId={KeyUtils.GetSqlKey(SqlKey.UserId)};Password={KeyUtils.GetSqlKey(SqlKey.Password)};Port = {KeyUtils.GetSqlKey(SqlKey.Port)}");

        cancellationTokenSource = new CancellationTokenSource();
        acceptClientTask = AcceptClientConnectAsync(cancellationTokenSource.Token);
        receiveClientTask = ReceiveClientMsgAsync(cancellationTokenSource.Token);
    }

    private static async Task AcceptClientConnectAsync(CancellationToken cancellationToken)
    {
        Logger.LogMessage(socket, "等待客户端连入...");
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                Socket clientSocket = await Task.Factory.FromAsync(socket.BeginAccept, socket.EndAccept, null);
                ClientSocket client = new ClientSocket(clientSocket);
                Logger.LogMessage(socket, $"IP:{clientSocket.RemoteEndPoint}连入...已连接IP数{ClientSocket.CLIENT_COUNT}");
                clientList.Add(client);
            }
            catch (SocketException e)
            {
                Console.WriteLine($"SocketException: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"AcceptClientConnectAsync Exception: {e.Message}");
            }
        }
    }

    public static async Task ReceiveClientMsgAsync(CancellationToken cancellationToken)
    {
        var tasks = new List<Task>(); // 存储所有接收消息的任务
        while (!isClose && !cancellationToken.IsCancellationRequested)
        {
            // 并行处理每个客户端的消息接收
            foreach (var client in clientList)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await client.ReceiveMsgAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"ReceiveClientMsg Exception: {e.Message}");
                    }
                }));
            }

            // 等待所有任务完成
            await Task.WhenAll(tasks);
            tasks.Clear(); // 清空已完成的任务列表，准备下一轮
            await Task.Delay(100); // 防止高 CPU 占用
        }
    }

    // 异步广播消息
    public static void BroadcastMsg(BaseMessage message)
    {
        if (isClose)
            return;

        byte[] msgBytes = message.ToByteArray(); // 使用 Protobuf 序列化消息
        var sendTasks = clientList.Select(client => client.SendMsg(msgBytes));
        Task.WhenAll(sendTasks).Wait(); // 等待所有任务完成
    }


    public static void Close()
    {
        isClose = true;
        cancellationTokenSource?.Cancel();
        Task.WhenAll(acceptClientTask, receiveClientTask).Wait(); // 等待所有接收和接入任务完成

        foreach (var client in clientList)
        {
            client.Close();
        }

        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        socket = null;
    }
}

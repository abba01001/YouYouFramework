using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Newtonsoft.Json;
using TCPServer.Core.DataAccess;
using TCPServer.Core;
using TCPServer.Core.Services;
using TCPServer.Utils;


public static class ServerSocket
{
    private static Socket socket;
    private static bool isClose;
    private static readonly ConcurrentBag<ClientSocket> clientList = new();
    private static CancellationTokenSource cancellationTokenSource;
    private static Task acceptClientTask;
    private static Task receiveClientTask;
    private static Timer midnightTimer;

    private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "Key", "task_state.json");
    private static readonly string DirectoryPath = Path.GetDirectoryName(FilePath) ?? AppContext.BaseDirectory;

    private static long serverStartTimestamp;

    public static long CurrentServerTimestamp =>
        serverStartTimestamp + (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - serverStartTimestamp);

    private class TaskState
    {
        public List<DateTime> ExecutionHistory { get; set; } = new();
    }

    private static async Task<TaskState> GetTaskStateAsync()
    {
        if (File.Exists(FilePath))
        {
            try
            {
                string jsonContent = await File.ReadAllTextAsync(FilePath);
                return JsonConvert.DeserializeObject<TaskState>(jsonContent) ?? new TaskState();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取任务状态时发生错误: {ex.Message}");
            }
        }
        return new TaskState();
    }

    private static async Task UpdateTaskExecutionTimeAsync(DateTime executedTime)
    {
        try
        {
            // 确保目录存在
            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);

            var taskState = await GetTaskStateAsync();
            taskState.ExecutionHistory.Add(executedTime);

            string jsonContent = JsonConvert.SerializeObject(taskState, Formatting.Indented);
            await File.WriteAllTextAsync(FilePath, jsonContent);

            Console.WriteLine($"任务执行时间已更新: {executedTime}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新任务状态时发生错误: {ex.Message}");
        }
    }

    private static async void PerformMidnightTask(object state)
    {
        //这里每天做一些清理事件
        Console.WriteLine("到达0点，执行定时任务...");
        DateTime executedTime = DateTime.UtcNow;
        await UpdateTaskExecutionTimeAsync(executedTime);
        await ChatService.ClearPublicChannelMessagesAsync();
        await RoleService.ResetSuspendParams();
        Console.WriteLine("检测到新的一天，任务已执行并记录。");
    }

    private static async void HandleDailyTasks()
    {
        TaskState taskState = await GetTaskStateAsync();
        DateTime lastExecuted = taskState.ExecutionHistory.LastOrDefault();
        DateTime now = DateTime.UtcNow;

        if (lastExecuted.Date < now.Date) PerformMidnightTask(null);

        TimeSpan timeUntilMidnight = DateTime.Today.AddDays(1) - now;
        midnightTimer = new Timer(PerformMidnightTask, null, timeUntilMidnight, TimeSpan.FromDays(1));
    }

    public static async Task Start(string ip, int port, int clientNum)
    {
        isClose = false;
        serverStartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
        socket.Listen(clientNum);
        SqlManager.Initialize($"Server={KeyUtils.GetSqlKey(SqlKey.Server)};Database={KeyUtils.GetSqlKey(SqlKey.Database)};" +
          $"UserId={KeyUtils.GetSqlKey(SqlKey.UserId)};Password={KeyUtils.GetSqlKey(SqlKey.Password)};Port = {KeyUtils.GetSqlKey(SqlKey.Port)}");
        Console.WriteLine($"服务器启动成功: IP={ip}, 端口={port}");

        cancellationTokenSource = new CancellationTokenSource();
        acceptClientTask = AcceptClientConnectAsync(cancellationTokenSource.Token);
        receiveClientTask = ReceiveClientMsgAsync(cancellationTokenSource.Token);

        HandleDailyTasks();
    }

    private static async Task AcceptClientConnectAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("等待客户端连接...");
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                Socket clientSocket = await Task.Factory.FromAsync(socket.BeginAccept, socket.EndAccept, null);
                var client = new ClientSocket(clientSocket);
                clientList.Add(client);
                Console.WriteLine($"新客户端连接: {clientSocket.RemoteEndPoint}, 当前连接数: {clientList.Count}");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"SocketException: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AcceptClientConnectAsync Exception: {ex.Message}");
            }
        }
    }

    private static async Task ReceiveClientMsgAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("开始接收客户端消息...");
        while (!isClose && !cancellationToken.IsCancellationRequested)
        {
            var tasks = clientList.Select(client => Task.Run(async () =>
            {
                try
                {
                    await client.ReceiveMsgAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"消息接收异常: {ex.Message}");
                }
            })).ToList();

            await Task.WhenAll(tasks);
            await Task.Delay(100); // 防止高 CPU 占用
        }
    }

    public static async Task BroadcastMsg<T>(T data) where T : IMessage<T>
    {
        if (isClose) return;

        var tasks = clientList.Select(client => client.Request.SendMessage(data));
        await Task.WhenAll(tasks);
        Console.WriteLine("消息广播完成");
    }

    public static void Close()
    {
        isClose = true;
        cancellationTokenSource?.Cancel();

        try
        {
            Task.WhenAll(acceptClientTask, receiveClientTask).Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"关闭时发生错误: {ex.Message}");
        }

        foreach (var client in clientList)
        {
            client.Close();
        }

        socket?.Close();
        Console.WriteLine("服务器已关闭");
    }
}

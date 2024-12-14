using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Sockets;
using UnityEngine;

public class NetLogger
{
    private readonly string logFilePath;
    private readonly ConcurrentQueue<string> logQueue;
    private readonly TimeSpan writeInterval;
    private readonly CancellationTokenSource cancellationTokenSource;

    public NetLogger(TimeSpan writeInterval)
    {
        logFilePath = Path.Combine(Application.persistentDataPath, "logfile.txt");
        // 确保日志目录存在
        string logDirectory = Path.GetDirectoryName(logFilePath);
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        logQueue = new ConcurrentQueue<string>();
        this.writeInterval = writeInterval;
        cancellationTokenSource = new CancellationTokenSource();
        StartLogWriter();
    }

    public void LogMessage(Socket socket, string message)
    {
        logQueue.Enqueue($"{DateTime.UtcNow}---{socket.RemoteEndPoint}---{message}");
    }

    private void StartLogWriter()
    {
        Task.Run(async () =>
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                await WriteLogAsync();
                await Task.Delay(writeInterval);
            }
        });
    }

    private async Task WriteLogAsync()
    {
        if (logQueue.IsEmpty) return;

        var logBuilder = new StringBuilder();
        while (logQueue.TryDequeue(out var logMessage))
        {
            logBuilder.AppendLine(logMessage);
        }

        // 追加日志内容到文件
        await File.AppendAllTextAsync(logFilePath, logBuilder.ToString());
    }

    public void Stop()
    {
        cancellationTokenSource.Cancel();
    }
}
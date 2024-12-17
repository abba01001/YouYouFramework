using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Sockets;

public class NetLogger
{
    private readonly string logFilePath;
    private readonly ConcurrentQueue<string> logQueue;
    private readonly TimeSpan writeInterval;
    private readonly CancellationTokenSource cancellationTokenSource;

    public NetLogger(TimeSpan writeInterval)
    {
        string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Log");
        Directory.CreateDirectory(logDirectory); // 创建目录（如果不存在）
        logFilePath = Path.Combine(logDirectory, "logfile.txt");

        logQueue = new ConcurrentQueue<string>();
        this.writeInterval = writeInterval;
        cancellationTokenSource = new CancellationTokenSource();
        StartLogWriter();
    }

    public void LogMessage(Socket socket, string message)
    {
        if (socket != null)
        {
            logQueue.Enqueue($"{DateTime.Now}---{socket.RemoteEndPoint}---{message}");
        }
        Console.WriteLine(message);
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
        //await File.AppendAllTextAsync(logFilePath, logBuilder.ToString());
    }

    public void Stop()
    {
        cancellationTokenSource.Cancel();
    }
}
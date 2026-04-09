using StackExchange.Redis;
using System;

public class RedisManager
{
    private static readonly Lazy<RedisManager> _instance =
        new Lazy<RedisManager>(() => new RedisManager());

    public static RedisManager Instance => _instance.Value;

    private ConnectionMultiplexer _conn;
    private IDatabase _db;

    private readonly object _lock = new object();
    private string _connectionString;

    public IDatabase DB => _db;

    private RedisManager() { }

    /// <summary>
    /// 初始化（服务器启动时调用一次）
    /// </summary>
    public void Init(string connectionString)
    {
        _connectionString = connectionString;
        Connect();
    }

    /// <summary>
    /// 建立连接
    /// </summary>
    private void Connect()
    {
        lock (_lock)
        {
            if (_conn != null && _conn.IsConnected)
                return;

            try
            {
                var options = ConfigurationOptions.Parse(_connectionString);
                options.AbortOnConnectFail = false; // 关键：允许自动重连
                options.ConnectRetry = 3;
                options.ConnectTimeout = 5000;
                options.KeepAlive = 60;

                _conn = ConnectionMultiplexer.Connect(options);

                // 注册事件
                RegisterEvents(_conn);

                _db = _conn.GetDatabase();

                Console.WriteLine("[Redis] 连接成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis] 连接失败: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// 注册连接事件
    /// </summary>
    private void RegisterEvents(ConnectionMultiplexer conn)
    {
        conn.ConnectionFailed += (sender, args) =>
        {
            Console.WriteLine($"[Redis] 连接失败: {args.EndPoint}, {args.FailureType}");
        };

        conn.ConnectionRestored += (sender, args) =>
        {
            Console.WriteLine($"[Redis] 连接恢复: {args.EndPoint}");
        };

        conn.ErrorMessage += (sender, args) =>
        {
            Console.WriteLine($"[Redis] 错误: {args.Message}");
        };

        conn.InternalError += (sender, args) =>
        {
            Console.WriteLine($"[Redis] 内部错误: {args.Exception.Message}");
        };

        conn.ConfigurationChanged += (sender, args) =>
        {
            Console.WriteLine($"[Redis] 配置变更: {args.EndPoint}");
        };
    }

    /// <summary>
    /// 获取数据库（可指定DB）
    /// </summary>
    public IDatabase GetDB(int db = -1)
    {
        if (_conn == null || !_conn.IsConnected)
        {
            Console.WriteLine("[Redis] 断线，尝试重连...");
            Connect();
        }

        return _conn.GetDatabase(db);
    }

    /// <summary>
    /// 获取Server（用于Keys、Flush等）
    /// </summary>
    public IServer GetServer()
    {
        var endpoint = _conn.GetEndPoints()[0];
        return _conn.GetServer(endpoint);
    }

    /// <summary>
    /// 发布消息
    /// </summary>
    public long Publish(string channel, string message)
    {
        var sub = _conn.GetSubscriber();
        return sub.Publish(channel, message);
    }

    /// <summary>
    /// 订阅消息
    /// </summary>
    public void Subscribe(string channel, Action<string> handler)
    {
        var sub = _conn.GetSubscriber();

        sub.Subscribe(channel, (ch, msg) =>
        {
            try
            {
                handler?.Invoke(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis] Subscribe Error: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// 创建批处理（Pipeline）
    /// </summary>
    public IBatch CreateBatch()
    {
        return GetDB().CreateBatch();
    }

    /// <summary>
    /// 创建事务（不常用）
    /// </summary>
    public ITransaction CreateTransaction()
    {
        return GetDB().CreateTransaction();
    }

    /// <summary>
    /// 关闭连接（服务器关闭时）
    /// </summary>
    public void Close()
    {
        try
        {
            _conn?.Close();
            _conn?.Dispose();
            Console.WriteLine("[Redis] 已关闭");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Redis] 关闭异常: {ex.Message}");
        }
    }
}
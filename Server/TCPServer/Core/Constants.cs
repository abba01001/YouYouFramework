public enum OperationResult
{
    Success = 1,
    UserNotFound = 0,
    PasswordIncorrect = 2,
    Failed = 3,
    PropertyNotFound = 4,
    NotFound = 5,
    NotBlocked = 6,
    AlreadyBlocked = 7,
    UserAlreadyOnline = 8,
}


public class Constants
{
    public static string SECURITYKEY = "3ZkPqF9hDjW8q2Z7"; //钥匙
    public static int BLOCK_SIZE = 16; // AES块大小
    public const int ProtocalHeadLength = 41;
    public const int ProtocalTotalLength = 1024;
}

public static class SqlKey
{
    public static string Server { get; set; } = "Server";
    public static string Database { get; set; } = "Database";
    public static string UserId { get; set; } = "User ID";
    public static string Password { get; set; } = "Password";
    public static string Port { get; set; } = "Port";
}

public static class SqlTable
{
    public static string GameSaveData { get; set; } = "game_saves";
    public static string GuildList { get; set; } = "guild_list";
    public static string GuildMembers { get; set; } = "guild_members";
    public static string EmailList { get; set; } = "email_list";
    public static string ChatMessages { get; set; } = "chat_messages";
    public static string PlayerBase { get; set; } = "player_base";
    
}

public static class Token
{
    public static string TokenKey { get; set; } = "TokenKey";
}

public static class RedisKey
{
    #region 全局RedisKey

    // 【全局公用】数据持久化刷盘队列 🔥 新增这一行
    public static string DbFlushQueue => "db:flush";
    public static string OnlinePlayers => "online:players";

    #endregion

    #region 个人RedisKey
    /// 玩家脏数据标记（便签：需要存库）
    public static string PlayerDirty(string uid) => $"player:dirty:{uid}";
    /// 玩家数据修改次数统计（累计N次触发存库）
    public static string PlayerDirtyCount(string uid) => $"player:dirty:count:{uid}";
    /// 玩家最后一次刷盘时间（间隔N秒触发存库）
    public static string PlayerLastSaveTime(string uid) => $"player:dirty:lastsave:{uid}";
    
    // 玩家基础信息
    public static string PlayerBase(string uid) => $"player:base:{uid}"; // ID, Name, Level, Exp

    // 玩家背包
    public static string PlayerBag(string uid) => $"player:bag:{uid}"; // 列表 / Hash 存储道具

    // 玩家货币
    public static string PlayerCurrency(string uid) => $"player:currency:{uid}"; // Hash: Gold, Gems

    // 玩家好友
    public static string PlayerFriends(string uid) => $"player:friends:{uid}"; // Set

    // 排行榜 / 排名
    public static string PlayerRank(string uid) => $"player:rank:{uid}"; // SortedSet

    // 临时缓存 / session
    public static string PlayerSession(string uid) => $"player:session:{uid}"; // 登录会话 Key

    // 玩家位置和旋转信息
    // 玩家位置：使用 Hash 存储位置坐标 (x, y, z)
    public static string PlayerPosition(string uid) => $"player:position:{uid}"; // Hash: x, y, z

    // 玩家旋转：使用 Hash 存储旋转信息 (pitch, yaw, roll)
    public static string PlayerRotation(string uid) => $"player:rotation:{uid}"; // Hash: pitch, yaw, roll

    #endregion
}
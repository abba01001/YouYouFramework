public static class RedisKey
{
    // ======================
    // 【全局公用】数据持久化刷盘队列 🔥 新增这一行
    // ======================
    public static string DbFlushQueue => "db:flush";

    // ----------------------
    // 玩家基础信息
    // ----------------------
    public static string PlayerBase(string uid) => $"player:base:{uid}"; // ID, Name, Level, Exp

    // ----------------------
    // 玩家背包
    // ----------------------
    public static string PlayerBag(string uid) => $"player:bag:{uid}"; // 列表 / Hash 存储道具

    // ----------------------
    // 玩家货币
    // ----------------------
    public static string PlayerCurrency(string uid) => $"player:currency:{uid}"; // Hash: Gold, Gems

    // ----------------------
    // 玩家好友
    // ----------------------
    public static string PlayerFriends(string uid) => $"player:friends:{uid}"; // Set

    // ----------------------
    // 排行榜 / 排名
    // ----------------------
    public static string PlayerRank(string uid) => $"player:rank:{uid}"; // SortedSet

    // ----------------------
    // 临时缓存 / session
    // ----------------------
    public static string PlayerSession(string uid) => $"player:session:{uid}"; // 登录会话 Key
    
    // ----------------------
    // 玩家位置和旋转信息
    // ----------------------
    // 玩家位置：使用 Hash 存储位置坐标 (x, y, z)
    public static string PlayerPosition(string uid) => $"player:position:{uid}"; // Hash: x, y, z

    // 玩家旋转：使用 Hash 存储旋转信息 (pitch, yaw, roll)
    public static string PlayerRotation(string uid) => $"player:rotation:{uid}"; // Hash: pitch, yaw, roll
}
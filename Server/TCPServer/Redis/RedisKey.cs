public static class RedisKey
{
    // ----------------------
    // 玩家基础信息
    // ----------------------
    public static string PlayerBase(long uid) => $"player:base:{uid}"; // ID, Name, Level, Exp

    // ----------------------
    // 玩家背包
    // ----------------------
    public static string PlayerBag(long uid) => $"player:bag:{uid}"; // 列表 / Hash 存储道具

    // ----------------------
    // 玩家货币
    // ----------------------
    public static string PlayerCurrency(long uid) => $"player:currency:{uid}"; // Hash: Gold, Gems

    // ----------------------
    // 玩家好友
    // ----------------------
    public static string PlayerFriends(long uid) => $"player:friends:{uid}"; // Set

    // ----------------------
    // 排行榜 / 排名
    // ----------------------
    public static string PlayerRank(long uid) => $"player:rank:{uid}"; // SortedSet

    // ----------------------
    // 临时缓存 / session
    // ----------------------
    public static string PlayerSession(long uid) => $"player:session:{uid}"; // 登录会话 Key
}
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class RedisHelper
{
    private static IDatabase DB => RedisManager.Instance.DB;

    #region String

    public static Task<bool> SetStringAsync(string key, string value, int expireSeconds = 0)
    {
        if (expireSeconds > 0)
            return DB.StringSetAsync(key, value, TimeSpan.FromSeconds(expireSeconds));
        else
            return DB.StringSetAsync(key, value);
    }

    public static async Task<string> GetStringAsync(string key)
    {
        RedisValue value = await DB.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    #endregion

    #region Hash

    public static Task HashSetAsync(string key, string field, string value)
    {
        return DB.HashSetAsync(key, field, value);
    }

    public static Task<RedisValue> HashGetAsync(string key, string field)
    {
        return DB.HashGetAsync(key, field);
    }

    public static Task HashSetAsync(string key, HashEntry[] entries)
    {
        return DB.HashSetAsync(key, entries);
    }

    public static Task<RedisValue[]> HashGetAllAsync(string key)
    {
        return DB.HashValuesAsync(key);
    }

    public static Task<long> HashIncrementAsync(string key, string field, long value)
    {
        return DB.HashIncrementAsync(key, field, value);
    }

    #endregion

    #region List

    public static Task<long> ListLeftPushAsync(string key, string value)
    {
        return DB.ListLeftPushAsync(key, value);
    }
    

    public static async Task ListRightPushAsync(string key, string value)
    {
        await DB.ListRightPushAsync(key, value);
    }

    public static async Task<string> ListLeftPopAsync(string key)
    {
        var val = await DB.ListLeftPopAsync(key);
        return val.HasValue ? val.ToString() : null;
    }

    public static async Task<List<string>> ListRangeAsync(string key, long start = 0, long stop = -1)
    {
        var vals = await DB.ListRangeAsync(key, start, stop);
        var list = new List<string>();
        foreach (var v in vals)
            list.Add(v.ToString());
        return list;
    }

    public static async Task<long> ListLengthAsync(string key)
    {
        return await DB.ListLengthAsync(key);
    }

    #endregion

    #region SortedSet（排行榜）

    public static Task<bool> ZAddAsync(string key, string member, double score)
    {
        return DB.SortedSetAddAsync(key, member, score);
    }

    public static Task<SortedSetEntry[]> ZRangeAsync(string key, int start, int stop, Order order = Order.Descending)
    {
        return DB.SortedSetRangeByRankWithScoresAsync(key, start, stop, order);
    }

    public static Task<double?> ZScoreAsync(string key, string member)
    {
        return DB.SortedSetScoreAsync(key, member);
    }

    #endregion

    #region Key

    public static Task<bool> KeyExpireAsync(string key, int seconds)
    {
        return DB.KeyExpireAsync(key, TimeSpan.FromSeconds(seconds));
    }

    public static Task<bool> KeyDeleteAsync(string key)
    {
        return DB.KeyDeleteAsync(key);
    }

    #endregion
}
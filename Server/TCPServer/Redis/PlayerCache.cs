using StackExchange.Redis;
using System.Threading.Tasks;

public class PlayerCache
{
    // ---------------- 基础数据 ----------------

    public async Task SetBase(string uid, int level, int exp)
    {
        var key = RedisKey.PlayerBase(uid);

        await RedisHelper.HashSetAsync(key, new[]
        {
            new HashEntry("level", level),
            new HashEntry("exp", exp),
        });

        MarkDirty(uid);
    }

    public async Task<HashEntry[]> GetBase(string uid)
    {
        var key = RedisKey.PlayerBase(uid);
        return await RedisManager.Instance.GetDB().HashGetAllAsync(key);
    }

    // ---------------- 货币（高频！） ----------------

    public async Task<long> AddGold(string uid, int value)
    {
        var key = RedisKey.PlayerCurrency(uid);

        var result = await RedisHelper.HashIncrementAsync(key, "gold", value);

        MarkDirty(uid);

        return result;
    }

    public async Task<long> GetGold(string uid)
    {
        var key = RedisKey.PlayerCurrency(uid);
        var val = await RedisHelper.HashGetAsync(key, "gold");

        return val.IsNull ? 0 : (long)val;
    }

    // ---------------- 背包 ----------------

    public async Task AddItem(string uid, int itemId, int count)
    {
        var key = RedisKey.PlayerBag(uid);

        await RedisHelper.HashIncrementAsync(key, $"item_{itemId}", count);

        MarkDirty(uid);
    }

    // ---------------- 脏标记 ----------------

    private void MarkDirty(string uid)
    {
        var key = $"dirty:player:{uid}";

        // 设置过期防止积压
        RedisManager.Instance.GetDB().StringSet(key, "1", System.TimeSpan.FromMinutes(10));

        // 推入队列
        RedisManager.Instance.GetDB().ListLeftPush("db:flush", uid);
    }
}
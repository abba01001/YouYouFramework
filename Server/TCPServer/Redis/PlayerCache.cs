using System;
using System.Collections.Generic;
using StackExchange.Redis;
using System.Numerics;
using System.Threading.Tasks;

public class PlayerCache
{
    // ---------------- 基础数据 ----------------

    public async Task SetBase(string uid, int level, int exp,Vector3 position,Vector3 rotation)
    {
        var key = RedisKey.PlayerBase(uid);

        await RedisHelper.HashSetAsync(key, new[]
        {
            new HashEntry("level", level),
            new HashEntry("exp", exp),
        });
        await SetPosAndRot(uid, position, rotation,false);
        MarkDirtyAsync(uid);
    }

    public async Task SetPosAndRot(string uid,Vector3 position, Vector3 rotation,bool markDirty = true)
    {
        var key = RedisKey.PlayerBase(uid);
        await RedisHelper.HashSetAsync(key, new[]
        {
            new HashEntry("pos_x", position.X),
            new HashEntry("pos_y", position.Y),
            new HashEntry("pos_z", position.Z),
        
            new HashEntry("rot_x", rotation.X),
            new HashEntry("rot_y", rotation.Y),
            new HashEntry("rot_z", rotation.Z),
        });

        MarkDirtyAsync(uid);
    }

    public async Task<HashEntry[]> GetPlayerBase(string uid)
    {
        var key = RedisKey.PlayerBase(uid);
        return await RedisHelper.HashGetAllAsync(key);
    }
    
    /// <summary>
    /// 从HashEntry[]数据中还原玩家位置和旋转
    /// </summary>
    public  (Vector3 position, Vector3 rotation) ParsePosRot(HashEntry[] data)
    {
        Dictionary<string, float> dict = new Dictionary<string, float>();
        foreach (var entry in data)
        {
            if (float.TryParse(entry.Value.ToString(), out float val))
                dict[entry.Name] = val;
        }

        Vector3 pos = new Vector3(
            dict.GetValueOrDefault("pos_x"),
            dict.GetValueOrDefault("pos_y"),
            dict.GetValueOrDefault("pos_z")
        );

        Vector3 rot = new Vector3(
            dict.GetValueOrDefault("rot_x"),
            dict.GetValueOrDefault("rot_y"),
            dict.GetValueOrDefault("rot_z")
        );

        return (pos, rot);
    }

    // ---------------- 货币（高频！） ----------------

    public async Task<long> AddGold(string uid, int value)
    {
        var key = RedisKey.PlayerCurrency(uid);

        var result = await RedisHelper.HashIncrementAsync(key, "gold", value);

        MarkDirtyAsync(uid);

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

        MarkDirtyAsync(uid);
    }

    // ---------------- 脏标记 ----------------
    // 优化后的 智能脏标记（定时+定次 自动刷盘）
    // 核心规则：5秒内 / 累计改3次 → 只存1次
    const int MAX_MODIFY_COUNT = 50;
    const int SAVE_INTERVAL_SEC = 20;
    
    private static async Task MarkDirtyAsync(string uid)
    {
        var db = RedisManager.Instance.DB;

        // 直接用规范Key，不再硬编码
        var dirtyKey = RedisKey.PlayerDirty(uid);
        var countKey = RedisKey.PlayerDirtyCount(uid);
        var lastSaveKey = RedisKey.PlayerLastSaveTime(uid);

        // 累加修改次数
        long count = await RedisHelper.StringIncrementAsync(countKey);
        long now = ServerSocket.CurrentServerTimestamp;
        bool needSave = false;

        // 条件1：累计修改满3次
        if (count >= MAX_MODIFY_COUNT)
        {
            needSave = true;
        }

        // 条件2：超过5秒未保存
        var lastSaveTime = await RedisHelper.GetStringAsync(lastSaveKey);
        if (string.IsNullOrEmpty(lastSaveTime) || (now - long.Parse(lastSaveTime)) > TimeSpan.FromSeconds(SAVE_INTERVAL_SEC).Ticks)
        {
            needSave = true;
        }

        if (needSave)
        {
            // 推入刷盘队列
            await RedisHelper.StringSetAsync(dirtyKey, "1", 10);
            await RedisHelper.ListLeftPushAsync(RedisKey.DbFlushQueue, uid);

            // 更新最后保存时间 + 清空计数
            await RedisHelper.StringSetAsync(lastSaveKey, now.ToString());
            await RedisHelper.KeyDeleteAsync(countKey);
        }
    }
}
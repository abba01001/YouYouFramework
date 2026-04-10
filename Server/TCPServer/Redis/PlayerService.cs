using System.Threading.Tasks;

public class PlayerService
{
    private static PlayerCache _cache = new PlayerCache();

    /// <summary>
    /// 玩家登录
    /// </summary>
    public static async Task OnLogin(string uid)
    {
        var db = RedisManager.Instance.GetDB();

        // 1. 判断 Redis 是否有
        bool exists = await db.KeyExistsAsync(RedisKey.PlayerBase(uid));

        if (!exists)
        {
            // 2. 没有 → 从 MySQL 加载
            var data = await LoadFromMySQL(uid);

            // 3. 写入 Redis
            await _cache.SetBase(uid, data.Level, data.Exp,new System.Numerics.Vector3(1,2,3),new System.Numerics.Vector3(5,0,8));

            await RedisHelper.HashSetAsync(RedisKey.PlayerCurrency(uid), "gold", data.Gold.ToString());
        }

        // 4. 标记在线
        await db.SetAddAsync("online:players", uid);
    }

    /// <summary>
    /// 玩家下线
    /// </summary>
    public async Task OnLogout(string uid)
    {
        var db = RedisManager.Instance.GetDB();

        // 标记需要立即刷库
        await db.ListLeftPushAsync("db:flush", uid);

        // 移除在线
        await db.SetRemoveAsync("online:players", uid);
    }

    /// <summary>
    /// 模拟 MySQL 读取
    /// </summary>
    private static async Task<(int Level, int Exp, int Gold)> LoadFromMySQL(string uid)
    {
        // TODO: 你自己接 MySQL
        await Task.Delay(10);

        return (1, 0, 1000);
    }
}
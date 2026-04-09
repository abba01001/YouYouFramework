using System;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

public class DBFlushService
{
    private bool _running = true;

    public void Start()
    {
        Task.Run(async () =>
        {
            Console.WriteLine("[DBFlush] 启动");

            while (_running)
            {
                try
                {
                    var db = RedisManager.Instance.GetDB();

                    // 从队列取
                    var uidValue = await db.ListRightPopAsync("db:flush");

                    if (uidValue.IsNull)
                    {
                        await Task.Delay(100); // 空队列
                        continue;
                    }

                    long uid = (long)uidValue;

                    await FlushPlayer(uid);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[DBFlush] 错误: " + ex.Message);
                }
            }
        });
    }

    private async Task FlushPlayer(long uid)
    {
        var db = RedisManager.Instance.GetDB();

        var baseData = await db.HashGetAllAsync(RedisKey.PlayerBase(uid));
        var currency = await db.HashGetAllAsync(RedisKey.PlayerCurrency(uid));
        var bag = await db.HashGetAllAsync(RedisKey.PlayerBag(uid));

        // 👉 这里写入 MySQL
        await SaveToMySQL(uid, baseData, currency, bag);

        Console.WriteLine($"[DBFlush] 已刷盘 uid={uid}");
    }

    private async Task SaveToMySQL(long uid,
        HashEntry[] baseData,
        HashEntry[] currency,
        HashEntry[] bag)
    {
        // TODO: 你接 MySQL
        await Task.Delay(5);
    }
}
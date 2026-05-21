using MySqlConnector;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCPServer.Core.DataAccess;

public class DBFlushService
{
    private bool _running = true;

    public void Start()
    {
        Task.Run(async () =>
        {
            LoggerHelper.Instance.Info("[DBFlush] 数据持久化服务已启动");
            while (_running)
            {
                try
                {
                    var db = RedisManager.Instance.GetDB();
                    var uidValue = await db.ListRightPopAsync(RedisKey.DbFlushQueue);

                    if (uidValue.IsNull)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    string uid = uidValue.ToString();
                    await FlushPlayer(uid);
                }
                catch (Exception ex)
                {
                    LoggerHelper.Instance.Error($"[DBFlush] 运行异常: {ex.Message}");
                }
            }
            LoggerHelper.Instance.Info("[DBFlush] 数据持久化服务已停止");
        });
    }

    public void Stop() => _running = false;

    private async Task FlushPlayer(string uid)
    {
        try
        {
            var db = RedisManager.Instance.GetDB();
            var baseData = await db.HashGetAllAsync(RedisKey.PlayerBase(uid));
            var currency = await db.HashGetAllAsync(RedisKey.PlayerCurrency(uid));
            var bag = await db.HashGetAllAsync(RedisKey.PlayerBag(uid));

            if (baseData.Length == 0 && currency.Length == 0 && bag.Length == 0)
            {
                LoggerHelper.Instance.Debug($"[DBFlush] 玩家{uid}无数据，跳过");
                return;
            }
            await SaveToMySQL(uid, baseData, currency, bag);
            LoggerHelper.Instance.Warn($"[DBFlush] 玩家{uid} 全动态刷盘成功");
        }
        catch (Exception ex)
        {
            LoggerHelper.Instance.Error($"[DBFlush] 玩家{uid} 刷盘失败: {ex.Message}");
        }
    }

    // ==============================================
    // ✅ 全动态自动存储：零硬编码，自动适配所有字段
    // ==============================================
    private async Task SaveToMySQL(string uid, HashEntry[] baseData, HashEntry[] currencyData, HashEntry[] bagData)
    {
        var baseDict = HashToDict(baseData);
        var currencyDict = HashToDict(currencyData);
        var bagDict = HashToDict(bagData);

        await SqlManager.Instance.ExecuteTransactionAsync(async (conn, tran) =>
        {
            // 1. 动态保存玩家基础信息（自动识别所有字段）
            if (baseDict.Count > 0)
                await DynamicSaveAsync(SqlTable.PlayerBase, uid, baseDict, conn, tran);

            //// 2. 动态保存玩家货币（自动识别所有字段）
            //if (currencyDict.Count > 0)
            //    await DynamicSaveAsync("player_currency", uid, currencyDict, conn, tran);

            //// 3. 动态保存背包（不变，本来就是动态）
            //if (bagDict.Count > 0)
            //    await SaveBagDynamicAsync(uid, bagDict, conn, tran);

            return true;
        });
    }

    // ==============================================
    // ✅ 核心：动态保存任意Hash表 → 自动SQL，零硬编码
    // ==============================================
    private async Task DynamicSaveAsync(string tableName, string uid, Dictionary<string, string> dataDict, MySqlConnection conn, MySqlTransaction tran)
    {
        // 字段列表
        var fields = new List<string> { "uid" };
        // 参数占位符
        var values = new List<string> { "@uid" };
        // 更新语句
        var updates = new List<string>();
        // 参数
        var parameters = new Dictionary<string, object>
        {
            { "@uid", uid }
        };

        int index = 0;
        foreach (var kv in dataDict)
        {
            string field = kv.Key;
            string param = $"@val{index}";
            fields.Add(field);
            values.Add(param);
            updates.Add($"{field}={param}");
            parameters.Add(param, kv.Value);
            index++;
        }

        // 动态生成最终SQL
        string sql = $@"
            INSERT INTO {tableName} ({string.Join(",", fields)})
            VALUES ({string.Join(",", values)})
            ON DUPLICATE KEY UPDATE {string.Join(",", updates)};
        ";

        await SqlManager.Instance.ExecuteNonQueryAsync(sql, parameters, conn, tran);
    }

    // ==============================================
    // ✅ 背包动态批量保存
    // ==============================================
    //private async Task SaveBagDynamicAsync(string uid, Dictionary<string, string> bagDict, MySqlConnection conn, MySqlTransaction tran)
    //{
    //    // 清空旧背包
    //    await SqlManager.Instance.ExecuteNonQueryAsync(
    //        "DELETE FROM player_bag WHERE uid=@uid",
    //        new Dictionary<string, object> { { "@uid", uid } },
    //        conn, tran);

    //    var values = new List<string>();
    //    var parameters = new Dictionary<string, object> { { "@uid", uid } };
    //    int index = 0;

    //    foreach (var item in bagDict)
    //    {
    //        values.Add($"(@uid, @itemId_{index}, @count_{index})");
    //        parameters[$"@itemId_{index}"] = item.Key;
    //        parameters[$"@count_{index}"] = item.Value;
    //        index++;
    //    }

    //    string sql = $@"
    //        INSERT INTO player_bag (uid, item_id, item_count)
    //        VALUES {string.Join(",", values)};
    //    ";

    //    await SqlManager.Instance.ExecuteNonQueryAsync(sql, parameters, conn, tran);
    //}

    // 工具：Hash转字典
    private Dictionary<string, string> HashToDict(HashEntry[] entries)
    {
        var dict = new Dictionary<string, string>();
        foreach (var entry in entries)
            dict[entry.Name.ToString()] = entry.Value.ToString();
        return dict;
    }
}
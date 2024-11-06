using System;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using UnityEngine;

namespace YouYou
{
    public class SQLManager
    {
        private MySqlConnection connection;

        public SQLManager() { }

        // 初始化数据库连接
        public async Task InitConnect()
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = SecurityUtil.GetSqlKeyDic()["Server"],
                UserID = SecurityUtil.GetSqlKeyDic()["User ID"],
                Password = SecurityUtil.GetSqlKeyDic()["Password"],
                Database = SecurityUtil.GetSqlKeyDic()["Database"],
                Port = UInt32.Parse(SecurityUtil.GetSqlKeyDic()["Port"]),
                CharacterSet = "utf8mb4"
            };

            connection = new MySqlConnection(builder.ToString());
            await connection.OpenAsync();
            await connection.CloseAsync();
            Debug.Log("数据库连接正常");
        }

        // 统一打开和关闭连接
        private async Task<T> ExecuteWithConnectionAsync<T>(Func<MySqlCommand, Task<T>> action, bool useLongConnection = false)
        {
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync();

            await using var cmd = connection.CreateCommand();
            try
            {
                return await action(cmd);
            }
            finally
            {
                // 如果使用短连接，确保每次都关闭连接
                if (!useLongConnection && connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }

        // 注册新账户
        public async Task RegisterAccountAsync(string accountId, string passWord)
        {
            if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(passWord))
            {
                Debug.LogError("请输入账号和密码");
                return;
            }

            if (await InsertAsync(accountId, passWord))
            {
                Debug.Log("注册成功");
                GameEntry.Data.LoadGameData();
            }
            else
            {
                Debug.LogError("注册失败");
            }
        }

        // 登录
        public async Task LoginAsync(string account, string password)
        {
            if (await FindAsync(account))
            {
                var (isValid, uuid) = await ValidateUserAsync(account, password);
                if (isValid)
                {
                    GameEntry.Data.UserId = uuid;
                    GameEntry.Data.LoadGameData();
                }
                else
                {
                    GameUtil.LogError("密码错误！");
                }
            }
            else
            {
                await RegisterAccountAsync(account, password);
            }
        }

        // 创建数据表
        public async Task CreateDataTableAsync()
        {
            string query = "CREATE TABLE IF NOT EXISTS register_data (" +
                           "id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                           "user_uuid CHAR(36) NOT NULL UNIQUE, " +
                           "user_account VARCHAR(50) NOT NULL UNIQUE, " +
                           "user_password VARCHAR(88) NOT NULL) " +
                           "CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci";

            await ExecuteWithConnectionAsync(async cmd =>
            {
                cmd.CommandText = query;
                await cmd.ExecuteNonQueryAsync();
                return true;
            });
        }

        // 插入新用户数据
        private async Task<bool> InsertAsync(string user_account, string user_password)
        {
            user_password = SecurityUtil.ConvertBase64Key(user_password); // 确保密码是加密的
            string query = "INSERT INTO register_data (user_uuid, user_account, user_password) VALUES(@uuid, @account, @password)";

            return await ExecuteWithConnectionAsync(async cmd =>
            {
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@uuid", GameEntry.Data.UserId);
                cmd.Parameters.AddWithValue("@account", user_account);
                cmd.Parameters.AddWithValue("@password", user_password);
                int result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            });
        }

        // 查找用户是否存在
        private async Task<bool> FindAsync(string user_account)
        {
            string query = "SELECT COUNT(*) FROM register_data WHERE user_account = @value";

            return await ExecuteWithConnectionAsync(async cmd =>
            {
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@value", user_account);
                int result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return result > 0;
            });
        }

        // 验证用户
        private async Task<(bool isValid, string user_uuid)> ValidateUserAsync(string user_account, string input_password)
        {
            string query = "SELECT user_uuid, user_password FROM register_data WHERE user_account = @account";
            return await ExecuteWithConnectionAsync(async cmd =>
            {
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@account", user_account);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var user_uuid = reader["user_uuid"].ToString();
                        var stored_password = reader["user_password"].ToString();
                        return (stored_password == SecurityUtil.GetBase64Key(input_password), user_uuid);
                    }
                }
                return (false, null); // 用户不存在或密码不匹配
            });
        }

        // 关闭数据库连接
        public async Task CloseConnectionAsync()
        {
            if (connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
    }
}

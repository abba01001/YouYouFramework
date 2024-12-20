using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServer.Core.DataAccess;
using System.Security.Cryptography;
using Protocols.Player;
using TCPServer.Utils;

namespace TCPServer.Core.Services
{
    public enum OperationResult
    {
        Success = 1,
        UserNotFound = 0,
        PasswordIncorrect = 2,
        UpdateFailed = 3,
        PropertyNotFound = 4
    }

    public class RoleService
    {
        // 创建账号
        public static async Task<OperationResult> CreateUserAsync(string userAccount, string userPassword)
        {
            int registerTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string query = @"
                INSERT INTO register_data 
                (user_uuid, user_account, user_password, register_time, online_duration, 
                 charge_money, final_login_time, final_exit_time, guild_id, is_online) 
                VALUES 
                (@user_uuid, @user_account, @user_password, @register_time, @online_duration, 
                 @charge_money, @final_login_time, @final_exit_time, @guild_id, @is_online);
            ";

            var parameters = new Dictionary<string, object>
            {
                { "@user_uuid", Guid.NewGuid().ToString() },
                { "@user_account", userAccount },
                { "@user_password", userPassword },
                { "@register_time", registerTime },
                { "@online_duration", 0 },
                { "@charge_money", 0 },
                { "@final_login_time", 0 },
                { "@final_exit_time", 0 },
                { "@guild_id", 0 },
                { "@is_online", 0 }
            };

            try
            {
                int rowsAffected = await SqlManager.Instance.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0 ? OperationResult.Success : OperationResult.UpdateFailed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return OperationResult.UpdateFailed;
            }
        }

        // 登录验证
        public static async Task<(OperationResult, string)> LoginAsync(string userAccount, string userPassword)
        {
            // 合并查询和更新为一个事务性操作，减少数据库交互
            string query = @"
        UPDATE register_data
        SET is_online = 1
        WHERE user_account = @user_account AND user_password = @user_password;
        
        SELECT user_uuid
        FROM register_data
        WHERE user_account = @user_account AND user_password = @user_password;
    ";

            var parameters = new Dictionary<string, object>
    {
        { "@user_account", userAccount },
        { "@user_password", userPassword }
    };

            var result = await SqlManager.Instance.ExecuteQueryAsync(query, parameters);

            if (result.Count == 0)
            {
                return (OperationResult.UserNotFound, null);
            }

            string userUuid = result[0]["user_uuid"].ToString();
            return (OperationResult.Success, userUuid);
        }


        // 改密码功能
        public static async Task<OperationResult> ChangePasswordAsync(string userAccount, string oldPassword, string newPassword)
        {
            // 先获取用户信息
            var result = await GetUserByAccountAsync(userAccount);
            if (result == null)
                return OperationResult.UserNotFound;
            string storedPassword = result["user_password"].ToString();
            // 验证旧密码是否正确
            if (storedPassword != oldPassword)
                return OperationResult.PasswordIncorrect;
            // 创建更新的属性字典
            var updatedAttrs = new Dictionary<string, object>
    {
        { "user_password", newPassword }  // 更新密码
    };
            return await UpdateUserPropertyAsync(userAccount, updatedAttrs);
        }


        // 获取用户信息
        public static async Task<Dictionary<string, object>> GetUserByAccountAsync(string userAccount)
        {
            string query = "SELECT * FROM register_data WHERE user_account = @user_account";
            var parameters = new Dictionary<string, object>
    {
        { "@user_account", userAccount }
    };

            var result = await SqlManager.Instance.ExecuteQueryAsync(query, parameters);

            // 如果查询结果有数据，打印所有的键值对
            if (result.Count > 0)
            {
                var userData = result[0];  // 获取第一个结果
                foreach (var kvp in userData)
                {
                    Console.WriteLine($"列名: {kvp.Key} === 值: {kvp.Value}");
                }

                return userData;
            }
            return null;
        }


        //更新用户任意属性
        public static async Task<OperationResult> UpdateUserPropertyAsync(string userAccount, Dictionary<string, object> updatedAttrs)
        {
            if (updatedAttrs == null || updatedAttrs.Count == 0)
                return OperationResult.UpdateFailed;

            if (!GlobalUtils.ValidateKey<PlayerData>(updatedAttrs)) return OperationResult.PropertyNotFound;

            var queryParts = new List<string>();
            var parameters = new Dictionary<string, object> { { "@user_account", userAccount } };


            // 遍历传入的字段
            foreach (var kvp in updatedAttrs)
            {
                string propertyName = kvp.Key;
                object value = kvp.Value;
                // 转换成数据库中的列名
                string columnName = ConvertToColumnName(propertyName);

                // 生成 SQL 更新语句
                queryParts.Add($"{columnName} = @{columnName}");
                parameters.Add($"@{columnName}", value);

                Console.WriteLine($"列名: {propertyName} === 值: {value}");  // 调试输出
            }

            if (queryParts.Count == 0) return OperationResult.UpdateFailed;

            // 构建 SQL 更新查询语句
            string updateQuery = $"UPDATE register_data SET {string.Join(", ", queryParts)} WHERE user_account = @user_account";

            try
            {
                // 执行更新
                int rowsAffected = await SqlManager.Instance.ExecuteNonQueryAsync(updateQuery, parameters);
                return rowsAffected > 0 ? OperationResult.Success : OperationResult.UpdateFailed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user properties: {ex.Message}");
                return OperationResult.UpdateFailed;
            }
        }


        // 自动将 Protobuf 的驼峰命名属性转换为下划线分隔的列名
        private static string ConvertToColumnName(string propertyName)
        {
            // 如果字段名为空，返回空
            if (string.IsNullOrEmpty(propertyName))
                return null;

            // 将驼峰命名转换为下划线分隔的小写命名（例如 UserUuid -> user_uuid）
            var columnName = string.Concat(propertyName
                .Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + char.ToLower(x) : char.ToLower(x).ToString()));

            return columnName;
        }
    }
}

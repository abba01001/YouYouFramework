using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServer.Core.DataAccess;
using System.Security.Cryptography;
using Protocols.Player;
using TCPServer.Utils;
using Protocols.Game;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;

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
            int currentTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();  // 当前时间戳（秒）
            string query = $@"
        INSERT INTO {SqlTable.GameSaveData} 
        (user_uuid, user_account, user_password, is_online, register_time, login_time) 
        VALUES 
        (@user_uuid, @user_account, @user_password, @is_online, @register_time, @login_time);
    ";

            var parameters = new Dictionary<string, object>
    {
        { "@user_uuid", Guid.NewGuid().ToString() },
        { "@user_account", userAccount },
        { "@user_password", userPassword },
        { "@is_online", 1 },  // 设置用户在线状态为 1
        { "@register_time", currentTime },  // 注册时间为当前时间
        { "@login_time", currentTime }  // 登录时间为当前时间
    };

            try
            {
                int rowsAffected = await SqlManager.Instance.ExecuteNonQueryAsync(query, parameters);
                Console.WriteLine($"玩家{userAccount}注册成功:{rowsAffected > 0}");
                return rowsAffected > 0 ? OperationResult.Success : OperationResult.UpdateFailed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return OperationResult.UpdateFailed;
            }
        }


        // 登录验证
        public static async Task<(OperationResult, string, byte[])> LoginAsync(string userAccount, string userPassword)
        {
            // 合并查询和更新为一个事务性操作，减少数据库交互
            string query = $@"
        UPDATE {SqlTable.GameSaveData} 
        SET is_online = 1
        WHERE user_account = @user_account AND user_password = @user_password;
        
        SELECT user_uuid, save_data
        FROM {SqlTable.GameSaveData}
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
                Console.WriteLine("User not found or invalid credentials.");
                return (OperationResult.UserNotFound, null, null);
            }

            string userUuid = result[0]["user_uuid"].ToString();
            byte[] saveData = result[0]["save_data"] as byte[] ?? new byte[0];
            return (OperationResult.Success, userUuid, saveData); // 返回 userUuid 和 saveData
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
            Console.WriteLine($"旧密码{oldPassword}===新密码{newPassword}");
            if (storedPassword != oldPassword)
                return OperationResult.PasswordIncorrect;
            // 创建更新的属性字典
            var updatedAttrs = new Dictionary<string, string>
    {
        { "UserPassword", newPassword }  // 更新密码
    };
            var(res, _) = await UpdateUserPropertyAsync(userAccount, updatedAttrs);
            return res;
        }


        // 获取用户信息
        public static async Task<Dictionary<string, object>> GetUserByAccountAsync(string userAccount)
        {
            string query = $"SELECT * FROM {SqlTable.GameSaveData} WHERE user_account = @user_account";
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
        public static async Task<(OperationResult Result, Dictionary<string, object> UpdatedValues)> UpdateUserPropertyAsync(string userUuid, Dictionary<string, string> updatedAttrs)
        {
            if (updatedAttrs == null || updatedAttrs.Count == 0)
                return (OperationResult.UpdateFailed, null);

            if (!GlobalUtils.ValidateKey<GameSaveData>(updatedAttrs))
                return (OperationResult.PropertyNotFound, null);

            var queryParts = new List<string>();
            var parameters = new Dictionary<string, object> { { "@user_uuid", userUuid } };

            // 遍历传入的字段
            foreach (var kvp in updatedAttrs)
            {
                string propertyName = kvp.Key;
                object value = kvp.Value;
                // 转换成数据库中的列名
                string columnName = ConvertToColumnName(propertyName);

                // 生成 SQL 更新语句
                queryParts.Add($"{columnName} = @{columnName}");

                if(columnName == "save_data")
                {
                    parameters.Add($"@{columnName}", Convert.FromBase64String(value as string));
                }
                else
                {
                    parameters.Add($"@{columnName}", value);
                }


                Console.WriteLine($"更新键: {columnName} === 更新值: {value}"); // 调试输出
            }

            if (queryParts.Count == 0)
                return (OperationResult.UpdateFailed, null);

            // 构建 SQL 更新查询语句
            string updateQuery = $"UPDATE {SqlTable.GameSaveData} SET {string.Join(", ", queryParts)} WHERE user_uuid = @user_uuid";

            try
            {
                // 执行更新
                int rowsAffected = await SqlManager.Instance.ExecuteNonQueryAsync(updateQuery, parameters);
                if (rowsAffected <= 0) return (OperationResult.UpdateFailed, null);

                // 查询更新后的数据
                string selectQuery = $"SELECT {string.Join(", ", updatedAttrs.Keys.Select(ConvertToColumnName))} FROM {SqlTable.GameSaveData} WHERE user_uuid = @user_uuid";
                var updatedValues = await SqlManager.Instance.ExecuteQueryAsync(selectQuery, parameters);
                Console.WriteLine("更新成功");
                // 返回更新结果和新数据
                return (OperationResult.Success, updatedValues.Count > 0 ? updatedValues[0] : null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user properties: {ex.Message}");
                return (OperationResult.UpdateFailed, null);
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

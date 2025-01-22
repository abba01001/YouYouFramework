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
using Protocols;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace TCPServer.Core.Services
{

    public class RoleService
    {
        //更新在线状态
        public static async Task UpdateUserOnlineStatus(string userUuid, bool isOnline)
        {
            // 定义 SQL 更新语句
            string query = $"UPDATE {SqlTable.GameSaveData} SET is_online = @IsOnline WHERE user_uuid = @UserUuid";

            // 设置参数
            var parameters = new Dictionary<string, object>
    {
        { "@IsOnline", isOnline},
        { "@UserUuid", userUuid }  // 使用 user_uuid 更新
    };

            // 执行更新操作
            await SqlManager.Instance.ExecuteNonQueryAsync(query, parameters);
        }

        // 创建账号
        public static async Task<(OperationResult, string)> CreateUserAsync(string userAccount, string userPassword)
        {
            int currentTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();  // 当前时间戳（秒）
            string uuid = Guid.NewGuid().ToString();
            string query = $@"
        INSERT INTO {SqlTable.GameSaveData} 
        (user_uuid, user_account, user_password, is_online, register_time, login_time) 
        VALUES 
        (@user_uuid, @user_account, @user_password, @is_online, @register_time, @login_time);
    ";

            var parameters = new Dictionary<string, object>
    {
        { "@user_uuid",uuid },
        { "@user_account", userAccount },
        { "@user_password", userPassword },
        { "@is_online", true },  // 添加在线状态参数
        { "@register_time", currentTime },  // 注册时间为当前时间
        { "@login_time", currentTime }  // 登录时间为当前时间
    };

            try
            {
                int rowsAffected = await SqlManager.Instance.ExecuteNonQueryAsync(query, parameters);
                LoggerHelper.Instance.Info($"玩家{userAccount}注册成功:{rowsAffected > 0}");
                return (rowsAffected > 0 ? OperationResult.Success : OperationResult.Failed, uuid);
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Error($"Error creating user: {ex.Message}");
                return (OperationResult.Failed, string.Empty);
            }
        }


        public static async Task<(OperationResult, string, byte[])> LoginAsync(string userAccount, string userPassword)
        {
            int currentTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // 1. 先检查用户是否已经在线

            var user = RoleService.GetOnlineUsers(userAccount);
            if (user != null)
            {
                // 如果用户已经在线，检查是否断线重连

                // 假设我们定义10分钟内的心跳包未更新的玩家为掉线用户
                if (user.IsDropUser)
                {
                    // 玩家断线超过10分钟，认为是掉线用户，允许重连
                    Console.WriteLine($"用户 {userAccount} 已在线，但超时断线，允许重连！");
                    // 重新登录，更新在线状态
                    user.RefreshIsOffLine(false);
                    user.RefreshHeartBeatTime();  // 重置心跳时间
                    return await ProcessLogin(userAccount, userPassword, currentTime);  // 调用登录处理逻辑
                }

                // 如果用户没有超时断线，防止多次登录
                Console.WriteLine($"用户 {userAccount} 已经在线，拒绝登录！");
                return (OperationResult.UserAlreadyOnline, string.Empty, new byte[0]);
            }

            // 2. 查询数据库，验证用户账号和密码
            string query = $@"
    UPDATE {SqlTable.GameSaveData}
    SET is_online = 1, 
        suspend_time_start = IF(suspend_time_start = 0, @current_time, suspend_time_start)
    WHERE user_account = @user_account AND user_password = @user_password;
    
    SELECT user_uuid, save_data, suspend_time_start
    FROM {SqlTable.GameSaveData}
    WHERE user_account = @user_account AND user_password = @user_password;
";

            var parameters = new Dictionary<string, object>
    {
        { "@user_account", userAccount },
        { "@user_password", userPassword },
        { "@current_time", currentTime }
    };

            var result = await SqlManager.Instance.ExecuteQueryAsync(query, parameters);

            if (result.Count == 0)
            {
                Console.WriteLine("User not found or invalid credentials.");
                return (OperationResult.UserNotFound, string.Empty, new byte[0]);
            }

            // 3. 获取用户的 user_uuid 和存档数据
            string userUuid = result[0]["user_uuid"].ToString();
            byte[] saveData = result[0]["save_data"] as byte[] ?? new byte[0];
            int suspendTimeStart = Convert.ToInt32(result[0]["suspend_time_start"]);

            // 4. 将用户添加到在线用户列表
            RoleService.RefreshOnlineUsers(1,userAccount, new OnlineUser
            {
                UserAccount = userAccount,
                UserUUID = userUuid,
                IsOffline = false,  // 设置为在线
                LastHeartbeatTime = DateTime.UtcNow,  // 设置心跳时间
                Socket = null  // 这里可以放置连接的 Socket 对象，取决于如何管理连接
            });
            return (OperationResult.Success, userUuid, saveData);
        }

        // 登录处理逻辑
        private static async Task<(OperationResult, string, byte[])> ProcessLogin(string userAccount, string userPassword, int currentTime)
        {
            // 这里是登录数据库验证和返回处理的核心逻辑
            string query = $@"
    UPDATE {SqlTable.GameSaveData}
    SET is_online = 1, 
        suspend_time_start = IF(suspend_time_start = 0, @current_time, suspend_time_start)
    WHERE user_account = @user_account AND user_password = @user_password;
    
    SELECT user_uuid, save_data, suspend_time_start
    FROM {SqlTable.GameSaveData}
    WHERE user_account = @user_account AND user_password = @user_password;
";

            var parameters = new Dictionary<string, object>
    {
        { "@user_account", userAccount },
        { "@user_password", userPassword },
        { "@current_time", currentTime }
    };

            var result = await SqlManager.Instance.ExecuteQueryAsync(query, parameters);

            if (result.Count == 0)
            {
                Console.WriteLine("User not found or invalid credentials.");
                return (OperationResult.UserNotFound, string.Empty, new byte[0]);
            }

            // 3. 获取用户的 user_uuid 和存档数据
            string userUuid = result[0]["user_uuid"].ToString();
            byte[] saveData = result[0]["save_data"] as byte[] ?? new byte[0];
            int suspendTimeStart = Convert.ToInt32(result[0]["suspend_time_start"]);

            return (OperationResult.Success, userUuid, saveData);
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

            try
            {
                // 执行查询
                var result = await SqlManager.Instance.ExecuteQueryAsync(query, parameters);
                LoggerHelper.Instance.Debug($"查询用户 {userAccount} 的结果：");
                // 如果查询结果有数据，打印所有的键值对
                if (result.Count > 0)
                {
                    var userData = result[0];  // 获取第一个结果
                    foreach (var kvp in userData)
                    {
                        LoggerHelper.Instance.Debug($"列名: {kvp.Key} === 值: {kvp.Value}");
                    }
                    return userData;
                }
                else
                {
                    LoggerHelper.Instance.Debug("没有找到符合条件的用户数据。");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Debug($"查询出错: {ex.Message}");
            }
            return null;
        }

        //查询用户属性
        public static async Task<(OperationResult Result, Dictionary<string, object> UserProperties)> GetUserPropertiesAsync(string userUuid, List<string> propertiesToQuery, bool isInGameData = true)
        {
            // 参数验证
            if (propertiesToQuery == null || propertiesToQuery.Count == 0)
                return (OperationResult.Failed, null);

            // 验证属性是否存在于 GameSaveData 中（如果需要）
            if (isInGameData && !GlobalUtils.ValidateKey<GameSaveData>(propertiesToQuery.ToDictionary(p => p, p => "")))
                return (OperationResult.PropertyNotFound, null);

            // 构建查询的字段部分
            var queryParts = propertiesToQuery.Select(p => ConvertToColumnName(p)).ToList();
            if (queryParts.Count == 0)
                return (OperationResult.Failed, null);

            // 构建 SQL 查询语句
            string selectQuery = $"SELECT {string.Join(", ", queryParts)} FROM {SqlTable.GameSaveData} WHERE user_uuid = @user_uuid";
            var parameters = new Dictionary<string, object> { { "@user_uuid", userUuid } };

            try
            {
                // 执行查询
                var queryResult = await SqlManager.Instance.ExecuteQueryAsync(selectQuery, parameters);

                // 判断查询结果
                if (queryResult?.Count > 0)
                {
                    // 返回查询结果
                    return (OperationResult.Success, queryResult[0]);
                }
                else
                {
                    // 未查询到数据
                    return (OperationResult.Failed, null);
                }
            }
            catch (Exception ex)
            {
                // 记录错误并返回失败
                LoggerHelper.Instance.Error($"Error querying user properties: {ex.Message}");
                return (OperationResult.Failed, null);
            }
        }

        //更新用户属性
        public static async Task<(OperationResult Result, Dictionary<string, object> UpdatedValues)> UpdateUserPropertyAsync(string userUuid, Dictionary<string, string> updatedAttrs, bool isInGameData = true)
        {
            // 参数验证：如果 updatedAttrs 为空或没有任何字段，则返回失败
            if (updatedAttrs == null || updatedAttrs.Count == 0)
                return (OperationResult.Failed, null);

            // 如果需要验证属性是否存在于 GameSaveData 中
            if (isInGameData && !GlobalUtils.ValidateKey<GameSaveData>(updatedAttrs))
                return (OperationResult.PropertyNotFound, null);

            // 获取当前时间戳
            int currentTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var queryParts = new List<string> { "save_time = @save_time" };
            var parameters = new Dictionary<string, object>
    {
        { "@user_uuid", userUuid },
        { "@save_time", currentTime }
    };

            // 生成 SQL 更新语句的字段部分
            foreach (var kvp in updatedAttrs)
            {
                string propertyName = kvp.Key;
                object value = kvp.Value;

                string columnName = ConvertToColumnName(propertyName);
                queryParts.Add($"{columnName} = @{columnName}");

                // 针对特定字段进行转换
                if (columnName == "save_data")
                {
                    parameters[$"@{columnName}"] = Convert.FromBase64String(value as string);
                }
                else
                {
                    parameters[$"@{columnName}"] = value;
                }
            }

            // 如果没有需要更新的字段
            if (queryParts.Count == 1) return (OperationResult.Failed, null);

            // 构建 SQL 更新查询语句
            string updateQuery = $"UPDATE {SqlTable.GameSaveData} SET {string.Join(", ", queryParts)} WHERE user_uuid = @user_uuid";

            try
            {
                // 执行更新操作
                int rowsAffected = await SqlManager.Instance.ExecuteNonQueryAsync(updateQuery, parameters);
                if (rowsAffected <= 0) return (OperationResult.Failed, null);

                // 查询更新后的数据
                string selectQuery = $"SELECT {string.Join(", ", updatedAttrs.Keys.Select(ConvertToColumnName))} FROM {SqlTable.GameSaveData} WHERE user_uuid = @user_uuid";
                var updatedValues = await SqlManager.Instance.ExecuteQueryAsync(selectQuery, parameters);

                // 返回更新结果和新数据
                return (OperationResult.Success, updatedValues.Count > 0 ? updatedValues[0] : null);
            }
            catch (Exception ex)
            {
                // 错误处理
                LoggerHelper.Instance.Error($"Error updating user properties: {ex.Message}");
                return (OperationResult.Failed, null);
            }
        }

        // 自动将 Protobuf 的驼峰命名属性转换为下划线分隔的列名
        private static string ConvertToColumnName(string propertyName)
        {
            // 如果字段名为空，返回空
            if (string.IsNullOrEmpty(propertyName))
                return null;

            // 如果已经是下划线分隔的命名格式，直接返回原名称
            if (propertyName.Contains("_") && propertyName.All(c => char.IsLower(c) || c == '_'))
                return propertyName;

            // 将驼峰命名转换为下划线分隔的小写命名（例如 UserUuid -> user_uuid）
            var columnName = string.Concat(propertyName
                .Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + char.ToLower(x) : char.ToLower(x).ToString()));

            return columnName;
        }

      
        // 更新挂机时间为当前时间点
        public static async Task<OperationResult> UpdateSuspendTimeStartAsync(string userUuid)
        {
            var updatedAttrs = new Dictionary<string, string>
        {
            { "suspend_time_start", ((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString() }
        };

            var result = await UpdateUserPropertyAsync(userUuid, updatedAttrs,false);
            return result.Result;
        }

        // 更新快速领取奖励索引
        public static async Task<OperationResult> UpdateQuickGetSuspendRewardIndexAsync(string userUuid,int nowIndex)
        {
            var updatedAttrs = new Dictionary<string, string>
        {
            { "quick_get_suspend_reward_index", $"{nowIndex + 1}" }
        };

            var result = await UpdateUserPropertyAsync(userUuid, updatedAttrs,false);
            return result.Result;
        }

        // 获取 suspend_time_start, quick_get_suspend_reward_index 和 quick_get_suspend_reward_limit
        public static async Task<(int suspendTimeStart, int quickGetRewardIndex, int quickGetRewardLimit)> GetSuspendTimeParamsAsync(string userUuid)
        {

            (OperationResult result1, Dictionary< string, object> dic) = await GetUserPropertiesAsync(userUuid,new List<string>
            {
                "suspend_time_start","quick_get_suspend_reward_index","quick_get_suspend_reward_limit"
            },false);
            foreach (var item in dic)
            {
                Console.WriteLine($"查询信息{item.Key}==={Convert.ToInt32(item.Value)}");
            }
            if (dic.Count > 0)
            {
                var suspendTimeStart = Convert.ToInt32(dic["suspend_time_start"]);
                var quickGetRewardIndex = Convert.ToInt32(dic["quick_get_suspend_reward_index"]);
                var quickGetRewardLimit = Convert.ToInt32(dic["quick_get_suspend_reward_limit"]);

                return (suspendTimeStart, quickGetRewardIndex, quickGetRewardLimit);
            }
            return (0, 0, 0);
        }

        // 重置quick_get_suspend_reward_index为0
        public static async Task<OperationResult> ResetSuspendParams()
        {
            // 定义更新 quick_get_suspend_reward_index 的查询语句
            string updateQuery = $"UPDATE {SqlTable.GameSaveData} SET quick_get_suspend_reward_index = 0 WHERE quick_get_suspend_reward_index != 0";
            try
            {
                // 异步执行更新操作
                int rowsUpdated = await SqlManager.Instance.ExecuteNonQueryAsync(updateQuery);
                LoggerHelper.Instance.Info($"Reset {rowsUpdated} rows for quick_get_suspend_reward_index to 0.");
                // 判断操作是否成功
                return rowsUpdated > 0 ? OperationResult.Success : OperationResult.Failed;
            }
            catch (Exception ex)
            {
                // 捕获并打印异常
                LoggerHelper.Instance.Error($"Error resetting suspend params: {ex.Message}");
                return OperationResult.Failed;
            }
        }

        // 判断用户是否可以领取奖励
        public static async Task<(OperationResult,SuspendTimeMsg)> CanClaimRewardAsync(string userUuid, int rewardType)
        {
            // 获取当前时间（秒级）
            int currentTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // 查询用户数据
            string query = $@"
        SELECT suspend_time_start, quick_get_suspend_reward_index, quick_get_suspend_reward_limit 
        FROM {SqlTable.GameSaveData} 
        WHERE user_uuid = @user_uuid;
    ";

            var parameters = new Dictionary<string, object>
    {
        { "@user_uuid", userUuid }
    };

            var result = await SqlManager.Instance.ExecuteQueryAsync(query, parameters);

            if (result.Count == 0)
            {
                Console.WriteLine("User not found.");
                return (OperationResult.UserNotFound, null);
            }

            var userData = result[0];
            int suspendTimeStart = Convert.ToInt32(userData["suspend_time_start"]);
            int quickGetRewardIndex = Convert.ToInt32(userData["quick_get_suspend_reward_index"]);
            int quickGetRewardLimit = Convert.ToInt32(userData["quick_get_suspend_reward_limit"]);
            switch (rewardType)
            {
                case 1: // 类型1：根据挂机时间来领取奖励
                    {
                        int hoursDifference = (currentTime - suspendTimeStart) / 3600; // 计算小时差
                        Console.WriteLine($"Hours difference: {hoursDifference}");

                        if (hoursDifference > 0)  // 如果小时差大于0，则可以领取
                        {
                            await UpdateSuspendTimeStartAsync(userUuid);
                            return (OperationResult.Success, new SuspendTimeMsg()
                            {
                                CanGetReward = true,
                                Hour = hoursDifference
                            });
                        }
                        else
                        {
                            Console.WriteLine("Insufficient hours to claim reward.");
                            return (OperationResult.Failed, null);
                        }
                    }
                case 2: // 类型2：根据索引和限制来领取奖励
                    {
                        if (quickGetRewardIndex < quickGetRewardLimit)  // 如果 quick_get_suspend_reward_index 小于 quick_get_suspend_reward_limit
                        {
                            Console.WriteLine($"类型2：根据索引和限制来领取奖励{quickGetRewardIndex}======{quickGetRewardLimit}");
                            await UpdateQuickGetSuspendRewardIndexAsync(userUuid,quickGetRewardIndex);
                            return (OperationResult.Success, new SuspendTimeMsg()
                            {
                                CanGetReward = true
                            });
                        }
                        else
                        {
                            Console.WriteLine("Cannot claim reward. Limit reached.");
                            return (OperationResult.Failed, null);
                        }
                    }
                default:
                    LoggerHelper.Instance.Error("Invalid reward type.");
                    return (OperationResult.Failed, null);
            }
        }


        // 维护一个字典，记录每个在线用户
        public static int OnlineUserCount => OnlineUsers.Count;
        private static Dictionary<string, OnlineUser> OnlineUsers { get; set; } = new();
        public static void RefreshOnlineUsers(int operate, string user_account, OnlineUser user = null)
        {
            if (operate == 1)
            {
                OnlineUsers.TryAdd(user_account, user);
                LoggerHelper.Instance.Info($"用户 {user_account} 成功连接服务器==当前连接用户数{OnlineUserCount}");
            }
            else if (operate == 2)
            {
                var t = GetOnlineUsers(user_account);
                if (t != null)
                {
                    _ = RoleService.UpdateUserOnlineStatus(t.UserUUID, false);
                    LoggerHelper.Instance.Info($"用户 {user_account} 断开连接服务器==当前连接用户数{OnlineUserCount - 1}");
                }
                OnlineUsers.Remove(user_account);
            }
            else if (operate == 3)
            {
                if (OnlineUsers.ContainsKey(user_account))
                {
                    OnlineUsers[user_account].RefreshHeartBeatTime();
                }
            }
        }
        public static OnlineUser GetOnlineUsers(string user_account)
        {
            if (OnlineUsers.ContainsKey(user_account)) return OnlineUsers[user_account];
            return null;
        }
        public class OnlineUser
        {
            public string UserAccount { get; set; }       // 玩家账号
            public string UserUUID { get; set; }
            public bool IsOffline { get; set; }            // 玩家是否离线
            public DateTime LastOfflineTime { get; set; }  // 玩家断线时间
            public DateTime LastHeartbeatTime { get; set; } // 玩家最后一次发送心跳包的时间
            public Socket Socket { get; set; }             // 玩家连接的Socket

            public void RefreshHeartBeatTime()
            {
                LastHeartbeatTime = DateTime.Now;
            }

            public void RefreshIsOffLine(bool isOnline)
            {
                IsOffline = isOnline;
            }

            // 假设我们定义10分钟内的心跳包未更新的玩家为掉线用户
            public bool IsDropUser => (DateTime.UtcNow - LastHeartbeatTime).TotalMinutes > 10;

        }

    }
}

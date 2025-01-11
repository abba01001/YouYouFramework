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

namespace TCPServer.Core.Services
{

    public class RoleService
    {
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
                Console.WriteLine($"玩家{userAccount}注册成功:{rowsAffected > 0}");
                return (rowsAffected > 0 ? OperationResult.Success : OperationResult.Failed, uuid);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return (OperationResult.Failed, string.Empty);
            }
        }


        // 登录验证
        public static async Task<(OperationResult, string, byte[])> LoginAsync(string userAccount, string userPassword)
        {
            int currentTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

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

            string userUuid = result[0]["user_uuid"].ToString();
            byte[] saveData = result[0]["save_data"] as byte[] ?? new byte[0];
            int suspendTimeStart = Convert.ToInt32(result[0]["suspend_time_start"]);

            // 登录成功，返回用户信息
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
                Console.WriteLine($"Error querying user properties: {ex.Message}");
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
                Console.WriteLine($"Error updating user properties: {ex.Message}");
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


        //添加好友
        public static async Task<OperationResult> AddFriendAsync(string userUuid, string friendUuid)
        {
            // 1. 检查当前用户是否已经是好友，或是否有其他状态（如已拒绝、已删除）
            string checkQuery = $@"
SELECT status FROM friend_list 
WHERE (user_uuid = @user_uuid AND friend_uuid = @friend_uuid) 
OR (user_uuid = @friend_uuid AND friend_uuid = @user_uuid)
";

            var checkParameters = new Dictionary<string, object>
    {
        { "@user_uuid", userUuid },
        { "@friend_uuid", friendUuid }
    };

            var checkResult = await SqlManager.Instance.ExecuteQueryAsync(checkQuery, checkParameters);

            // 如果已经是好友或者有其他状态（如已拒绝、已删除），返回相应的错误
            if (checkResult.Count > 0)
            {
                var status = (int)checkResult[0]["status"];
                if (status == 2) // 2 - 已同意
                {
                    Console.WriteLine("已经是好友了");
                    return OperationResult.Success;
                }
                else if (status == 3) // 3 - 已拒绝
                {
                    Console.WriteLine("好友请求已被拒绝");
                    return OperationResult.Failed;
                }
                else if (status == 4) // 4 - 已删除
                {
                    Console.WriteLine("好友关系已删除");
                    return OperationResult.Failed;
                }
            }

            // 2. 检查是否被对方拉黑
            string checkBlacklistQuery = @"
SELECT status 
FROM friend_list
WHERE (user_uuid = @friend_uuid AND friend_uuid = @user_uuid) 
AND status = 5
";

            var checkBlacklistParameters = new Dictionary<string, object>
    {
        { "@user_uuid", userUuid },
        { "@friend_uuid", friendUuid }
    };

            var blacklistResult = await SqlManager.Instance.ExecuteQueryAsync(checkBlacklistQuery, checkBlacklistParameters);

            // 如果存在 status = 5 记录，表示被对方拉黑，不能继续添加好友
            if (blacklistResult.Count > 0)
            {
                Console.WriteLine("您已被对方拉黑，无法添加好友");
                return OperationResult.Failed;  // 或者返回其他适合的错误码
            }

            // 3. 如果没有被拉黑且没有其他好友关系，创建两条好友请求数据
            string insertQuery = $@"
INSERT INTO friend_list (user_uuid, friend_uuid, status, is_invitor) 
VALUES (@user_uuid, @friend_uuid, 1, 1),  -- 发送请求的玩家, is_invitor = 1
       (@friend_uuid, @user_uuid, 1, 0);  -- 接收请求的玩家, is_invitor = 0
";

            var insertParameters = new Dictionary<string, object>
    {
        { "@user_uuid", userUuid },
        { "@friend_uuid", friendUuid }
    };

            try
            {
                int rowsAffected = await SqlManager.Instance.ExecuteNonQueryAsync(insertQuery, insertParameters);
                if (rowsAffected > 0)
                {
                    Console.WriteLine("好友请求已发送");
                    return OperationResult.Success;
                }
                else
                {
                    Console.WriteLine("添加好友失败");
                    return OperationResult.Failed;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding friend: {ex.Message}");
                return OperationResult.Failed;
            }
        }


        // 同意好友请求
        public static async Task<OperationResult> AcceptFriendRequestAsync(string userUuid, string friendUuid)
        {
            string updateQuery = $@"
    UPDATE friend_list 
    SET status = 2  -- 2 - 已同意
    WHERE 
        (user_uuid = @user_uuid AND friend_uuid = @friend_uuid OR user_uuid = @friend_uuid AND friend_uuid = @user_uuid) 
        AND status = 1  -- 只更新状态为请求中的记录
";

            var updateParameters = new Dictionary<string, object>
    {
        { "@user_uuid", userUuid },
        { "@friend_uuid", friendUuid }
    };

            try
            {
                int rowsAffected = await SqlManager.Instance.ExecuteNonQueryAsync(updateQuery, updateParameters);
                if (rowsAffected > 0)
                {
                    Console.WriteLine("好友请求已接受");
                    return OperationResult.Success;
                }
                else
                {
                    Console.WriteLine("接受好友请求失败");
                    return OperationResult.Failed;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting friend request: {ex.Message}");
                return OperationResult.Failed;
            }
        }

        // 删除好友
        public static async Task<OperationResult> DeleteFriendAsync(string userUuid, string friendUuid)
        {
            // 删除好友关系，删除两方记录
            string deleteQuery = $@"
    DELETE FROM friend_list 
    WHERE (user_uuid = @user_uuid AND friend_uuid = @friend_uuid) 
    OR (user_uuid = @friend_uuid AND friend_uuid = @user_uuid)
";

            var deleteParameters = new Dictionary<string, object>
    {
        { "@user_uuid", userUuid },
        { "@friend_uuid", friendUuid }
    };

            try
            {
                int rowsAffected = await SqlManager.Instance.ExecuteNonQueryAsync(deleteQuery, deleteParameters);
                if (rowsAffected > 0)
                {
                    Console.WriteLine("好友已删除");
                    return OperationResult.Success;
                }
                else
                {
                    Console.WriteLine("删除好友失败");
                    return OperationResult.Failed;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting friend: {ex.Message}");
                return OperationResult.Failed;
            }
        }

        //获取好友列表
        public static async Task<OperationResult> GetFriendListAsync(string userUuid)
        {
            // 查询用户的所有已同意的好友列表，去重，并且获取状态
            string query = @"
    SELECT DISTINCT 
        CASE
            WHEN user_uuid = @user_uuid THEN friend_uuid
            ELSE user_uuid
        END AS friend_uuid,
        is_online, 
        status
    FROM friend_list
    WHERE (user_uuid = @user_uuid OR friend_uuid = @user_uuid) 
    ";

            var parameters = new Dictionary<string, object>
    {
        { "@user_uuid", userUuid }
    };

            try
            {
                var result = await SqlManager.Instance.ExecuteQueryAsync(query, parameters);

                if (result.Count > 0)
                {
                    // 使用 StringBuilder 来构建批量输出
                    var friendList = new List<FriendInfo>();
                    var stringBuilder = new StringBuilder();

                    foreach (var row in result)
                    {
                        var friendUuid = (string)row["friend_uuid"]; // 处理唯一好友 ID
                        var isOnline = Convert.ToBoolean(row["is_online"]); // 使用 Convert.ToBoolean 来处理布尔类型
                        var status = Convert.ToInt32(row["status"]);

                        // 添加到好友列表
                        friendList.Add(new FriendInfo
                        {
                            FriendUuid = friendUuid,
                            IsOnline = isOnline,
                            Status = status
                        });

                        // 累积好友信息到 StringBuilder
                        stringBuilder.AppendLine($"Friend UUID: {friendUuid}, Is Online: {isOnline}, Status: {status}");
                    }

                    // 一次性打印所有好友列表信息
                    Console.WriteLine($"玩家{userUuid}获取好友列表成功:\n" + stringBuilder.ToString());

                    // 返回成功
                    return OperationResult.Success;
                }
                else
                {
                    Console.WriteLine("没有找到好友");
                    return OperationResult.PropertyNotFound;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting friend list: {ex.Message}");
                return OperationResult.PropertyNotFound;
            }
        }

        // 拉黑或解除拉黑好友
        public static async Task<OperationResult> BlockFriendAsync(string userUuid, string friendUuid, bool isBlock)
        {
            // 1. 检查是否已经有好友关系
            string checkQuery = @"
        SELECT status
        FROM friend_list
        WHERE (user_uuid = @user_uuid AND friend_uuid = @friend_uuid)
           OR (user_uuid = @friend_uuid AND friend_uuid = @user_uuid)
    ";

            var checkParameters = new Dictionary<string, object>
    {
        { "@user_uuid", userUuid },
        { "@friend_uuid", friendUuid }
    };

            try
            {
                // 执行查询
                var checkResult = await SqlManager.Instance.ExecuteQueryAsync(checkQuery, checkParameters);

                // 如果没有找到记录，表示没有好友关系
                if (checkResult.Count == 0)
                {
                    // 如果 isBlock 为 true，可以插入一条拉黑记录
                    if (isBlock)
                    {
                        string insertQuery = @"
                    INSERT INTO friend_list (user_uuid, friend_uuid, status, is_invitor)
                    VALUES (@user_uuid, @friend_uuid, 5, 1)  -- user_uuid 是拉黑者，status = 5 表示拉黑
                ";

                        int rowsAffected = await SqlManager.Instance.ExecuteNonQueryAsync(insertQuery, checkParameters);
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("已成功拉黑好友（尚未建立好友关系）");
                            return OperationResult.Success;
                        }
                        else
                        {
                            Console.WriteLine("创建拉黑记录失败");
                            return OperationResult.Failed;
                        }
                    }
                    else
                    {
                        // 如果 isBlock 为 false 且没有建立好友关系，无法解除拉黑
                        Console.WriteLine("无法解除拉黑，尚未建立好友关系");
                        return OperationResult.NotFound;
                    }
                }
                else
                {
                    // 获取当前的状态
                    int currentStatus = Convert.ToInt32(checkResult[0]["status"]);

                    // 2. 根据 isBlock 来决定是拉黑还是解除拉黑
                    if (isBlock)
                    {
                        // 拉黑操作，更新为拉黑状态（status = 5）
                        if (currentStatus != 5)  // 如果不是拉黑状态，则执行拉黑
                        {
                            string updateQuery = @"
                        UPDATE friend_list
                        SET status = 5  -- 5表示拉黑状态
                        WHERE (user_uuid = @user_uuid AND friend_uuid = @friend_uuid)
                           OR (user_uuid = @friend_uuid AND friend_uuid = @user_uuid)
                    ";

                            int rowsAffected = await SqlManager.Instance.ExecuteNonQueryAsync(updateQuery, checkParameters);
                            if (rowsAffected > 0)
                            {
                                Console.WriteLine("已成功拉黑好友");
                                return OperationResult.Success;
                            }
                            else
                            {
                                Console.WriteLine("拉黑失败");
                                return OperationResult.Failed;
                            }
                        }
                        else
                        {
                            Console.WriteLine("该好友已经处于拉黑状态");
                            return OperationResult.AlreadyBlocked;
                        }
                    }
                    else
                    {
                        // 解除拉黑操作，更新为已同意好友状态（status = 2）
                        if (currentStatus == 5)  // 只有当前状态是拉黑状态（status = 5）时才能解除拉黑
                        {
                            string updateQuery = @"
                        UPDATE friend_list
                        SET status = 2  -- 2表示已同意好友
                        WHERE (user_uuid = @user_uuid AND friend_uuid = @friend_uuid)
                           OR (user_uuid = @friend_uuid AND friend_uuid = @user_uuid)
                    ";

                            int rowsAffected = await SqlManager.Instance.ExecuteNonQueryAsync(updateQuery, checkParameters);
                            if (rowsAffected > 0)
                            {
                                Console.WriteLine("已成功解除拉黑好友");
                                return OperationResult.Success;
                            }
                            else
                            {
                                Console.WriteLine("解除拉黑失败");
                                return OperationResult.Failed;
                            }
                        }
                        else
                        {
                            Console.WriteLine("好友当前未处于拉黑状态，无法解除拉黑");
                            return OperationResult.NotBlocked;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error toggling block status for friend: {ex.Message}");
                return OperationResult.Failed;
            }
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
                Console.WriteLine($"Reset {rowsUpdated} rows for quick_get_suspend_reward_index to 0.");
                // 判断操作是否成功
                return rowsUpdated > 0 ? OperationResult.Success : OperationResult.Failed;
            }
            catch (Exception ex)
            {
                // 捕获并打印异常
                Console.WriteLine($"Error resetting suspend params: {ex.Message}");
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
                    Console.WriteLine("Invalid reward type.");
                    return (OperationResult.Failed, null);
            }
        }


        // 定义好友信息模型类
        public class FriendInfo
        {
            public string FriendUuid { get; set; }
            public bool IsOnline { get; set; }
            public int Status { get; set; }
        }


    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TCPServer.Core.DataAccess;

namespace TCPServer.Core.Services
{
    internal class FriendService
    {
        // 定义好友信息模型类
        public class FriendInfo
        {
            public string FriendUuid { get; set; }
            public bool IsOnline { get; set; }
            public int Status { get; set; }
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
                    LoggerHelper.Instance.Info("已经是好友了");
                    return OperationResult.Success;
                }
                else if (status == 3) // 3 - 已拒绝
                {
                    LoggerHelper.Instance.Info("好友请求已被拒绝");
                    return OperationResult.Failed;
                }
                else if (status == 4) // 4 - 已删除
                {
                    LoggerHelper.Instance.Info("好友关系已删除");
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

            var blacklistResult =
                await SqlManager.Instance.ExecuteQueryAsync(checkBlacklistQuery, checkBlacklistParameters);

            // 如果存在 status = 5 记录，表示被对方拉黑，不能继续添加好友
            if (blacklistResult.Count > 0)
            {
                LoggerHelper.Instance.Info("您已被对方拉黑，无法添加好友");
                return OperationResult.Failed; // 或者返回其他适合的错误码
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
                    LoggerHelper.Instance.Info("好友请求已发送");
                    return OperationResult.Success;
                }
                else
                {
                    LoggerHelper.Instance.Info("添加好友失败");
                    return OperationResult.Failed;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Info($"Error adding friend: {ex.Message}");
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
                    LoggerHelper.Instance.Info("好友请求已接受");
                    return OperationResult.Success;
                }
                else
                {
                    LoggerHelper.Instance.Info("接受好友请求失败");
                    return OperationResult.Failed;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Info($"Error accepting friend request: {ex.Message}");
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
                    LoggerHelper.Instance.Info("好友已删除");
                    return OperationResult.Success;
                }
                else
                {
                    LoggerHelper.Instance.Info("删除好友失败");
                    return OperationResult.Failed;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Info($"Error deleting friend: {ex.Message}");
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
                    LoggerHelper.Instance.Info($"玩家{userUuid}获取好友列表成功:\n" + stringBuilder.ToString());

                    // 返回成功
                    return OperationResult.Success;
                }
                else
                {
                    LoggerHelper.Instance.Info("没有找到好友");
                    return OperationResult.PropertyNotFound;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Info($"Error getting friend list: {ex.Message}");
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
                            LoggerHelper.Instance.Info("已成功拉黑好友（尚未建立好友关系）");
                            return OperationResult.Success;
                        }
                        else
                        {
                            LoggerHelper.Instance.Info("创建拉黑记录失败");
                            return OperationResult.Failed;
                        }
                    }
                    else
                    {
                        // 如果 isBlock 为 false 且没有建立好友关系，无法解除拉黑
                        LoggerHelper.Instance.Info("无法解除拉黑，尚未建立好友关系");
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
                        if (currentStatus != 5) // 如果不是拉黑状态，则执行拉黑
                        {
                            string updateQuery = @"
                        UPDATE friend_list
                        SET status = 5  -- 5表示拉黑状态
                        WHERE (user_uuid = @user_uuid AND friend_uuid = @friend_uuid)
                           OR (user_uuid = @friend_uuid AND friend_uuid = @user_uuid)
                    ";

                            int rowsAffected =
                                await SqlManager.Instance.ExecuteNonQueryAsync(updateQuery, checkParameters);
                            if (rowsAffected > 0)
                            {
                                LoggerHelper.Instance.Info("已成功拉黑好友");
                                return OperationResult.Success;
                            }
                            else
                            {
                                LoggerHelper.Instance.Info("拉黑失败");
                                return OperationResult.Failed;
                            }
                        }
                        else
                        {
                            LoggerHelper.Instance.Info("该好友已经处于拉黑状态");
                            return OperationResult.AlreadyBlocked;
                        }
                    }
                    else
                    {
                        // 解除拉黑操作，更新为已同意好友状态（status = 2）
                        if (currentStatus == 5) // 只有当前状态是拉黑状态（status = 5）时才能解除拉黑
                        {
                            string updateQuery = @"
                        UPDATE friend_list
                        SET status = 2  -- 2表示已同意好友
                        WHERE (user_uuid = @user_uuid AND friend_uuid = @friend_uuid)
                           OR (user_uuid = @friend_uuid AND friend_uuid = @user_uuid)
                    ";

                            int rowsAffected =
                                await SqlManager.Instance.ExecuteNonQueryAsync(updateQuery, checkParameters);
                            if (rowsAffected > 0)
                            {
                                LoggerHelper.Instance.Info("已成功解除拉黑好友");
                                return OperationResult.Success;
                            }
                            else
                            {
                                LoggerHelper.Instance.Info("解除拉黑失败");
                                return OperationResult.Failed;
                            }
                        }
                        else
                        {
                            LoggerHelper.Instance.Info("好友当前未处于拉黑状态，无法解除拉黑");
                            return OperationResult.NotBlocked;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Info($"Error toggling block status for friend: {ex.Message}");
                return OperationResult.Failed;
            }
        }
    }
}
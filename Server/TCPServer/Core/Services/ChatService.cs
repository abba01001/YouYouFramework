using Google.Protobuf.WellKnownTypes;
using Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TCPServer.Core.DataAccess;

namespace TCPServer.Core.Services
{
    public class ChatService
    {
        // 发送聊天消息
        public static async Task<OperationResult> SendMessageAsync(string senderId, string receiverId, string messageContent, int channelType, int messageType = 1)
        {
            string query = $@"
        INSERT INTO {SqlTable.ChatMessages} (sender_id, receiver_id, message, channel_type, is_read, message_type, is_deleted)
        VALUES (@senderId, @receiverId, @message, @channelType, @isRead, @messageType, @isDeleted)";

            var parameters = new Dictionary<string, object>
    {
        { "@senderId", senderId },
        { "@receiverId", receiverId },
        { "@message", messageContent },
        { "@channelType", channelType },
        { "@isRead", false },
        { "@messageType", messageType },
        { "@isDeleted", false }
    };

            try
            {
                // 使用异步方式执行 SQL 查询
                int result = await SqlManager.Instance.ExecuteNonQueryAsync(query, parameters);

                // 判断执行是否成功
                if (result > 0)
                {
                    return OperationResult.Success;
                }
                else
                {
                    return OperationResult.Failed;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Error("Error sending message: " + ex.Message);
                return OperationResult.Failed;
            }
        }

        public static async Task<OperationResult> SendMessageToAllPlayersAsync(string senderId, string messageContent)
        {
            try
            {
                await SendMessageAsync(senderId, "", messageContent, 1);
                return OperationResult.Success;
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Error("Error sending messages to all players: " + ex.Message);
                return OperationResult.Failed;
            }
        }

        // 获取用户的聊天消息列表
        private static async Task<List<ChatMsg>> GetUserMessages(string user_uuid, int channelType, int currentPage, int pageSize)
        {
            // 计算分页偏移量
            int offset = (currentPage - 1) * pageSize;

            string query = $@"
        SELECT id, sender_id, receiver_id, message, channel_type, 
               UNIX_TIMESTAMP(timestamp) AS timestamp, 
               is_read, message_type, is_deleted 
        FROM {SqlTable.ChatMessages} 
        WHERE ((receiver_id = @receiverId OR sender_id = @senderId) OR receiver_id IS NULL) 
              AND is_deleted = FALSE 
              AND channel_type = @channelType
        ORDER BY timestamp DESC
        LIMIT @offset, @pageSize";

            var parameters = new Dictionary<string, object>
    {
        { "@receiverId", user_uuid },
        { "@senderId", user_uuid },
        { "@channelType", channelType },
        { "@offset", offset },
        { "@pageSize", pageSize }
    };

            try
            {
                // 执行查询并获取结果
                List<Dictionary<string, object>> result = await SqlManager.Instance.ExecuteQueryAsync(query, parameters);
                // 直接将查询结果转为消息列表
                var messages = result.Select(row => new ChatMsg
                {
                    Id = row["id"]?.ToString(),
                    SenderId = row["sender_id"]?.ToString(),
                    ReceiverId = row["receiver_id"]?.ToString(),
                    Message = row["message"]?.ToString(),
                    ChannelType = Convert.ToInt32(row["channel_type"]),
                    Timestamp = Convert.ToInt32(row["timestamp"]),  // 直接使用查询时转换的 Unix 时间戳
                    IsRead = Convert.ToBoolean(row["is_read"]),
                    MessageType = Convert.ToInt32(row["message_type"]),
                    IsDeleted = Convert.ToBoolean(row["is_deleted"])
                }).ToList();
                return messages;
            }
            catch (Exception ex)
            {
                // 记录日志并抛出异常
                LoggerHelper.Instance.Error("Error fetching user messages: " + ex.Message);
                throw;  // 如果发生错误，可以让异常向上传递，让调用者处理
            }
        }


        //获取公共频道聊天消息
        private static async Task<List<ChatMsg>> GetPublicMessages(int channelType, int currentPage, int pageSize)
        {
            int offset = (currentPage - 1) * pageSize;
            string query = $@"
                SELECT id, sender_id, receiver_id, message, channel_type, timestamp, is_read, message_type, is_deleted
                FROM {SqlTable.ChatMessages}
                WHERE channel_type = @channelType AND is_deleted = FALSE
                ORDER BY timestamp DESC
                LIMIT @offset, @pageSize";

            var parameters = new Dictionary<string, object>
            {
                { "@channelType", channelType },
                { "@offset", offset },
                { "@pageSize", pageSize }
            };

            try
            {
                List<Dictionary<string, object>> result = await SqlManager.Instance.ExecuteQueryAsync(query, parameters);
                var messages = new List<ChatMsg>();

                foreach (var row in result)
                {
                    DateTime dateTime = (DateTime)row["timestamp"];
                    DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime.ToLocalTime());

                    ChatMsg msg = new ChatMsg();
                    msg.Id = row["id"]?.ToString();  // 获取 id
                    msg.SenderId = row["sender_id"]?.ToString();
                    msg.ReceiverId = row["receiver_id"]?.ToString();
                    msg.Message = row["message"]?.ToString();
                    msg.ChannelType = Convert.ToInt32(row["channel_type"]);
                    msg.Timestamp = Convert.ToInt32(dateTimeOffset.ToUnixTimeSeconds());
                    msg.IsRead = Convert.ToBoolean(row["is_read"]);
                    msg.MessageType = Convert.ToInt32(row["message_type"]);
                    msg.IsDeleted = Convert.ToBoolean(row["is_deleted"]);
                    messages.Add(msg);
                }
                return messages;
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Error("Error fetching messages by channel type: " + ex.Message);
                return new List<ChatMsg>();
            }
        }

        // 按频道类型获取消息
        public static async Task<List<ChatMsg>> GetChatMessages(bool requestPublic,string user_uuid,int channelType, int currentPage, int pageSize)
        {
            if (requestPublic)
            {
                return await GetPublicMessages(channelType, currentPage, pageSize);
            }
            else
            {
                return await GetUserMessages(user_uuid,channelType, currentPage, pageSize);
            }
        }

        // 更新消息状态（如标记已读）
        public static bool UpdateMessageStatus(string id, bool isRead)
        {
            string query = "UPDATE {SqlTable.ChatMessages} SET is_read = @isRead WHERE id = @id AND is_deleted = FALSE";

            var parameters = new Dictionary<string, object>
    {
        { "@id", id },
        { "@isRead", isRead }
    };

            try
            {
                int result = SqlManager.Instance.ExecuteNonQuery(query, parameters);
                return result > 0; // 返回操作是否成功
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Error("Error updating message status: " + ex.Message);
                return false;
            }
        }

        //转发消息
        public static async Task<OperationResult> HandleChatMsg(ChatMsg data)
        {
            OperationResult result = await SendMessageAsync(data.SenderId, data.ReceiverId, data.Message, data.ChannelType);
            return result;
        }

        // 清理公共频道聊天记录
        public static async Task<OperationResult> ClearPublicChannelMessagesAsync()
        {
            // 定义删除查询语句
            string query = $"DELETE FROM {SqlTable.ChatMessages} WHERE channel_type = 1";

            try
            {
                // 异步执行删除操作
                int rowsAffected = await SqlManager.Instance.ExecuteNonQueryAsync(query);

                // 打印删除的行数
                LoggerHelper.Instance.Info($"Deleted {rowsAffected} rows from public channel messages.");

                // 判断是否成功
                return rowsAffected > 0 ? OperationResult.Success : OperationResult.Failed;
            }
            catch (Exception ex)
            {
                // 捕获并打印异常
                LoggerHelper.Instance.Error($"Error clearing public channel messages: {ex.Message}");
                return OperationResult.Failed;
            }
        }

    }
}

using Protocols;
using System;
using System.Collections.Generic;
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
        INSERT INTO {SqlTable.ChatMessages} (uuid, sender_id, receiver_id, message, channel_type, is_read, message_type, is_deleted)
        VALUES (UUID(), @senderId, @receiverId, @message, @channelType, @isRead, @messageType, @isDeleted)";

            var parameters = new Dictionary<string, object>
    {
        { "@senderId", senderId },
        { "@receiverId", receiverId ?? (object)DBNull.Value },
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
                Console.WriteLine("Error sending message: " + ex.Message);
                return OperationResult.Failed;
            }
        }

        public static async Task<OperationResult> SendMessageToAllPlayersAsync(string senderId, string messageContent, int channelType)
        {
            string query = $"SELECT user_uuid FROM {SqlTable.GameSaveData}"; // 查询所有玩家
            try
            {
                // 异步获取所有玩家的 user_uuid
                var result = await SqlManager.Instance.ExecuteQueryAsync(query);
                bool allSucceeded = true; // 假设所有发送都成功
                foreach (var row in result)
                {
                    string receiverId = row["user_uuid"].ToString();
                    OperationResult sendResult = await SendMessageAsync(senderId, receiverId, messageContent, channelType);
                    // 如果发送失败，标记为失败
                    if (sendResult != OperationResult.Success)
                    {
                        Console.WriteLine($"Failed to send message to {receiverId}");
                        allSucceeded = false;
                    }
                }
                // 如果所有消息都成功发送，则返回 Success，否则返回 UpdateFailed
                if (allSucceeded)
                {
                    Console.WriteLine("All messages sent successfully.");
                    return OperationResult.Success;
                }
                else
                {
                    Console.WriteLine("Some messages failed to send.");
                    return OperationResult.Failed;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending messages to all players: " + ex.Message);
                return OperationResult.Failed;
            }
        }

        // 获取用户的聊天消息列表
        public static async Task<List<ChatMsg>> GetUserMessages(string userId, int currentPage, int pageSize, int channelType)
        {
            int offset = (currentPage - 1) * pageSize;

            string countQuery = "SELECT COUNT(*) FROM chat_messages WHERE (receiver_id = @receiverId OR receiver_id IS NULL) AND is_deleted = FALSE AND channel_type = @channelType";
            var countParameters = new Dictionary<string, object>
    {
        { "@receiverId", userId },
        { "@channelType", channelType }
    };
            int totalCount = Convert.ToInt32(await SqlManager.Instance.ExecuteScalarAsync(countQuery, countParameters));

            string query = $@"
        SELECT uuid, sender_id, receiver_id, message, channel_type, timestamp, is_read, message_type, is_deleted 
        FROM {SqlTable.ChatMessages} 
        WHERE (receiver_id = @receiverId OR receiver_id IS NULL) AND is_deleted = FALSE AND channel_type = @channelType
        ORDER BY timestamp DESC
        LIMIT @offset, @pageSize";

            var parameters = new Dictionary<string, object>
    {
        { "@receiverId", userId },
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
                    messages.Add(new ChatMsg
                    {
                        Uuid = row["uuid"]?.ToString(),  // 获取 uuid
                        SenderId = row["sender_id"]?.ToString(),
                        ReceiverId = row["receiver_id"]?.ToString(),
                        Message = row["message"]?.ToString(),
                        ChannelType = Convert.ToInt32(row["channel_type"]),
                        Timestamp = ((DateTime)row["timestamp"]).ToString(),
                        IsRead = Convert.ToBoolean(row["is_read"]),
                        MessageType = Convert.ToInt32(row["message_type"]),
                        IsDeleted = Convert.ToBoolean(row["is_deleted"])
                    });
                }

                return messages;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching user messages: " + ex.Message);
                return new List<ChatMsg>();
            }
        }

        // 获取公告频道消息
        public static async Task<List<ChatMsg>> GetAnnouncementMessages(int currentPage, int pageSize)
        {
            return await GetMessagesByChannelType(4, currentPage, pageSize); // 公告频道 (channelType = 4)
        }

        // 按频道类型获取消息
        public static async Task<List<ChatMsg>> GetMessagesByChannelType(int channelType, int currentPage, int pageSize)
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
                    messages.Add(new ChatMsg
                    {
                        SenderId = row["sender_id"]?.ToString(),
                        ReceiverId = row["receiver_id"]?.ToString(),
                        Message = row["message"]?.ToString(),
                        ChannelType = Convert.ToInt32(row["channel_type"]),
                        Timestamp = ((DateTime)row["timestamp"]).ToString(),
                        IsRead = Convert.ToBoolean(row["is_read"]),
                        MessageType = Convert.ToInt32(row["message_type"]),
                        IsDeleted = Convert.ToBoolean(row["is_deleted"])
                    });
                }

                return messages;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching messages by channel type: " + ex.Message);
                return new List<ChatMsg>();
            }
        }

        // 更新消息状态（如标记已读）
        public static bool UpdateMessageStatus(string uuid, bool isRead)
        {
            string query = "UPDATE {SqlTable.ChatMessages} SET is_read = @isRead WHERE uuid = @uuid AND is_deleted = FALSE";

            var parameters = new Dictionary<string, object>
    {
        { "@uuid", uuid },
        { "@isRead", isRead }
    };

            try
            {
                int result = SqlManager.Instance.ExecuteNonQuery(query, parameters);
                return result > 0; // 返回操作是否成功
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating message status: " + ex.Message);
                return false;
            }
        }

        //转发消息
        public static async Task<OperationResult> HandleChatMsg(ChatMsg data)
        {
            OperationResult result = await SendMessageAsync(data.SenderId, data.ReceiverId, data.Message, data.ChannelType);
            return result;
        }
    }
}

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
        public static bool SendMessage(string senderId, string receiverId, string messageContent, int channelType, int messageType = 1)
        {
            string query = $@"
                INSERT INTO {SqlTable.ChatMessages} (sender_id, receiver_id, message, channel_type, is_read, message_type, is_deleted)
                VALUES (@senderId, @receiverId, @message, @channelType, @isRead, @messageType, @isDeleted)";

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
                int result = SqlManager.Instance.ExecuteNonQuery(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending message: " + ex.Message);
                return false;
            }
        }

        // 发送消息到所有玩家
        public static async Task<bool> SendMessageToAllPlayers(string senderId, string messageContent, int channelType)
        {
            string query = $"SELECT user_uuid FROM {SqlTable.GameSaveData}"; // 查询所有玩家

            try
            {
                var result = await SqlManager.Instance.ExecuteQueryAsync(query);

                foreach (var row in result)
                {
                    string receiverId = row["user_uuid"].ToString();
                    bool success = SendMessage(senderId, receiverId, messageContent, channelType);
                    if (!success)
                    {
                        Console.WriteLine($"Failed to send message to {receiverId}");
                    }
                }

                Console.WriteLine("All messages sent successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending messages to all players: " + ex.Message);
                return false;
            }
        }

        // 获取用户的聊天消息列表
        public static async Task<List<ChatMessage>> GetUserMessages(string userId, int currentPage, int pageSize, int channelType)
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
                SELECT id, sender_id, receiver_id, message, channel_type, timestamp, is_read, message_type, is_deleted 
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
                var messages = new List<ChatMessage>();

                foreach (var row in result)
                {
                    messages.Add(new ChatMessage
                    {
                        Id = Convert.ToInt32(row["id"]),
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
                return new List<ChatMessage>();
            }
        }

        // 获取公告频道消息
        public static async Task<List<ChatMessage>> GetAnnouncementMessages(int currentPage, int pageSize)
        {
            return await GetMessagesByChannelType(4, currentPage, pageSize); // 公告频道 (channelType = 4)
        }

        // 按频道类型获取消息
        public static async Task<List<ChatMessage>> GetMessagesByChannelType(int channelType, int currentPage, int pageSize)
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
                var messages = new List<ChatMessage>();

                foreach (var row in result)
                {
                    messages.Add(new ChatMessage
                    {
                        Id = Convert.ToInt32(row["id"]),
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
                return new List<ChatMessage>();
            }
        }

        // 更新消息状态（如标记已读）
        public static bool UpdateMessageStatus(int messageId, bool isRead)
        {
            string query = "UPDATE {SqlTable.ChatMessages} SET is_read = @isRead WHERE id = @messageId AND is_deleted = FALSE";

            var parameters = new Dictionary<string, object>
            {
                { "@messageId", messageId },
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

    }
}

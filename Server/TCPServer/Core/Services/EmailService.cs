using Protocols;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TCPServer.Core.DataAccess;

namespace TCPServer.Core.Services
{
    public class EmailService
    {
        // 异步发送邮件
        public static async Task<OperationResult> SendEmailAsync(string senderId, string receiverId, string subject, string content)
        {
            // 插入邮件数据
            string query = $@"
            INSERT INTO {SqlTable.EmailList} (sender_id, receiver_id, subject, content, send_time, is_read, is_deleted, is_get)
            VALUES (@senderId, @receiverId, @subject, @content, @sendTime, @isRead, @isDeleted, @isGet)";

            var parameters = new Dictionary<string, object>
            {
                { "@senderId", senderId },
                { "@receiverId", receiverId },
                { "@subject", subject },
                { "@content", content },
                { "@sendTime", DateTimeOffset.Now.ToUnixTimeSeconds() },  // 使用Unix时间戳
                { "@isRead", false },  // 默认邮件为未读
                { "@isDeleted", false },  // 默认邮件为未删除
                { "@isGet", false }  // 默认邮件为未领取
            };

            try
            {
                // 异步执行数据库插入操作
                int result = await SqlManager.Instance.ExecuteNonQueryAsync(query, parameters);

                if (result > 0)
                {
                    return OperationResult.Success;  // 成功发送邮件
                }
                else
                {
                    return OperationResult.Failed;  // 邮件发送失败
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
                return OperationResult.Failed;  // 异常时返回失败
            }
        }

        // 获取用户的邮件列表
        public static async Task<List<EmailMsg>> GetUserEmails(string userId, int currentPage, int pageSize)
        {
            // 计算分页的起始位置
            int offset = (currentPage - 1) * pageSize;

            // 查询总数
            string countQuery = "SELECT COUNT(*) FROM email_list WHERE receiver_id = @receiverId AND is_deleted = FALSE";
            var countParameters = new Dictionary<string, object> { { "@receiverId", userId } };
            int totalCount = Convert.ToInt32(await SqlManager.Instance.ExecuteScalarAsync(countQuery, countParameters));

            // 查询邮件列表
            string query = $@"
                SELECT email_id, sender_id, subject, content, send_time, is_read, is_get 
                FROM {SqlTable.EmailList} 
                WHERE receiver_id = @receiverId AND is_deleted = FALSE 
                ORDER BY send_time DESC
                LIMIT @offset, @pageSize";

            var parameters = new Dictionary<string, object>
            {
                { "@receiverId", userId },
                { "@offset", offset },
                { "@pageSize", pageSize }
            };

            List<Dictionary<string, object>> result = await SqlManager.Instance.ExecuteQueryAsync(query, parameters);

            List<EmailMsg> emails = new List<EmailMsg>();
            foreach (var row in result)
            {
                emails.Add(new EmailMsg
                {
                    EmailId = Convert.ToInt32(row["email_id"]),  // 使用数据库中的字段名
                    SenderId = row["sender_id"]?.ToString(),    // 使用数据库中的字段名
                    Subject = row["subject"]?.ToString(),
                    Content = row["content"]?.ToString(),
                    SendTime = Convert.ToInt64(row["send_time"]),  // 使用Unix时间戳
                    IsRead = Convert.ToBoolean(row["is_read"]),
                    IsGet = Convert.ToBoolean(row["is_get"])  // 处理邮件是否领取
                });
            }

            return emails;
        }

        // 异步标记邮件为已读
        public static async Task<OperationResult> MarkEmailAsReadAsync(int emailId)
        {
            string query = $"UPDATE {SqlTable.EmailList} SET is_read = TRUE WHERE email_id = @emailId";

            var parameters = new Dictionary<string, object>
            {
                { "@emailId", emailId }
            };

            try
            {
                int result = await SqlManager.Instance.ExecuteNonQueryAsync(query, parameters);
                return result > 0 ? OperationResult.Success : OperationResult.Failed;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error marking email as read: " + ex.Message);
                return OperationResult.Failed;
            }
        }

        // 异步删除邮件
        public static async Task<OperationResult> DeleteEmailAsync(int emailId)
        {
            string query = $"UPDATE {SqlTable.EmailList} SET is_deleted = TRUE WHERE email_id = @emailId";

            var parameters = new Dictionary<string, object>
            {
                { "@emailId", emailId }
            };

            try
            {
                int result = await SqlManager.Instance.ExecuteNonQueryAsync(query, parameters);
                return result > 0 ? OperationResult.Success : OperationResult.Failed;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting email: " + ex.Message);
                return OperationResult.Failed;
            }
        }

        // 异步硬删除：完全删除某个用户的所有邮件
        public static async Task<OperationResult> DeleteAllEmailsAsync(string userId)
        {
            try
            {
                // 删除该用户的所有邮件
                string query = $"DELETE FROM {SqlTable.EmailList} WHERE receiver_id = @receiverId";
                var parameters = new Dictionary<string, object>
                {
                    { "@receiverId", userId }
                };

                int result = await SqlManager.Instance.ExecuteNonQueryAsync(query, parameters);
                return result > 0 ? OperationResult.Success : OperationResult.Failed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting emails for user {userId}: {ex.Message}");
                return OperationResult.Failed;
            }
        }

        // 异步给全服玩家发送邮件
        public static async Task<OperationResult> SendEmailToAllPlayersAsync(string senderId, string subject, string content)
        {
            // 查询所有玩家的user_uuid
            string query = $"SELECT user_uuid FROM {SqlTable.GameSaveData}"; // 只给在线玩家发送邮件
            try
            {
                List<Dictionary<string, object>> result = await SqlManager.Instance.ExecuteQueryAsync(query, new Dictionary<string, object>());

                // 遍历所有玩家并发送邮件
                foreach (var row in result)
                {
                    string receiverId = row["user_uuid"].ToString();
                    var emailResult = await SendEmailAsync(senderId, receiverId, subject, content);
                    if (emailResult != OperationResult.Success)
                    {
                        Console.WriteLine($"Failed to send email to {receiverId}");
                        continue;  // 继续发送给其他玩家
                    }
                }

                Console.WriteLine("All emails sent successfully.");
                return OperationResult.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending emails to all players: " + ex.Message);
                return OperationResult.Failed;
            }
        }
    }
}

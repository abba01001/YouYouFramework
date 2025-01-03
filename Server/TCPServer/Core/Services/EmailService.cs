using Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServer.Core.DataAccess;

namespace TCPServer.Core.Services
{
    public class EmailService
    {
        // 发送邮件
        public static bool SendEmail(string senderId, string receiverId, string subject, string content)
        {
            // 插入邮件数据
            string query = $@"
            INSERT INTO {SqlTable.EmailList} (senderId, receiverId, subject, content, sendTime, isRead, isDeleted, isGet)
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
                int result = SqlManager.Instance.ExecuteNonQuery(query, parameters);
                return result > 0; // 返回操作成功与否
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
                return false;
            }
        }

        // 获取用户的邮件列表
        public static async Task<List<EmailMsg>> GetUserEmails(string userId, int currentPage, int pageSize)
        {
            // 计算分页的起始位置
            int offset = (currentPage - 1) * pageSize;

            // 查询总数
            string countQuery = "SELECT COUNT(*) FROM email WHERE receiverId = @receiverId AND isDeleted = FALSE";
            var countParameters = new Dictionary<string, object> { { "@receiverId", userId } };
            int totalCount = Convert.ToInt32(SqlManager.Instance.ExecuteScalar(countQuery, countParameters));

            // 查询邮件列表
            string query = $@"
                SELECT emailId, senderId, subject, content, sendTime, isRead, isGet 
                FROM {SqlTable.EmailList} 
                WHERE receiverId = @receiverId AND isDeleted = FALSE 
                ORDER BY sendTime DESC
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
                    EmailId = Convert.ToInt32(row["emailId"]),
                    SenderId = row["senderId"]?.ToString(),
                    Subject = row["subject"]?.ToString(),
                    Content = row["content"]?.ToString(),
                    SendTime = Convert.ToInt64(row["sendTime"]),  // 使用Unix时间戳
                    IsRead = Convert.ToBoolean(row["isRead"]),
                    IsGet = Convert.ToBoolean(row["isGet"])  // 处理邮件是否领取
                });
            }

            return emails;
        }

        // 标记邮件为已读
        public static bool MarkEmailAsRead(int emailId)
        {
            string query = $"UPDATE {SqlTable.EmailList} SET isRead = TRUE WHERE emailId = @emailId";

            var parameters = new Dictionary<string, object>
            {
                { "@emailId", emailId }
            };

            try
            {
                int result = SqlManager.Instance.ExecuteNonQuery(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error marking email as read: " + ex.Message);
                return false;
            }
        }

        // 删除邮件
        public static bool DeleteEmail(int emailId)
        {
            string query = $"UPDATE {SqlTable.EmailList} SET isDeleted = TRUE WHERE emailId = @emailId";

            var parameters = new Dictionary<string, object>
            {
                { "@emailId", emailId }
            };

            try
            {
                int result = SqlManager.Instance.ExecuteNonQuery(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting email: " + ex.Message);
                return false;
            }
        }

        // 硬删除：完全删除某个用户的所有邮件
        public static bool DeleteAllEmails(string userId)
        {
            try
            {
                // 删除该用户的所有邮件
                string query = $"DELETE FROM {SqlTable.EmailList} WHERE receiverId = @receiverId";
                var parameters = new Dictionary<string, object>
                {
                    { "@receiverId", userId }
                };

                int result = SqlManager.Instance.ExecuteNonQuery(query, parameters);
                if (result > 0)
                {
                    Console.WriteLine($"Successfully deleted all emails for user {userId}.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"No emails found for user {userId}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting emails for user {userId}: {ex.Message}");
                return false;
            }
        }

        // 给全服玩家发送邮件
        public static async Task<bool> SendEmailToAllPlayers(string senderId, string subject, string content)
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
                    bool success = SendEmail(senderId, receiverId, subject, content);
                    if (!success)
                    {
                        Console.WriteLine($"Failed to send email to {receiverId}");
                        // 这里可以根据需要决定是否继续发送给其他玩家，或者全部失败时返回
                        continue;
                    }
                }

                Console.WriteLine("All emails sent successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending emails to all players: " + ex.Message);
                return false;
            }
        }
    }
}

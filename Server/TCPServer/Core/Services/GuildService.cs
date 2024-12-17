using Protocols.Guild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServer.Core.DataAccess;

namespace TCPServer.Core.Services
{
    public class GuildService
    {
        //添加公会
        public static bool AddGuild(string guildId, string name, string leaderName, string description)
        {
            string query = @"INSERT INTO guild_list (guildId, name, leaderName, description)
                             VALUES (@guildId, @name, @leaderName, @description)";

            var parameters = new Dictionary<string, object>
            {
                { "@guildId", guildId },
                { "@name", name },
                { "@leaderName", leaderName },
                { "@description", description },
            };

            try
            {
                int result = SqlManager.Instance.ExecuteNonQuery(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding guild: " + ex.Message);
                return false;
            }
        }

        //获取公会列表
        public static async Task<GuildList> GetGuildList(int currentPage, int pageSize)
        {
            // 计算分页的起始位置
            int offset = (currentPage - 1) * pageSize;

            // 查询总数
            string countQuery = "SELECT COUNT(*) FROM guild_list";  // 假设你的表名是 guild_list
            int totalCount = Convert.ToInt32(SqlManager.Instance.ExecuteScalar(countQuery));

            // 查询公会信息
            string query = $"SELECT guildId, name, leaderName, level, memberCount, memberLimit, description, activityScore, isRecruiting, emblem FROM guild_list LIMIT {offset}, {pageSize}";

            // 执行查询

            List<Dictionary<string, object>> result = await SqlManager.Instance.ExecuteQueryAsync(query);

            // 转换查询结果为 GuildList
            List<GuildInfo> guildInfos = new List<GuildInfo>();
            foreach (var row in result)
            {
                GuildInfo guildInfo = new GuildInfo
                {
                    GuildId = row["guildId"]?.ToString(),
                    Name = row["name"]?.ToString(),
                    LeaderName = row["leaderName"]?.ToString(),
                    Level = Convert.ToInt32(row["level"]),
                    MemberCount = Convert.ToInt32(row["memberCount"]),
                    MemberLimit = Convert.ToInt32(row["memberLimit"]),
                    Description = row["description"]?.ToString(),
                    ActivityScore = Convert.ToInt32(row["activityScore"]),
                    IsRecruiting = Convert.ToBoolean(row["isRecruiting"]),
                    Emblem = row["emblem"]?.ToString()
                };
                guildInfos.Add(guildInfo);
            }

            Console.WriteLine($"{guildInfos.Count}=={totalCount}=={currentPage}=={pageSize}");

            // 创建并返回 GuildList
            return new GuildList
            {
                Guilds = { guildInfos },
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize
            };
        }

        // 删除公会
        public static bool DeleteGuild(string guildId)
        {
            string query = "DELETE FROM guild_list WHERE guildId = @guildId";
            var parameters = new Dictionary<string, object>
            {
                { "@guildId", guildId }
            };

            try
            {
                int result = SqlManager.Instance.ExecuteNonQuery(query, parameters);
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting guild: " + ex.Message);
                return false;
            }
        }

        // 成员退出公会
        public static async Task<bool> ExitGuild(string memberId, string guildId)
        {
            try
            {
                // 1. 删除成员记录
                string deleteQuery = @"DELETE FROM guild_members WHERE memberId = @memberId AND guildId = @guildId";
                var deleteParams = new Dictionary<string, object>
                {
                    { "@memberId", memberId },
                    { "@guildId", guildId }
                };

                int deleteResult = SqlManager.Instance.ExecuteNonQuery(deleteQuery, deleteParams);
                if (deleteResult <= 0)
                {
                    Console.WriteLine($"Error: Member {memberId} is not part of the guild {guildId}.");
                    return false; // 如果没有记录被删除，说明成员不在该公会
                }

                // 2. 更新公会成员数量
                string updateQuery = @"
                    UPDATE guild_list 
                    SET memberCount = memberCount - 1 
                    WHERE guildId = @guildId";

                var updateParams = new Dictionary<string, object>
                {
                    { "@guildId", guildId }
                };

                int updateResult = SqlManager.Instance.ExecuteNonQuery(updateQuery, updateParams);
                if (updateResult > 0)
                {
                    Console.WriteLine($"Member {memberId} successfully exited the guild {guildId}. Updated member count.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Error updating member count for guild {guildId}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        // 成员加入公会
        public static async Task<bool> JoinGuild(string memberId, string memberName, string guildId, string role, int level)
        {
            try
            {
                // 1. 向 guild_members 表中插入成员信息
                string insertQuery = @"
                    INSERT INTO guild_members (memberId, guildId, memberName, role, level, joinDate, isOnline)
                    VALUES (@memberId, @guildId, @memberName, @role, @level, @joinDate, @isOnline)";

                var insertParams = new Dictionary<string, object>
                {
                    { "@memberId", memberId },
                    { "@guildId", guildId },
                    { "@memberName", memberName },
                    { "@role", role },  // 例如，角色可以是 "成员", "副会长", "会长" 等
                    { "@level", level },
                    { "@joinDate", DateTime.Now },
                    { "@isOnline", 0 } // 默认加入公会时成员不在线
                };

                int insertResult = SqlManager.Instance.ExecuteNonQuery(insertQuery, insertParams);
                if (insertResult <= 0)
                {
                    Console.WriteLine($"Error: Failed to add member {memberId} to the guild {guildId}.");
                    return false; // 如果插入失败，返回 false
                }

                // 2. 更新公会成员数量
                string updateQuery = @"
                    UPDATE guild_list 
                    SET memberCount = memberCount + 1 
                    WHERE guildId = @guildId";

                var updateParams = new Dictionary<string, object>
                {
                    { "@guildId", guildId }
                };

                int updateResult = SqlManager.Instance.ExecuteNonQuery(updateQuery, updateParams);
                if (updateResult > 0)
                {
                    Console.WriteLine($"Member {memberId} successfully joined the guild {guildId}. Updated member count.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Error updating member count for guild {guildId}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
}


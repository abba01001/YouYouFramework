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
    }
}

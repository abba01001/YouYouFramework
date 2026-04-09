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
        // 创建公会
        public static async Task<bool> CreateGuild(string creatorId, string creatorName, string guildName,
            string description)
        {
            // 0️⃣ 检查玩家是否存在
            string checkPlayerQuery = "SELECT COUNT(*) FROM game_saves WHERE user_uuid = @creator_id";
            var checkPlayerParams = new Dictionary<string, object> { { "@creator_id", creatorId } };
            int playerExists =
                Convert.ToInt32(await SqlManager.Instance.ExecuteScalarAsync(checkPlayerQuery, checkPlayerParams));
            if (playerExists == 0)
            {
                LoggerHelper.Instance.Info($"Error: Creator {creatorId} does not exist.");
                return false;
            }

            // 1️⃣ 检查玩家是否已经创建过公会
            string checkGuildQuery = "SELECT COUNT(*) FROM guild_list WHERE creator_id = @creator_id";
            int existingGuildCount =
                Convert.ToInt32(await SqlManager.Instance.ExecuteScalarAsync(checkGuildQuery, checkPlayerParams));
            if (existingGuildCount > 0)
            {
                LoggerHelper.Instance.Info($"Error: Player {creatorId} has already created a guild.");
                return false;
            }

            // 2️⃣ 检查玩家是否已经加入其他公会
            string checkMemberQuery = "SELECT COUNT(*) FROM guild_members WHERE member_id = @creator_id";
            int memberCount =
                Convert.ToInt32(await SqlManager.Instance.ExecuteScalarAsync(checkMemberQuery, checkPlayerParams));
            if (memberCount > 0)
            {
                LoggerHelper.Instance.Info($"Error: Player {creatorId} is already in a guild.");
                return false;
            }

            // 3️⃣ 开始事务（局部化，高并发安全）
            try
            {
                return await SqlManager.Instance.ExecuteTransactionAsync(async (conn, tran) =>
                {
                    // 4️⃣ 插入公会
                    string insertGuildQuery = @"
                INSERT INTO guild_list (creator_id, guild_name, leader_name, description, member_count)
                VALUES (@creator_id, @guild_name, @leader_name, @description, @member_count)";
                    var guildParams = new Dictionary<string, object>
                    {
                        { "@creator_id", creatorId },
                        { "@guild_name", guildName },
                        { "@leader_name", creatorName },
                        { "@description", description },
                        { "@member_count", 1 } // 创建者本人
                    };
                    int guildResult =
                        await SqlManager.Instance.ExecuteNonQueryAsync(insertGuildQuery, guildParams, conn, tran);
                    if (guildResult <= 0)
                    {
                        LoggerHelper.Instance.Info("Error: Failed to create guild.");
                        return false;
                    }

                    // 5️⃣ 获取刚插入的 guild_id
                    string guildId =
                        (await SqlManager.Instance.ExecuteScalarAsync("SELECT LAST_INSERT_ID()", null, conn, tran))
                        .ToString();

                    // 6️⃣ 将创建者加入 guild_members
                    string insertMemberQuery = @"
                INSERT INTO guild_members (member_id, guild_id, member_name, position, level, join_date)
                VALUES (@member_id, @guild_id, @member_name, @position, @level, @join_date)";
                    var memberParams = new Dictionary<string, object>
                    {
                        { "@member_id", creatorId },
                        { "@guild_id", guildId },
                        { "@member_name", creatorName },
                        { "@position", 1 }, // 会长
                        { "@level", 1 }, // 默认等级
                        { "@join_date", DateTime.Now }
                    };
                    int memberResult =
                        await SqlManager.Instance.ExecuteNonQueryAsync(insertMemberQuery, memberParams, conn, tran);
                    if (memberResult <= 0)
                    {
                        LoggerHelper.Instance.Info("Error: Failed to add creator to guild_members.");
                        return false;
                    }

                    // 7️⃣ 更新创建者的角色表 guild_id（非事务操作也可以，但你可以选择事务内也更新）
                    await AccountService.UpdateGuildId(creatorId, guildId);

                    // 8️⃣ 事务提交（由 ExecuteTransactionAsync 自动处理）
                    return true;
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Info("Error creating guild: " + ex.Message);
                return false;
            }
        }


        //获取公会列表
        public static async Task<GuildList> GetGuildList(int currentPage, int pageSize)
        {
            // 计算分页的起始位置
            int offset = (currentPage - 1) * pageSize;

            // 查询总数
            string countQuery = "SELECT COUNT(*) FROM guild_list"; // 假设你的表名是 guild_list
            int totalCount = Convert.ToInt32(SqlManager.Instance.ExecuteScalar(countQuery));

            // 查询公会信息
            string query = $"SELECT * FROM guild_list LIMIT {offset}, {pageSize}";

            // 执行查询

            List<Dictionary<string, object>> result = await SqlManager.Instance.ExecuteQueryAsync(query);

            // 转换查询结果为 GuildList
            List<GuildInfo> guildInfos = new List<GuildInfo>();
            foreach (var row in result)
            {
                var rowStr = string.Join(", ", row.Select(kv => $"{kv.Key}={kv.Value}"));
                LoggerHelper.Instance.Info($"Row: {rowStr}");

                GuildInfo guildInfo = new GuildInfo
                {
                    GuildId = row["guild_id"]?.ToString(),
                    Name = row["name"]?.ToString(),
                    LeaderName = row["leader_name"]?.ToString(),
                    Level = Convert.ToInt32(row["level"]),
                    MemberCount = Convert.ToInt32(row["member_count"]),
                    Description = row["description"]?.ToString(),
                    ActivityScore = Convert.ToInt32(row["activity_score"]),
                    IconId = Convert.ToInt32(row["iconId"])
                };
                guildInfos.Add(guildInfo);
            }

            LoggerHelper.Instance.Info($"{guildInfos.Count}=={totalCount}=={currentPage}=={pageSize}");

            // 创建并返回 GuildList
            return new GuildList
            {
                Guilds = { guildInfos },
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize
            };
        }

        public static async Task<bool> DeleteGuild(string memberId, string guildId)
        {
            try
            {
                // 0️⃣ 检查权限：只有会长可以解散公会
                string checkQuery = @"
            SELECT position 
            FROM guild_members 
            WHERE member_id = @member_id AND guild_id = @guild_id";
                var checkParams = new Dictionary<string, object>
                {
                    { "@member_id", memberId },
                    { "@guild_id", guildId }
                };
                var positionObj = SqlManager.Instance.ExecuteScalar(checkQuery, checkParams);
                if (positionObj == null || Convert.ToInt32(positionObj) != 1) // 假设 1 是会长
                {
                    LoggerHelper.Instance.Info($"Member {memberId} has no permission to delete guild {guildId}.");
                    return false;
                }

                // 1️⃣ 获取所有成员
                string selectMembersQuery = "SELECT member_id FROM guild_members WHERE guild_id = @guild_id";
                var memberRows = SqlManager.Instance.ExecuteQuery(selectMembersQuery,
                    new Dictionary<string, object> { { "@guild_id", guildId } });

                // 2️⃣ 更新每个成员的公会状态
                foreach (var row in memberRows)
                {
                    string mId = row["member_id"].ToString();
                    await AccountService.UpdateGuildId(mId, null); // 清空成员的公会ID
                }

                // 3️⃣ 删除 guild_members 表里的记录
                string deleteMembersQuery = "DELETE FROM guild_members WHERE guild_id = @guild_id";
                SqlManager.Instance.ExecuteNonQuery(deleteMembersQuery,
                    new Dictionary<string, object> { { "@guild_id", guildId } });

                // 4️⃣ 删除 guild_list 表里的记录
                string deleteGuildQuery = "DELETE FROM guild_list WHERE guild_id = @guild_id";
                int result = SqlManager.Instance.ExecuteNonQuery(deleteGuildQuery,
                    new Dictionary<string, object> { { "@guild_id", guildId } });

                return result > 0;
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Info("Error deleting guild: " + ex.Message);
                return false;
            }
        }


        // 成员退出公会
        public static async Task<bool> ExitGuild(string memberId, string guildId)
        {
            try
            {
                // 1. 删除成员记录
                string deleteQuery = @"DELETE FROM guild_members WHERE member_id = @member_id AND guild_id = @guild_id";
                var deleteParams = new Dictionary<string, object>
                {
                    { "@member_id", memberId },
                    { "@guild_id", guildId }
                };

                int deleteResult = SqlManager.Instance.ExecuteNonQuery(deleteQuery, deleteParams);
                if (deleteResult <= 0)
                {
                    LoggerHelper.Instance.Info($"Error: Member {memberId} is not part of the guild {guildId}.");
                    //return false; // 如果没有记录被删除，说明成员不在该公会
                }

                // 2 更新玩家的公会 ID
                await AccountService.UpdateGuildId(memberId, null);

                // 3. 更新公会成员数量
                string updateQuery = @"
                    UPDATE guild_list 
                    SET member_count = member_count - 1 
                    WHERE guild_id = @guild_id";

                var updateParams = new Dictionary<string, object>
                {
                    { "@guild_id", guildId }
                };

                int updateResult = SqlManager.Instance.ExecuteNonQuery(updateQuery, updateParams);
                if (updateResult > 0)
                {
                    LoggerHelper.Instance.Info(
                        $"Member {memberId} successfully exited the guild {guildId}. Updated member count.");
                    return true;
                }
                else
                {
                    LoggerHelper.Instance.Info($"Error updating member count for guild {guildId}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Info($"Error: {ex.Message}");
                return false;
            }
        }

        // 成员加入公会（安全版、优化）
        public static async Task<bool> JoinGuild(string memberId, string memberName, string guildId, int position,
            int level)
        {
            try
            {
                // 统一参数字典，避免重复创建
                var parameters = new Dictionary<string, object>
                {
                    { "@member_id", memberId },
                    { "@guild_id", guildId }
                };

                // 1️⃣ 检查公会是否存在
                int guildCount = Convert.ToInt32(SqlManager.Instance.ExecuteScalar(
                    "SELECT COUNT(*) FROM guild_list WHERE guild_id = @guild_id", parameters));

                if (guildCount == 0)
                {
                    LoggerHelper.Instance.Info($"Error: Guild {guildId} does not exist.");
                    return false;
                }

                // 2️⃣ 检查成员是否已加入
                int memberCount = Convert.ToInt32(SqlManager.Instance.ExecuteScalar(
                    "SELECT COUNT(*) FROM guild_members WHERE member_id = @member_id AND guild_id = @guild_id",
                    parameters));

                if (memberCount > 0)
                {
                    LoggerHelper.Instance.Info($"Error: Member {memberId} already in guild {guildId}.");
                    return false;
                }

                // 3️⃣ 插入成员信息
                string insertQuery = @"
            INSERT INTO guild_members (member_id, guild_id, member_name, position, level, join_date)
            VALUES (@member_id, @guild_id, @member_name, @position, @level, @join_date)";

                var insertParams = new Dictionary<string, object>(parameters)
                {
                    { "@member_name", memberName },
                    { "@position", position },
                    { "@level", level },
                    { "@join_date", DateTime.Now }
                };

                if (SqlManager.Instance.ExecuteNonQuery(insertQuery, insertParams) <= 0)
                {
                    LoggerHelper.Instance.Info($"Error: Failed to add member {memberId} to guild {guildId}.");
                    return false;
                }

                // 4️⃣ 更新玩家的公会 ID
                await AccountService.UpdateGuildId(memberId, guildId);

                // 5️⃣ 更新公会成员数量
                string updateQuery = "UPDATE guild_list SET member_count = member_count + 1 WHERE guild_id = @guild_id";
                if (SqlManager.Instance.ExecuteNonQuery(updateQuery, parameters) > 0)
                {
                    LoggerHelper.Instance.Info(
                        $"Member {memberId} successfully joined guild {guildId}. Updated member count.");
                    return true;
                }
                else
                {
                    LoggerHelper.Instance.Info($"Error updating member count for guild {guildId}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Info($"Error: {ex.Message}");
                return false;
            }
        }


        // 通过 guild_id 查询单个公会
        public static async Task<GuildInfo> GetGuildById(string guildId)
        {
            string query = "SELECT * FROM guild_list WHERE guild_id = @guildId";

            var parameters = new Dictionary<string, object>
            {
                { "@guildId", guildId }
            };

            try
            {
                List<Dictionary<string, object>>
                    result = await SqlManager.Instance.ExecuteQueryAsync(query, parameters);

                if (result.Count == 0)
                {
                    LoggerHelper.Instance.Info($"Guild with id {guildId} not found.");
                    return null;
                }

                var row = result[0];
                // 打印 row 的全部字段
                var rowStr = string.Join(", ", row.Select(kv => $"{kv.Key}={kv.Value}"));
                LoggerHelper.Instance.Info($"Row: {rowStr}");

                GuildInfo guildInfo = new GuildInfo
                {
                    GuildId = row["guild_id"]?.ToString(),
                    Name = row["name"]?.ToString(),
                    LeaderName = row["leader_name"]?.ToString(),
                    Level = Convert.ToInt32(row["level"]),
                    MemberCount = Convert.ToInt32(row["member_count"]),
                    Description = row["description"]?.ToString(),
                    ActivityScore = Convert.ToInt32(row["activity_score"]),
                    // 如果表里有 iconId 再打开下面这一行
                    // IconId = Convert.ToInt32(row["iconId"])
                };

                return guildInfo;
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.Info("Error fetching guild: " + ex.Message);
                return null;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.Core
{
    public class Constants
    {
        public static string SECURITYKEY = "3ZkPqF9hDjW8q2Z7";//钥匙
        public static int BLOCK_SIZE = 16; // AES块大小
        //public struct SqlKey
        //{
        //    public string SqlServer = "Server";
        //    public string SqlDatabase = "Database";
        //    public string SqlUserId = "User ID";
        //    public string SqlPassword = "Password";
        //    public string SqlPort = "Port";
        //}



    }

    public static class SqlKey
    {
        public static string Server { get; set; } = "Server";
        public static string Database { get; set; } = "Database";
        public static string UserId { get; set; } = "User ID";
        public static string Password { get; set; } = "Password";
        public static string Port { get; set; } = "Port";
    }

    public static class SqlTable
    {
        public static string GameSaveData { get; set; } = "game_saves";
        public static string GuildList { get; set; } = "guild_list";
        public static string GuildMembers { get; set; } = "guild_members";
    }

    public static class Token
    {
        public static string TokenKey { get; set; } = "TokenKey";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.Core
{
    public enum OperationResult
    {
        Success = 1,
        UserNotFound = 0,
        PasswordIncorrect = 2,
        Failed = 3,
        PropertyNotFound = 4,
        NotFound = 5,
        NotBlocked = 6,
        AlreadyBlocked = 7,
        UserAlreadyOnline = 8,
    }


    public class Constants
    {
        public static string SECURITYKEY = "3ZkPqF9hDjW8q2Z7";//钥匙
        public static int BLOCK_SIZE = 16; // AES块大小
        public const int ProtocalHeadLength = 41;
        public const int ProtocalTotalLength = 1024;


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
        public static string EmailList { get; set; } = "email_list";
        public static string ChatMessages { get; set; } = "chat_messages";
    }

    public static class Token
    {
        public static string TokenKey { get; set; } = "TokenKey";
    }
}

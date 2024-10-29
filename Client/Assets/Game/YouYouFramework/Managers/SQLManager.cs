using System;
using System.Data;
using MySql.Data.MySqlClient;
using UnityEngine;

namespace YouYou
{
    public class SQLManager
{
    public SQLManager()
    {

    }
    private MySqlConnection connection;
    
    // 注册新账户
    public void RegisterAccount(string accountId, string passWord)
    {
        if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(passWord))
        {
            Debug.LogError("请输入账号和密码");
            return;
        }

        if (Insert(accountId, passWord))
        {
            Debug.Log("注册成功");
            GameEntry.Player.InitGameData();
        }
        else
        {
            Debug.LogError("注册失败");
        }
    }

    //这里改成服务器那边返回 TODO
    public void Login(string account,string password)
    {
        if (Find(account))
        {
            if (ValidateUser(account, password,out string uuid))
            {
                Constants.UserUUID = uuid;
                GameEntry.Player.InitGameData();
            }
        }
        else
        {
            RegisterAccount(account,password);
        }
    }

    // 初始化数据库连接
    public void Init()
    {
        string connectionString = $"Server={SecurityUtil.GetSqlKeyDic()["Server"]};Database={SecurityUtil.GetSqlKeyDic()["Database"]};" +
                                  $"User ID={SecurityUtil.GetSqlKeyDic()["User ID"]};Password={SecurityUtil.GetSqlKeyDic()["Password"]};" +
                                  $"Port={SecurityUtil.GetSqlKeyDic()["Port"]};charset=utf8mb4";
        int maxRetries = 3; // 最大重试次数
        int retryCount = 0; // 当前重试次数
        bool isConnected = false; // 连接状态
        while (retryCount < maxRetries && !isConnected)
        {
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open(); // 尝试打开连接
                isConnected = true; // 连接成功
            }
            catch (MySqlException ex) // 捕获 MySqlException 异常
            {
                retryCount++;
                Debug.LogError($"数据库连接失败，尝试次数: {retryCount}/{maxRetries}，错误信息: {ex.Message}");
                System.Threading.Thread.Sleep(1000);
            }
            finally
            {
                // 如果连接成功，关闭连接
                if (isConnected)
                {
                    connection.Close();
                    //CreateDataTable();
                    Debug.Log("数据库连接正常");
                }
            }
        }
        if (!isConnected)
        {
            Debug.LogError("所有数据库连接尝试均失败，请检查连接设置");
        }
    }


    /// <summary>
    /// 打开数据库连接
    /// </summary>
    public bool OpenConnection()
    {
        try
        {
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            return true;
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"SQL数据库连接错误： {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 关闭数据库连接
    /// </summary>
    public bool CloseConnection()
    {
        try
        {
            if (connection.State == ConnectionState.Open)
                connection.Close();
            return true;
        }
        catch (MySqlException ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
    }

    /// <summary>
    /// 检查是否存在数据表
    /// </summary>
    public bool HasDataTable(string tableName)
    {
        string query = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{connection.Database}' AND table_name = '{tableName}'";
        using (var cmd = new MySqlCommand(query, connection))
        {
            int result = Convert.ToInt32(cmd.ExecuteScalar());
            return result > 0;
        }
    }

    /// <summary>
    /// 创建数据表，包含唯一标识user_uuid和账号唯一约束
    /// </summary>
    public void CreateDataTable()
    {
        string name = "register_data";
        string query = $"CREATE TABLE IF NOT EXISTS {name} (" +
                       "id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                       "user_uuid CHAR(36) NOT NULL UNIQUE, " +
                       "user_account VARCHAR(50) NOT NULL UNIQUE, " +
                       "user_password VARCHAR(88) NOT NULL) " +
                       "CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci";

        if (OpenConnection())
        {
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.ExecuteNonQuery(); // 执行创建表的命令
            }
            CloseConnection(); // 确保关闭连接
        }
    }


    /// <summary>
    /// 插入新用户数据，使用UUID作为唯一标识
    /// </summary>
    private bool Insert(string user_account, string user_password)
    {
        // 生成唯一UUID
        string user_uuid = Guid.NewGuid().ToString();
        user_password = SecurityUtil.ConvertBase64Key(user_account);
        string query = "INSERT INTO register_data (user_uuid, user_account, user_password) VALUES(@uuid, @account, @password)";

        if (OpenConnection())
        {
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@uuid", user_uuid);
                cmd.Parameters.AddWithValue("@account", user_account);
                cmd.Parameters.AddWithValue("@password", user_password);
                Constants.UserUUID = user_uuid;
                int result = cmd.ExecuteNonQuery();
                CloseConnection();
                return result > 0;
            }
        }
        return false;
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    private bool Delete(string user_account)
    {
        string query = "DELETE FROM register_data WHERE user_account = @value";

        if (OpenConnection())
        {
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@value", user_account);
                int result = cmd.ExecuteNonQuery();
                CloseConnection();
                return result > 0;
            }
        }
        return false;
    }

    /// <summary>
    /// 查找用户是否存在
    /// </summary>
    private bool Find(string user_account)
    {
        string query = "SELECT COUNT(*) FROM register_data WHERE user_account = @value";

        if (OpenConnection())
        {
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@value", user_account);
                int result = Convert.ToInt32(cmd.ExecuteScalar());
                CloseConnection();
                return result > 0;
            }
        }
        return false;
    }

    /// <summary>
    /// 修改用户密码
    /// </summary>
    private bool Change(string user_account, string user_password)
    {
        string query = "UPDATE register_data SET user_password = @newpassword WHERE user_account = @account";

        if (OpenConnection())
        {
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@newpassword", user_password);
                cmd.Parameters.AddWithValue("@account", user_account);
                int result = cmd.ExecuteNonQuery();
                CloseConnection();
                return result > 0;
            }
        }
        return false;
    }
    
    private bool ValidateUser(string user_account, string input_password, out string user_uuid)
    {
        string query = "SELECT user_uuid, user_password FROM register_data WHERE user_account = @account";
        user_uuid = null; // 初始化输出参数
        user_account = "a123";
        input_password = "a123";
        if (OpenConnection())
        {
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@account", user_account);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) // 如果找到了用户
                    {
                        user_uuid = reader["user_uuid"].ToString(); // 获取 UUID
                        string stored_password = reader["user_password"].ToString();
                        return stored_password == SecurityUtil.GetBase64Key(input_password);
                    }
                }
                CloseConnection();
            }
        }
        return false; // 用户不存在或密码不匹配
    }
}

}


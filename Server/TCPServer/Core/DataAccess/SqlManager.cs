using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TCPServer.Core.DataAccess
{
    public class SqlManager
    {
        private static SqlManager _instance;
        private static readonly object _lock = new object();  // 用于线程同步
        private readonly string _connectionString;

        // 私有构造函数，防止外部直接创建实例
        private SqlManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        // 静态属性，获取 SqlManager 实例
        public static SqlManager Instance
        {
            get
            {
                // 只有在实例未创建时才创建新的实例
                if (_instance == null)
                {
                    lock (_lock) // 确保线程安全
                    {
                        if (_instance == null)
                        {
                            // 使用连接字符串创建实例
                            throw new InvalidOperationException("SqlManager instance is not initialized. Please initialize it first.");
                        }
                    }
                }
                return _instance;
            }
        }

        // 设置 SqlManager 实例（用于初始化）
        public static void Initialize(string connectionString)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SqlManager(connectionString);
                        Console.WriteLine("Sql管理器初始化成功...");
                    }
                }
            }
        }

        // 获取数据库连接，启用连接池（默认启用）
        private MySqlConnection GetConnection()
        {
            try
            {
                var connection = new MySqlConnection(_connectionString);
                Console.WriteLine("Connecting with: " + connection.ConnectionString);
                connection.Open();
                return connection;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("MySQL Error: " + ex.Message);
                Console.WriteLine("Error Code: " + ex.Number);
                throw;
            }
        }

        // 执行查询并返回结果
        public List<Dictionary<string, object>> ExecuteQuery(string query, Dictionary<string, object> parameters = null)
        {
            var results = new List<Dictionary<string, object>>();
            using (var connection = GetConnection())
            using (var command = new MySqlCommand(query, connection))
            {
                // 参数化查询
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(new MySqlParameter(param.Key, param.Value ?? DBNull.Value));
                    }
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var columnName = reader.GetName(i);
                            var value = reader[i];

                            // 判断是否是 Guid 类型的列，尝试转换
                            if (value is string && Guid.TryParse(value.ToString(), out Guid guidValue))
                            {
                                row[columnName] = guidValue;
                            }
                            else
                            {
                                row[columnName] = value;
                            }
                        }
                        results.Add(row);
                    }
                }
            }
            return results;
        }

        // 执行非查询（例如：INSERT, UPDATE, DELETE）
        public int ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = GetConnection())
            using (var command = new MySqlCommand(query, connection))
            {
                // 参数化查询
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                return command.ExecuteNonQuery();
            }
        }

        // 执行查询并返回单一结果（例如：COUNT, MAX 等聚合函数）
        public object ExecuteScalar(string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = GetConnection())
            using (var command = new MySqlCommand(query, connection))
            {
                // 参数化查询
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                return command.ExecuteScalar();
            }
        }

        // 执行异步查询并返回结果
        public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query, Dictionary<string, object> parameters = null)
        {
            var results = new List<Dictionary<string, object>>();

            using (var connection = GetConnection())
            using (var command = new MySqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader[i];
                        }
                        results.Add(row);
                    }
                }
            }

            return results;
        }

        // 执行异步非查询操作（例如：INSERT, UPDATE, DELETE）
        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = GetConnection())
            using (var command = new MySqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                return await command.ExecuteNonQueryAsync();
            }
        }

        // 执行异步查询并返回单一结果（例如：COUNT, MAX 等聚合函数）
        public async Task<object> ExecuteScalarAsync(string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = GetConnection())
            using (var command = new MySqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                return await command.ExecuteScalarAsync();
            }
        }
    }
}

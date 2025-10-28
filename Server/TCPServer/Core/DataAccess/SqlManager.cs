using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace TCPServer.Core.DataAccess
{
    public class SqlManager
    {
        private static SqlManager _instance;
        private static readonly object _lock = new object();
        private readonly string _connectionString;

        private SqlManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static SqlManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            throw new InvalidOperationException("SqlManager not initialized. Call Initialize first.");
                    }
                }

                return _instance;
            }
        }

        public static void Initialize(string connectionString)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SqlManager(connectionString);
                        LoggerHelper.Instance.Info("SqlManager initialized.");
                    }
                }
            }
        }

        private MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        #region 同步操作

        public int ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(query, conn);
            AddParameters(cmd, parameters);
            LoggerHelper.Instance.Info("ExecuteNonQuery:\nSQL: " + FormatSqlInline(query, parameters));
            return cmd.ExecuteNonQuery();
        }

        public object ExecuteScalar(string query, Dictionary<string, object> parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(query, conn);
            AddParameters(cmd, parameters);

            //LoggerHelper.Instance.Info($"ExecuteScalar: {query} | Parameters: {FormatParameters(parameters)}");
            LoggerHelper.Instance.Info("ExecuteScalar:\nSQL: " + FormatSqlInline(query, parameters));
            return cmd.ExecuteScalar();
        }

        public List<Dictionary<string, object>> ExecuteQuery(string query, Dictionary<string, object> parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(query, conn);
            AddParameters(cmd, parameters);

            LoggerHelper.Instance.Info("ExecuteQuery:\nSQL: " + FormatSqlInline(query, parameters));

            var results = new List<Dictionary<string, object>>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(ReadRow(reader));
            }

            return results;
        }

        #endregion

        #region 异步操作

        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters,
            MySqlConnection conn, MySqlTransaction tran)
        {
            using var cmd = new MySqlCommand(query, conn, tran);
            AddParameters(cmd, parameters);
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<object> ExecuteScalarAsync(string query, Dictionary<string, object> parameters,
            MySqlConnection conn, MySqlTransaction tran)
        {
            using var cmd = new MySqlCommand(query, conn, tran);
            AddParameters(cmd, parameters);
            return await cmd.ExecuteScalarAsync();
        }

        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(query, conn);
            AddParameters(cmd, parameters);

            LoggerHelper.Instance.Info("ExecuteNonQueryAsync:\nSQL: " + FormatSqlInline(query, parameters));

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<object> ExecuteScalarAsync(string query, Dictionary<string, object> parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(query, conn);
            AddParameters(cmd, parameters);
            LoggerHelper.Instance.Info("ExecuteScalarAsync:\nSQL: " + FormatSqlInline(query, parameters));

            return await cmd.ExecuteScalarAsync();
        }

        public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query,
            Dictionary<string, object> parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(query, conn);
            AddParameters(cmd, parameters);

            LoggerHelper.Instance.Info("ExecuteQueryAsync:\nSQL: " + FormatSqlInline(query, parameters));

            var results = new List<Dictionary<string, object>>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(ReadRow(reader));
            }

            return results;
        }

        #endregion

        #region 事务操作

        public async Task<T> ExecuteTransactionAsync<T>(Func<MySqlConnection, MySqlTransaction, Task<T>> action)
        {
            using var conn = GetConnection();
            using var transaction = await conn.BeginTransactionAsync();
            try
            {
                T result = await action(conn, transaction);
                await transaction.CommitAsync();
                LoggerHelper.Instance.Info("Transaction committed successfully.");
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                LoggerHelper.Instance.Info($"Transaction rolled back. Error: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region 辅助方法

        private void AddParameters(MySqlCommand cmd, Dictionary<string, object> parameters)
        {
            if (parameters == null) return;
            foreach (var kv in parameters)
                cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
        }

        private Dictionary<string, object> ReadRow(System.Data.Common.DbDataReader reader)
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.GetValue(i);
            return row;
        }

        private string FormatSqlInline(string sql, Dictionary<string, object> parameters)
        {
            if (parameters != null)
            {
                foreach (var kv in parameters)
                {
                    string valueStr = kv.Value switch
                    {
                        string s => $"'{s}'",
                        DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
                        null => "NULL",
                        _ => kv.Value.ToString()
                    };
                    sql = sql.Replace(kv.Key, valueStr);
                }
            }

            // 压缩多余空白字符为一个空格
            var parts = sql.Split(new char[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts);
        }

        #endregion
    }
}
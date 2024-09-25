using System;
using System.Data;
using MySql.Data.MySqlClient;
using UnityEngine;
using UnityEngine.UI;

public class SQLManager:MonoBehaviour
{
    private MySqlConnection connection;

    public SQLManager()
    {
        Initialize();
    }

    private InputField accountField;
    private InputField passwordField;

    void Awake()
    {
        accountField = transform.Find("Account").gameObject.GetComponent<InputField>();
        passwordField = transform.Find("Password").gameObject.GetComponent<InputField>();
    }

    void Start()
    {
        CreateDataTable();
    }

    public void RegisterAccount()
    {
        if (accountField.text != "" && passwordField.text != "")
        {
            if (Insert(accountField.text, passwordField.text))
            {
                Debug.LogError("注册成功");
            }
            else
            {
                Debug.LogError("注册失败");
            }
        }
        else
        {
            Debug.LogError("请输入账号和密码");
        }
    }

    public void ChangePassword()
    {
        if (accountField.text != "" && passwordField.text != "")
        {
            if (Change(accountField.text, passwordField.text))
            {
                Debug.LogError("修改成功");
            }
            else
            {
                Debug.LogError("修改失败");
            }
        }
        else
        {
            Debug.LogError("请输入账号和密码");
        }
    }

    public void FindAccount()
    {
        if (accountField.text != "")
        {
            if (Find(accountField.text))
            {
                Debug.LogError("查找成功");
            }
            else
            {
                Debug.LogError("查找失败");
            }
        }
        else
        {
            Debug.LogError("请输入要查找的账号");
        }
    }

    public void DeleteAccount()
    {
        if (accountField.text != "")
        {
            if (Delete(accountField.text))
            {
                Debug.LogError("删除成功");
            }
            else
            {
                Debug.LogError("删除失败");
            }
        }
        else
        {
            Debug.LogError("请输入要删除的账号");
        }
    }


    //初始化连接login库
    private void Initialize()
    {
        string sqlSer = "server = gz-cynosdbmysql-grp-qbnq8bv5.sql.tencentcdb.com;" +
                        "port = 28311;" +
                        "database = login;" +
                        "user = root;" +
                        "password = Ww6221905;";
        connection = new MySqlConnection(sqlSer);
    }

    //打开Sql
    public bool OpenConnection()
    {
        try
        {
            connection.Open();
            return true;
        }
        catch (MySqlException ex)
        {
            switch (ex.Number)
            {
                case 0:
                    Console.WriteLine("无法连接到服务器。");
                    break;
                case 1045:
                    Console.WriteLine("用户名或密码无效，请重试。");
                    break;
                default:
                    Console.WriteLine($"连接错误： {ex.Message}");
                    break;
            }
            return false;
        }
    }

    //关闭Sql
    public bool CloseConnection()
    {
        try
        {
            connection.Close();
            return true;
        }
        catch (MySqlException ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    //查询库里是否有数据表
    public bool HasDataTable(string tableName)
    {
        string query = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{connection.Database}' AND table_name = '{tableName}'";
        MySqlCommand cmd = new MySqlCommand(query, connection);
        int result = Convert.ToInt32(cmd.ExecuteScalar());
        return result > 0;
    }

    //创建数据表
    public void CreateDataTable()
    {
        string name = "register_data";
        string query = $"CREATE TABLE {name} (id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, user_account VARCHAR(11), user_password VARCHAR(20))";
        if (this.OpenConnection() == true)
        {
            if (HasDataTable(name) == false)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
            }
            this.CloseConnection();
        }
    }


    //增删查改

    //增(完成)
    private bool Insert(string user_account, string user_password)
    {
        string query = $"INSERT INTO register_data (user_account, user_password) VALUES('{user_account}', {user_password})";
        if (this.OpenConnection() == true)
        {
            MySqlCommand cmd = new MySqlCommand(query, connection);
            int result = cmd.ExecuteNonQuery();
            this.CloseConnection();
            return result > 0;
        }
        else
        {
            return false;
        }
    }

    //删(完成)
    private bool Delete(string user_account)
    {
        string query = $"DELETE FROM {"register_data"} WHERE {"user_account"} = @value";

        if (this.OpenConnection() == true)
        {
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@value", user_account);
            int result = cmd.ExecuteNonQuery();
            this.CloseConnection();
            return result > 0;
        }
        else
        {
            return false;
        }
    }

    //查(完成)
    private bool Find(string user_account)
    {
        string query = $"SELECT COUNT(*) FROM {"register_data"} WHERE {"user_account"}  = @value";
        if (this.OpenConnection() == true)
        {
            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@value", user_account);
            int result = Convert.ToInt32(cmd.ExecuteScalar());
            this.CloseConnection();
            return result > 0;
        }
        else
        {
            return false;
        }
    }

    //改(完成)
    private bool Change(string user_account,string user_password)
    {
        string query = $"UPDATE {"register_data"} SET {"user_password"} = @newpassword WHERE {"user_account"} = @account";
        if (this.OpenConnection() == true)
        {
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@newpassword", user_password);
            command.Parameters.AddWithValue("@account", user_account);
            int result = command.ExecuteNonQuery();
            this.CloseConnection();
            return result > 0;
        }
        else
        {
            return false;
        }
    }
}
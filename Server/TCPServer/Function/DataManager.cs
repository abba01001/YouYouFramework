using System;
using MessagePack;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Protocols;

[MessagePackObject(keyAsPropertyName: true)]
public partial class DataManager 
{
    private static DataManager _instance;
    private static readonly object _lock = new object();
    private DataManager() { }
    public static DataManager Instance
    {
        get
        {
            // 双重检查锁定，确保线程安全并且只有第一次访问时才会创建实例
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DataManager();
                    }
                }
            }
            return _instance;
        }
    }


    #region 持久化数据

    private string _user_id;

    public string UserId
    {
        get
        {
            if (string.IsNullOrEmpty(_user_id)) _user_id = Guid.NewGuid().ToString("N");
            return _user_id;
        }
        set
        {
            _user_id = value;
        }
    }

    private bool _is_first_login_time;
    public bool IsFirstLoginTime
    {
        get => _is_first_login_time;
        set => _is_first_login_time = value;
    }
    private int _data_update_time; //保存数据时的时间戳
    public int DataUpdateTime { get => _data_update_time; set { _data_update_time = value; } }
    
    private int _last_refresh_time; //刷新时间
    public int LastRefreshTime{get => _last_refresh_time; set { _last_refresh_time = value; }}
    
    private PlayerRoleData _playerRoleData;
    public PlayerRoleData PlayerRoleData
    {
        get => _playerRoleData;
        set => _playerRoleData = value;
    }

    #endregion

    public string PrintUserData()
    {
        var str = MessagePackSerializer.SerializeToJson(this, MessagePackSerializer.DefaultOptions);
        return str;
    }

    private void InitializeWithDefaultData()
    {
        // 为每个属性设置默认值
        // 这里只是示例，具体的默认值根据你的需求设置
        InitPlayData();

        // 继续为其他属性初始化默认值
        // ...
    }

    public void InitPlayData()
    {
        _playerRoleData = new PlayerRoleData();
        _playerRoleData.roleAttr.Add("huo_bi_1", 0);
        _playerRoleData.roleAttr.Add("huo_bi_3", 2000);
        _playerRoleData.roleAttr.Add("huo_bi_4", 0);
        _playerRoleData.roleAttr.Add("role_level", 1);
        _playerRoleData.roleAttr.Add("role_exp", 0);
        _playerRoleData.roleAttr.Add("role_liveness", 0);
        _playerRoleData.roleAttr.Add("map_level", 1);
        _playerRoleData.roleAttr.Add("map_level_num", 1);
    }

    public void InitGameData(byte[] datas)
    {
        InitializeWithDefaultData();
        if (datas is { Length: > 0 })
        {
            try
            {
                StringBuilder logBuilder = new StringBuilder();
                DataManager mc2 = MessagePackSerializer.Deserialize<DataManager>(datas);


                PropertyInfo[] properties = mc2.GetType().GetProperties();
                LoggerHelper.Instance.Info($"========初始化GameData========");
                foreach (var property in properties)
                {
                    var value = property.GetValue(mc2);
                    var targetProperty = this.GetType().GetProperty(property.Name);
                    LoggerHelper.Instance.Info($"设置 {property.Name} =======> {value}");
                    if (targetProperty != null && targetProperty.CanWrite && value != null)
                    {
                        targetProperty.SetValue(this, value);
                    }
                }

                string json = MessagePackSerializer.SerializeToJson(this, MessagePackSerializer.DefaultOptions);
                LoggerHelper.Instance.Info(json);
            }
            catch (MessagePackSerializationException ex)
            {
                LoggerHelper.Instance.Error($"初始化失败 {ex.Message}===>使用默认值初始化");
            }
        }
        else
        {
            LoggerHelper.Instance.Error($"数据库没有数据====>使用默认值初始化");
        }
    }
}
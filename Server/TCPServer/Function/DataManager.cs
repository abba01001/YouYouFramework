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
    
    public void InitGameData(byte[] datas)
    {
        if (datas is {Length: > 0})
        {
            try
            {
                StringBuilder logBuilder = new StringBuilder();
                DataManager mc2 = MessagePackSerializer.Deserialize<DataManager>(datas);

                PropertyInfo[] properties = mc2.GetType().GetProperties();
                logBuilder.AppendLine($"========玩家GameData数据========");
                foreach (var property in properties)
                {
                    var value = property.GetValue(mc2);
                    var targetProperty = this.GetType().GetProperty(property.Name);
                    //logBuilder.AppendLine($"设置 {property.Name} =======> {value}");
                    if (targetProperty != null && targetProperty.CanWrite && value != null)
                    {
                        targetProperty.SetValue(this, value);
                    }
                }
                string json = MessagePackSerializer.SerializeToJson(this, MessagePackSerializer.DefaultOptions);
                logBuilder.AppendLine(json);
                LoggerHelper.Instance.Debug(logBuilder.ToString());
            }
            catch (Exception ex) { }
        }
    }
}
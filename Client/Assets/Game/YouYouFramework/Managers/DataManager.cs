using System;
using Main;
using MessagePack;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Protocols;
using UnityEngine;


public interface IDataManager
{
    void SaveData(bool writeLocal = true,bool ignoreLocalTime = false,bool writeCloud = false,bool ignoreCloudTime = false);
    void SaveData(bool forceWriteCloud);
    PlayerRoleData PlayerRoleData { get; set; }
    int DataUpdateTime { get; set; }
    void InitPlayData();
}

[MessagePackObject(keyAsPropertyName: true)]
public class DataManager : Observable<DataManager>, IDataManager
{
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

    #region 临时public数据

    [IgnoreMember]
    public long Coin => _playerRoleData.roleAttr["coin"];
    [IgnoreMember] public int TempSelectMapLvNum { get; set; } = 1;
    [IgnoreMember] public int TempSelectMapLv { get; set; } = 1;
    [IgnoreMember] public int SuspendStartTime { get; set; } = -1;//挂机奖励开始时间点
    [IgnoreMember] public int SuspendQuickGetRewardIndex { get; set; } = -1;//挂机奖励开始时间点
    [IgnoreMember] public int SuspendQuickGetRewardLimit { get; set; } = -1;//挂机奖励开始时间点
    [IgnoreMember] public bool RequestPublicChat { get; set; } = false;//挂机奖励开始时间点
    [IgnoreMember] public List<List<ChatMsg>> TempChatMsgs { get; set; } = new List<List<ChatMsg>>();
    #endregion

    #region 临时private数据
    [IgnoreMember] private float lastWriteTime = 0f; // 上次写入时间
    [IgnoreMember] private float lastUploadTime = 0f; // 上次上传时间
    [IgnoreMember] private float writeCooldown = 5f; // 写入的冷却时间（5秒）
    [IgnoreMember] private float uploadCooldown = 10f; // 上传的冷却时间（10秒）

    [IgnoreMember]
    public int RoleLevel => _playerRoleData.roleAttr["role_level"];
    public int MapLevel => _playerRoleData.roleAttr["map_level"];
    public int MapLevelNum => _playerRoleData.roleAttr["map_level_num"];
    #endregion

    #region Public方法

    public void AddMoney(int count)
    {
        _playerRoleData.roleAttr["coin"] += count;
        GameEntry.Event.Dispatch(Constants.EventName.SetMoneyText,_playerRoleData.roleAttr["coin"]);
    }
    
    public void LessMoney(int count = 1)
    {
        if (_playerRoleData.roleAttr["coin"] == 0) return;
        _playerRoleData.roleAttr["coin"] -= count;
        GameEntry.Event.Dispatch(Constants.EventName.SetMoneyText,_playerRoleData.roleAttr["coin"]);
    }
    
    public string PrintUserData()
    {
        var str = MessagePackSerializer.SerializeToJson(this, MessagePackSerializer.DefaultOptions);
        Debug.Log(str);
        return str;
    }
    
    public void InitPlayData()
    {
        _playerRoleData = new PlayerRoleData();
        _playerRoleData.roleAttr.Add("coin",0);
        _playerRoleData.roleAttr.Add("huo_bi_1",0);
        _playerRoleData.roleAttr.Add("huo_bi_3",2000);
        _playerRoleData.roleAttr.Add("huo_bi_4",0);
        _playerRoleData.roleAttr.Add("role_level",1);
        _playerRoleData.roleAttr.Add("role_exp", 0);
        _playerRoleData.roleAttr.Add("role_liveness", 0);
        _playerRoleData.roleAttr.Add("map_level",1);
        _playerRoleData.roleAttr.Add("map_level_num",1);
    }
    
    public void OnUpdate()
    {
        
    }
    
    public void InitGameData(byte[] datas)
    {
        InitializeWithDefaultData();
        if (datas is {Length: > 0})
        {
            try
            {
                StringBuilder logBuilder = new StringBuilder();
                DataManager mc2 = MessagePackSerializer.Deserialize<DataManager>(datas);
                
                
                PropertyInfo[] properties = mc2.GetType().GetProperties();
                logBuilder.AppendLine($"========初始化GameData========");
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
            }
            catch (MessagePackSerializationException ex)
            {
                GameUtil.LogError($"初始化失败 {ex.Message}===>使用默认值初始化");
            }
        }
        else
        {
            GameUtil.LogError($"数据库没有数据====>使用默认值初始化");
        }
    }

    public void SaveData(bool forceWriteCloud)
    {
        SaveData(true, true, forceWriteCloud, true);
    }

    private TimeAction SaveAction = null;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="writeLocal">写入本地</param>
    /// <param name="ignoreLocalTime">无视cd写入</param>
    /// <param name="writeCloud">写入云端</param>
    /// <param name="ignoreCloudTime">无视cd写入</param>
    public void SaveData(bool writeLocal = true,bool ignoreLocalTime = false,bool writeCloud = false,bool ignoreCloudTime = false)
    {
        return;
        _data_update_time = (int)GameEntry.Time.GetNetTime();
        var binaryData = MessagePackSerializer.Serialize(this, MessagePackSerializer.DefaultOptions);
        
        var str = Convert.ToBase64String(binaryData);
        if (writeLocal)
        {
            if (Time.time - lastWriteTime >= writeCooldown || ignoreLocalTime)
            {
                PlayerPrefs.SetString("SaveData", str);
                string json = MessagePackSerializer.SerializeToJson(this, MessagePackSerializer.DefaultOptions);
                lastWriteTime = Time.time;  // 更新写入的时间
                MainEntry.Log(MainEntry.LogCategory.GameData,$"保存本地数据=={json}");
            }
        }
        
        if (!GameEntry.Net.IsConnectServer) return;
        if (writeCloud)
        {
            string json = MessagePackSerializer.SerializeToJson(this, MessagePackSerializer.DefaultOptions);
            MainEntry.Log(MainEntry.LogCategory.GameData,$"上传云端?{Time.time - lastUploadTime >= uploadCooldown || ignoreCloudTime}");
            MainEntry.Log(MainEntry.LogCategory.GameData,$"数据====>{json}");
            if (Time.time - lastUploadTime >= uploadCooldown || ignoreCloudTime)
            {
                if (SaveAction != null) SaveAction.Stop();
                SaveAction = GameEntry.Time.CreateTimer(this, 0.02f, () =>
                {
                    MainEntry.Log(MainEntry.LogCategory.GameData,$"上传数据=={str}");
                    GameEntry.SDK.UploadGameData(UserId, str);
                    lastUploadTime = Time.time;  // 更新上传的时间
                });
            }
        }
    }
    
    #endregion

    #region Private方法

    private void InitializeWithDefaultData()
    {
        // 为每个属性设置默认值
        // 这里只是示例，具体的默认值根据你的需求设置
        InitPlayData();
        
        // 继续为其他属性初始化默认值
        // ...
    }

    #endregion
}
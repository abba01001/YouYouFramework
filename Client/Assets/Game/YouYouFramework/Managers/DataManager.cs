using System;
using Main;
using MessagePack;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MessagePack.Resolvers;
using UnityEngine;
using YouYou;

public interface IDataManager
{
    void SaveData(bool writeLocal = true,bool ignoreLocalTime = false,bool writeCloud = false,bool ignoreCloudTime = false);
    void SaveDialogueId(int type, int id);
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
            SaveData();
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
        set
        {
            _playerRoleData = value;
            SaveData();
        }
    }

    #endregion

    #region 临时public数据
    [IgnoreMember] public long Coin { get; }
    #endregion

    #region 临时private数据
    [IgnoreMember] private float lastWriteTime = 0f; // 上次写入时间
    [IgnoreMember] private float lastUploadTime = 0f; // 上次上传时间
    [IgnoreMember] private float writeCooldown = 5f; // 写入的冷却时间（5秒）
    [IgnoreMember] private float uploadCooldown = 10f; // 上传的冷却时间（10秒）
    
    #endregion

    #region Public方法

    public void SaveDialogueId(int type, int id)
    {
        if (PlayerRoleData.dialogueIds.ContainsKey(type))
        {
            PlayerRoleData.dialogueIds[type] = id;
        }
        else
        {
            PlayerRoleData.dialogueIds.Add(type, id);
        }
    }

    public void InitPlayData()
    {
        _playerRoleData = new PlayerRoleData();
        _playerRoleData.roleAttr.Add("role_level",1);
        _playerRoleData.roleAttr.Add("role_exp", 0);
        _playerRoleData.roleAttr.Add("role_heat", 0);
        _playerRoleData.roleAttr.Add("role_coin", 0);
        _playerRoleData.roleAttr.Add("role_snow", 0);
        _playerRoleData.roleAttr.Add("role_liveness", 0);
    }
    
    public void OnUpdate()
    {
        
    }
    
    public void InitGameData(byte[] datas)
    {
        if (datas is {Length: > 0})
        {
            DataManager mc2 = MessagePackSerializer.Deserialize<DataManager>(datas);
            PropertyInfo[] properties = mc2.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(mc2);
                var targetProperty = this.GetType().GetProperty(property.Name);
                if (targetProperty != null && targetProperty.CanWrite)
                {
                    targetProperty.SetValue(this, value);
                }
            }
        }
        else
        {
            // 数据为空时，使用默认值进行初始化
            InitializeWithDefaultData();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="writeLocal">写入本地</param>
    /// <param name="ignoreLocalTime">无视cd写入</param>
    /// <param name="writeCloud">写入云端</param>
    /// <param name="ignoreCloudTime">无视cd写入</param>
    public void SaveData(bool writeLocal = true,bool ignoreLocalTime = false,bool writeCloud = false,bool ignoreCloudTime = false)
    {
        if (!Constants.IsLoginGame) return;
        _data_update_time = (int)GameEntry.Time.GetNetTime();
        var binaryData = MessagePackSerializer.Serialize(this, MessagePackSerializer.DefaultOptions);
        if (writeLocal)
        {
            if (Time.time - lastWriteTime >= writeCooldown || ignoreLocalTime)
            {
                var str = Convert.ToBase64String(binaryData);
                PlayerPrefs.SetString(GameEntry.Data.UserId, str);
                
                string json = MessagePackSerializer.SerializeToJson(this, MessagePackSerializer.DefaultOptions);
                lastWriteTime = Time.time;  // 更新写入的时间
                MainEntry.Log(MainEntry.LogCategory.GameData,$"写入本地文件=={json}");
            }
        }
        if (writeCloud)
        {
            MainEntry.Log(MainEntry.LogCategory.GameData,$"上传云端?{Time.time - lastUploadTime >= uploadCooldown}");
            if (Time.time - lastUploadTime >= uploadCooldown || ignoreCloudTime)
            {
                GameEntry.SDK.UploadGameData(UserId, binaryData);
                lastUploadTime = Time.time;  // 更新上传的时间
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
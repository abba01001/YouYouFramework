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
    void SetPlayerPos(Vector3 pos);
    void LoadGameData();
    PlayerRoleData PlayerRoleData { get; set; }
    int DataUpdateTime { get; set; }
}

[MessagePackObject(keyAsPropertyName: true)]
public class DataManager : Observable<DataManager>, IDataManager
{
    #region 持久化数据

    private string _user_id;
    public string UserId
    {
        get => _user_id;
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
        set
        {
            _is_first_login_time = value;
            SaveData();
        }
    }
    private int _data_update_time; //保存数据时的时间戳
    public int DataUpdateTime { get => _data_update_time; set { _data_update_time = value; } }
    
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
    [IgnoreMember] private List<float> playerPosCache = new List<float>(3){0,0,0};
    [IgnoreMember] private List<float> playerRotateCache = new List<float>(3){0,0,0};
    [IgnoreMember] private List<float> playerBornPosCache = new List<float>(3){0,0,0};
    [IgnoreMember] private List<float> cameraRotateCache = new List<float>(3){0,0,0};
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

    public void SetPlayerPos(Vector3 pos)
    {
        playerPosCache[0] = pos.x;
        playerPosCache[1] = pos.y;
        playerPosCache[2] = pos.z;
        PlayerRoleData.playerPos = playerPosCache;
    }

    public void SetPlayerRotate(Vector3 rotate)
    {
        playerRotateCache[0] = rotate.x;
        playerRotateCache[1] = rotate.y;
        playerRotateCache[2] = rotate.z;
        PlayerRoleData.playerRotate = playerRotateCache;
    }

    public void SetPlayerBornPos(Vector3 pos)
    {
        playerBornPosCache[0] = pos.x;
        playerBornPosCache[1] = pos.y;
        playerBornPosCache[2] = pos.z;
        PlayerRoleData.playerBornPos = playerBornPosCache;
    }

    public void SetCameraRotate(Vector3 rotate)
    {
        cameraRotateCache[0] = rotate.x;
        cameraRotateCache[1] = rotate.y;
        cameraRotateCache[2] = rotate.z;
        PlayerRoleData.cameraRotate = cameraRotateCache;
    }

    #endregion

    public void OnUpdate()
    {
        
    }

    public void Init()
    {
        InitGameData(null);
    }
    
    
    public void InitGameData(byte[] datas)
    {
        if (datas != null)
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
            if (string.IsNullOrEmpty(_user_id)) _user_id = Guid.NewGuid().ToString("N");
            _playerRoleData = new PlayerRoleData();
        }
    }
    
    
    public void LoadGameData()
    {
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
                GameUtil.LogError($"写入本地++++===={json}");
            }
        }
        if (writeCloud)
        {
            GameUtil.LogError($"开始上传{Time.time - lastUploadTime >= uploadCooldown}");
            if (Time.time - lastUploadTime >= uploadCooldown || ignoreCloudTime)
            {
                GameEntry.SDK.UploadGameData(UserId, binaryData);
                lastUploadTime = Time.time;  // 更新上传的时间
            }
        }
    }
}
using Main;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using YouYou;
using Random = System.Random;

public class PlayerDataMgr : Observable<PlayerDataMgr>
{
    #region 玩家数据

    private GameData GenerateGameData()
    {
        var data = new GameData
        {
            playerRoleData = PlayerRoleData,
            commonIntDic = CommonIntDic,
            commonFloatDic = CommonFloatDic,
            commonStringDic = CommonStringDic,
            uuid = Constants.UserID,
            saveTime = GameEntry.Time.GetNetTime().ToString()
        };
        return data;
    }

    private void HandleLoadData(GameData gameData)
    {
        PlayerRoleData = gameData.playerRoleData;
        CommonIntDic = gameData.commonIntDic;
        CommonFloatDic = gameData.commonFloatDic;
        CommonStringDic = gameData.commonStringDic;
        if (gameData.uuid == String.Empty) gameData.uuid = GenerateUniqueID();
        Constants.UserID = gameData.uuid;
    }

    [Serializable]
    public class GameData
    {
        public Dictionary<string, int> commonIntDic = new Dictionary<string, int>();
        public Dictionary<string, float> commonFloatDic = new Dictionary<string, float>();
        public Dictionary<string, string> commonStringDic = new Dictionary<string, string>();
        public PlayerRoleData playerRoleData = new PlayerRoleData();
        public string uuid = GenerateUniqueID();
        public string saveTime = GameEntry.Time.GetNetTime().ToString();
    }

    #endregion

    public void InitGameData()
    {
        string url = string.Format("{0}{1}", SystemModel.Instance.CurrChannelConfig.GameDataUrl, "gamedata.json");
        MainEntry.Download.DownloadGameData(url, null, (string fileUrl) =>
        {
            if (fileUrl == Constants.EMPTYGAMEDATA)
            {
                SaveDataAll(true, true);
                //没有文件，则new出一个并上传
            }
            else if (fileUrl == Constants.REQUESTFAIL)
            {
                //弹窗，网路不好？
            }
            else
            {
                LoadGameData();
                //加载GameData
            }

            Constants.IsLoadCouldData = true;
        });
    }

    public void OnUpdate()
    {
    }



    public void Init()
    {
        GameEntry.Time.CreateTimerLoop(this, 1f, -1, (int loop) =>
        {
            UpdateOnlineTime();
        });
        GameEntry.Time.CreateTimerLoop(this, 10f, -1, (int loop) =>
        {
            if (Constants.IsLoadCouldData)
            {
                //SaveDataAll(true, true);
            }
        });
    }

    public void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }


    public void SaveDataAll(bool writeToLocal = true, bool writeToCloud = false)
    {
        CommonIntDic.TryAdd("你好", 1);
        string jsonData = JsonConvert.SerializeObject(GenerateGameData());
        GameEntry.LogError($"上传json文件:{jsonData}");
        string filePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
        try
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            File.WriteAllText(filePath, SecurityUtil.Encrypt(jsonData));
        }
        catch (Exception ex)
        {
            GameEntry.LogError("保存数据到文件时发生错误: " + ex.Message);
        }

        if (writeToCloud)
        {
            GameEntry.SDK.UploadGameData(filePath);
        }
    }

    public void LoadGameData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
        try
        {
            if (File.Exists(filePath))
            {
                string encryptedData = File.ReadAllText(filePath);
                string jsonData = SecurityUtil.Decrypt(encryptedData);
                MainEntry.LogError(MainEntry.LogCategory.Assets, $"当前GameData文件---{jsonData}");
                HandleLoadData(JsonConvert.DeserializeObject<GameData>(jsonData));
            }
            else
            {
                GameEntry.LogError("文件不存在：" + filePath);
            }
        }
        catch (Exception ex)
        {
            GameEntry.LogError("加载数据时发生错误: " + ex.Message);
        }
    }

    private PlayerRoleData PlayerRoleData = new PlayerRoleData();
    private Dictionary<string, int> CommonIntDic = new Dictionary<string, int>();
    private Dictionary<string, float> CommonFloatDic = new Dictionary<string, float>();
    private Dictionary<string, string> CommonStringDic = new Dictionary<string, string>();

    #region 玩家数据
    private List<float> playerPosCache = new List<float>(3){0,0,0};
    private List<float> playerRotateCache = new List<float>(3){0,0,0};
    private List<float> cameraRotateCache = new List<float>(3){0,0,0};
    private List<float> playerBornPosCache = new List<float>(3){0,0,0};
    
    public PlayerRoleData GetPlayerRoleData()
    {
        return PlayerRoleData;
    }

    public void UpdateOnlineTime()
    {
        PlayerRoleData.todayOnlineDuration += 1;
        PlayerRoleData.totalOnlineDuration += 1;
    }
    
    public void SaveDialogueId(int type,int id)
    {
        if (PlayerRoleData.dialogueIds.ContainsKey(type))
        {
            PlayerRoleData.dialogueIds[type] = id;
        }
        else
        {
            PlayerRoleData.dialogueIds.Add(type,id);
        }
    }
    
    public void SetPlayerBornPos(Vector3 pos)
    {
        playerBornPosCache[0] = pos.x;
        playerBornPosCache[1] = pos.y;
        playerBornPosCache[2] = pos.z;
        PlayerRoleData.playerBornPos = playerBornPosCache;
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

    public void SetCameraRotate(Vector3 rotate)
    {
        cameraRotateCache[0] = rotate.x;
        cameraRotateCache[1] = rotate.y;
        cameraRotateCache[2] = rotate.z;
        PlayerRoleData.cameraRotate = cameraRotateCache;
    }

    #endregion

    #region int字典

    public int GetInt(string key, int defaultValue = 0)
    {
        if (CommonIntDic.TryGetValue(key, out int retValue))
            return retValue;
        else
            return defaultValue;
    }

    public void SetInt(string key, int value, object param = null)
    {
        CommonIntDic[key] = value;
        Dispatch(key, param);
    }

    public void SetIntAdd(string key, int value)
    {
        SetInt(key, GetInt(key) + value);
    }

    public void SetIntHas(string key, int value)
    {
        if (PlayerPrefs.HasKey(key.ToString())) return;
        SetInt(key, value);
    }

    public bool GetBool(string key, bool defaultValue)
    {
        return GetInt(key, defaultValue ? 1 : 0) == 1;
    }

    public bool GetBool(string key)
    {
        return GetInt(key) == 1;
    }

    public void SetBool(string key, bool value, object param = null)
    {
        SetInt(key, value ? 1 : 0);
        Dispatch(key, param);
    }

    public void SetBoolHas(string key, bool value)
    {
        if (PlayerPrefs.HasKey(key.ToString())) return;
        SetBool(key, value);
    }

    #endregion

    #region float字典

    public float GetFloat(string key, float defaultValue = 0)
    {
        if (CommonFloatDic.TryGetValue(key, out float retValue))
            return retValue;
        else
            return defaultValue;
    }

    public void SetFloat(string key, float value, object param = null)
    {
        CommonFloatDic[key] = value;
        Dispatch(key, param);
    }

    public void SetFloatAdd(string key, float value)
    {
        SetFloat(key, GetFloat(key) + value);
    }

    public void SetFloatHas(string key, float value)
    {
        if (PlayerPrefs.HasKey(key.ToString())) return;
        SetFloat(key, value);
    }

    #endregion

    #region string字典

    public string GetString(string key, string defaultValue = null)
    {
        if (CommonStringDic.TryGetValue(key, out string retValue))
            return retValue;
        else
            return defaultValue;
    }

    public void SetString(string key, string value, object param = null)
    {
        CommonStringDic[key] = value;
        Dispatch(key, param);
    }

    public void SetStringHas(string key, string value)
    {
        if (CommonStringDic.ContainsKey(key)) return;
        SetString(key, value);
    }

    #endregion

    public T GetObject<T>(string key) where T : new()
    {
        string value = PlayerPrefs.GetString(key);
        if (!string.IsNullOrEmpty(value))
        {
            return value.ToObject<T>();
        }
        else
        {
            return new T();
        }
    }

    public void SetObject<T>(string key, T data)
    {
        PlayerPrefs.SetString(key, data.ToJson());
    }

    public static string GenerateUniqueID()
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Random random = new Random();
        int randomValue = random.Next(100000, 999999); // 生成6位随机数
        return timestamp.ToString() + randomValue.ToString();
    }
}
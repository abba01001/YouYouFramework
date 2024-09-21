using Main;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using YouYou;


public class PlayerPrefsDataMgr : Observable<PlayerPrefsDataMgr, PlayerPrefsDataMgr.EventName>
{
    [Serializable]
    public class GameData
    {
        public Dictionary<string, int> intDic = new Dictionary<string, int>();
        public Dictionary<string, float> floatDic = new Dictionary<string, float>();
        public Dictionary<string, string> stringDic = new Dictionary<string, string>();
    }
    
    public enum EventName : uint
    {
        //主音量
        MasterVolume,
        //背景音乐音量
        BGMVolume,
        //音效音量
        AudioVolume,
        //游戏暂停
        GamePause,
        //最大帧率
        FrameRate,
        //屏幕分辨率
        Screen,
        //画质等级
        QualityLevel,

        //测试事件
        TestEvent,
    }

    public void Init()
    {
        //一定要等拉取存档完成才进入场景
        //第一次拉取存档，如果没有，则新建一个
        //云端拉GameData
        // GameEntry.PlayerPrefs.SetFloatHas(EventName.MasterVolume, 1);
        // GameEntry.PlayerPrefs.SetFloatHas(EventName.AudioVolume, 1);
        // GameEntry.PlayerPrefs.SetFloatHas(EventName.BGMVolume, 1);
        // GameEntry.PlayerPrefs.SetIntHas(EventName.FrameRate, 2);
    }
    public void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    public GameData GetNewGameData()
    {
        var data = new GameData
        {
            intDic = IntDic,
            floatDic = FloatDic,
            stringDic = StringDic
        };
        return data;
    }
    
    public void SaveDataAll(bool upload = false)
    {
        IntDic.Add("你好", 1);
        string jsonData = JsonConvert.SerializeObject(GetNewGameData());
        string filePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
        try
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            File.WriteAllText(filePath, SecurityUtil.Encrypt(jsonData));
            GameEntry.LogError("数据已成功保存到文件中：" + filePath);
        }
        catch (Exception ex)
        {
            GameEntry.LogError("保存数据到文件时发生错误: " + ex.Message);
        }
        if (upload)
        {
            
        }
    }

    private Dictionary<string, int> IntDic = new Dictionary<string, int>();
    private Dictionary<string, float> FloatDic = new Dictionary<string, float>();
    private Dictionary<string, string> StringDic = new Dictionary<string, string>();

    #region int字典

    public int GetInt(string key, int defaultValue = 0)
    {
        if (IntDic.TryGetValue(key, out int retValue))
            return retValue;
        else
            return defaultValue;
    }
    public void SetInt(string key, int value, object param = null)
    {
        IntDic[key] = value;
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
        if (FloatDic.TryGetValue(key, out float retValue))
            return retValue;
        else
            return defaultValue;
    }
    public void SetFloat(string key, float value, object param = null)
    {
        FloatDic[key] = value;
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
        if (StringDic.TryGetValue(key, out string retValue))
            return retValue;
        else
            return defaultValue;
    }
    public void SetString(string key, string value, object param = null)
    {
        StringDic[key] = value;
        Dispatch(key, param);
    }
    public void SetStringHas(string key, string value)
    {
        if (StringDic.ContainsKey(key)) return;
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

    public void SaveDataAllToFile()
    {
        IntDic.Add("你好", 1);
        var data = new GameData
        {
            intDic = IntDic,
        };

        string jsonData = JsonConvert.SerializeObject(data);
        GameEntry.LogError(jsonData);
        string filePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
        try
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            File.WriteAllText(filePath, SecurityUtil.Encrypt(jsonData));
            GameEntry.LogError("数据已成功保存到文件中：" + filePath);
        }
        catch (Exception ex)
        {
            GameEntry.LogError("保存数据到文件时发生错误: " + ex.Message);
        }
    }
}
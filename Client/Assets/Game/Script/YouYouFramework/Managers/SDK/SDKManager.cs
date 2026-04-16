using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using COSXML.Utils;
using Cysharp.Threading.Tasks;
using Main;
using MessagePack;
using Protocols.Game;
using UnityEngine;


public class SDKManager
{
    public void Init()
    {

    }
    public void OnUpdate()
    {

    }

    #region 腾讯云COS
    //上传数据到云端
    public async Task UploadGameData(string userId, string str)
    {
        GameSaveData data = new GameSaveData();
        GameEntry.Net.Requset.c2s_request_update_role_info(new Dictionary<string, string>()
        {
            {nameof(data.SaveData),str}
        });
        
        
        // CosXml cosXml = CreateCosXml();
        // // 生成文件名
        // string fileName = $"{userId}.bin";
        // using (MemoryStream memoryStream = new MemoryStream(binaryData))
        // {
        //     var dic = SecurityUtil.GetSecretKeyDic();
        //     PutObjectRequest request = new PutObjectRequest(dic["bucket"], "Unity/GameData/" + fileName, memoryStream);
        //     request.SetSign(TimeUtils.GetCurrentTime(TimeUnit.Seconds), 600);
        //     try
        //     {
        //         PutObjectResult result = await Task.Run(() => cosXml.PutObject(request));
        //         GameEntry.LogError($"游戏数据 {fileName} 上传成功{result.IsSuccessful()}！");
        //     }
        //     catch (Exception ex)
        //     {
        //         GameEntry.LogError($"{fileName} 上传状态：<color=red>失败</color>，错误：{ex.Message}");
        //     }
        // }
    }
    
    public async UniTask DownloadGameData(string userId)
    {
        CosXml cosXml = await CreateCosXml();
        string fileName = $"{userId}.bin";
        string tempFileName = $"temp_{userId}.bin";
        var dic =  SecurityUtil.GetSecretKeyDic();
        string bucketName = dic["bucket"];
        string filePath = "Unity/GameData/" + fileName; // COS 上的文件路径
        string localDir = Application.persistentDataPath;
        
        string localFilePath = Path.Combine(localDir, fileName); // 完整的存档文件路径
        string tempFilePath = Path.Combine(localDir, tempFileName); // 临时文件路径

        GetObjectRequest request = new GetObjectRequest(bucketName, filePath, localDir, tempFileName);
        try
        {
            GetObjectResult result = await Task.Run(() => cosXml.GetObject(request));
            if (result.httpCode >= 200 && result.httpCode < 300)
            {
                GameEntry.LogError($"下载成功，保存路径: {tempFilePath}");
                GameEntry.Data.InitGameData(GetGameData(localFilePath, tempFilePath));
                GameEntry.Event.Dispatch(Constants.EventName.LoginSuccess);
            }
            else
            {
                GameEntry.Data.InitGameData(null);
                GameEntry.Event.Dispatch(Constants.EventName.LoginSuccess);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"下载失败：{ex.Message}");
        }
    }

    public byte[] GetGameData(string localFilePath,string tempFilePath)
    {
        byte[] fileContent = default;

        if (File.Exists(localFilePath))
        {
            fileContent = CompareGameData(localFilePath,tempFilePath);
        }
        else
        {
            fileContent = File.ReadAllBytes(tempFilePath);
            File.Move(tempFilePath, localFilePath);
        }
        return fileContent;
    }
    
    //游戏存档校验
    public byte[] CompareGameData(string localFilePath,string tempFilePath)
    {
        byte[] localData = File.ReadAllBytes(localFilePath);
        byte[] cloudData = File.ReadAllBytes(tempFilePath);
        
        int cloudDataUpdateTime = default;
        DataManager mc1 = MessagePackSerializer.Deserialize<DataManager>(cloudData);
        foreach (var property in mc1.GetType().GetProperties())
        {
            if (property.Name == "DataUpdateTime")
            {
                cloudDataUpdateTime = property.GetValue(mc1).ToInt();
                break;
            }
        }
        
        int localDataUpdateTime = default;
        DataManager mc2 = MessagePackSerializer.Deserialize<DataManager>(localData);
        foreach (var property in mc2.GetType().GetProperties())
        {
            if (property.Name == "DataUpdateTime")
            {
                localDataUpdateTime = property.GetValue(mc2).ToInt();
                break;
            }
        }

        if (cloudDataUpdateTime > localDataUpdateTime)
        {
            if (File.Exists(localFilePath)) File.Delete(localFilePath);
            File.Move(tempFilePath, localFilePath);
        }
        else
        {
            File.Delete(tempFilePath);
        }
        return cloudDataUpdateTime > localDataUpdateTime ? cloudData : localData;
    }


    //下载头像
    public async Task<Texture2D> DownloadAvatar(string spriteId, Action<Texture2D> action = null)
    {
        CosXml cosXml = await CreateCosXml();
        string fileName = $"{spriteId}.jpg"; // 头像文件名
        var dic =  SecurityUtil.GetSecretKeyDic();
        string bucketName = dic["bucket"];
        string filePath = "Unity/HeadImage/" + fileName; // COS 上的文件路径
        string localDir = Path.Combine(Application.persistentDataPath, "HeadIcon"); // 创建 "HeadIcon" 文件夹路径
        if (!Directory.Exists(localDir))
        {
            Directory.CreateDirectory(localDir); // 如果文件夹不存在，创建它
        }

        string localFilePath = Path.Combine(localDir, fileName); // 完整的文件路径
        if (File.Exists(localFilePath))
        {
            Debug.Log($"头像已存在，直接加载本地头像：{localFilePath}");
            byte[] avatarBytes = File.ReadAllBytes(localFilePath);
            Texture2D texture = new Texture2D(2, 2); // 创建一个新的 Texture2D 对象
            texture.LoadRawTextureData(avatarBytes); // 直接加载原始纹理数据
            texture.Apply(); // 应用纹理数据
            action?.Invoke(texture);
            return texture;
        }
        else
        {
            Debug.Log("本地头像不存在，开始从COS下载");
            GetObjectRequest request = new GetObjectRequest(bucketName, filePath, localDir, fileName);
            try
            {
                GetObjectResult result = await Task.Run(() => cosXml.GetObject(request));
                if (result.httpCode == 200)
                {
                    Debug.Log($"头像下载成功，保存路径: {localFilePath}");
                    byte[] avatarBytes = File.ReadAllBytes(localFilePath);
                    Texture2D texture = new Texture2D(2, 2); // 创建一个新的 Texture2D 对象
                    texture.LoadImage(avatarBytes); // 加载字节数组为纹理
                    return texture;
                }
                else
                {
                    Debug.LogError($"头像下载失败，状态码：{result.httpCode}");
                    return null; // 如果下载失败，返回null
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"下载头像失败：{ex.Message}");
                return null; // 如果发生异常，返回null
            }
        }
    }

    static async UniTask<CosXml> CreateCosXml()
    {
        var dic =  SecurityUtil.GetSecretKeyDic();
        CosXmlConfig config = new CosXmlConfig.Builder()
            .SetConnectionTimeoutMs(60000) //设置连接超时时间，单位毫秒，默认45000ms
            .SetReadWriteTimeoutMs(40000) //设置读写超时时间，单位毫秒，默认45000ms
            .IsHttps(true) //设置默认 HTTPS 请求
            .SetAppid("1318826377")
            .SetRegion(dic["region"])
            .Build();

        long durationSecond = 600; //每次请求签名有效时长，单位为秒
        QCloudCredentialProvider qCloudCredentialProvider =
            new DefaultQCloudCredentialProvider(dic["secretId"], dic["secretKey"], durationSecond);
        return new CosXmlServer(config, qCloudCredentialProvider);
    }

    #endregion

    #region 后台

    // 调用这个方法发送POST请求
    public async void PostVersion()
    {
        string url = "http://159.75.164.29:32002/api/tutorials/getVersion";
        // 你要发送的数据，序列化成JSON格式
        var postData = new
        {
            key = "value",  // 根据API的实际需求构建请求体
            anotherKey = "anotherValue"
        };
        string jsonData = JsonUtility.ToJson(postData);
        // 调用 PostAsync 方法
        GameEntry.Http.Post(url, null, true, (s =>
        {
            Debugger.LogError(s);
        }));
    }
    #endregion

    #region TalkingData

    public void InitTalkingData()
    {
        Debugger.Log("========>开始初始化TalkingDataSdk");
        //TalkingDataSDK.SetVerboseLogDisable();//关闭日志
        // TalkingDataSDK.BackgroundSessionEnabled();
        // TalkingDataSDK.InitSDK(Constants.TalkingDataAppid, "102", "");

        //用户获得隐私授权后才能调用StartA()
        // TalkingDataSDK.StartA();
        Debugger.Log("初始化TalkingDataSDK完成");
    }

    #endregion
    
    #region 安卓sdk
    
    #endregion
}

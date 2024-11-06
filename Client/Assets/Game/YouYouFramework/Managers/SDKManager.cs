using System;
using System.IO;
using System.Threading.Tasks;
using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using COSXML.Utils;
using Main;
using MessagePack;
using UnityEngine;
using YouYou;

public enum DownloadStatus
{
    Success = 0,
    FileNotFound = 1,
    NetworkError = 2,
    UnknownError = 3
}

public class SDKManager : Observable<SDKManager>
{
    public void Init()
    {

    }
    public void OnUpdate()
    {

    }
    
    //上传数据到云端
    public async Task UploadGameData(string userId, byte[] binaryData)
    {
        CosXml cosXml = CreateCosXml();
        // 生成文件名
        string fileName = $"{userId}.bin";
        using (MemoryStream memoryStream = new MemoryStream(binaryData))
        {
            var dic = SecurityUtil.GetSecretKeyDic();
            PutObjectRequest request = new PutObjectRequest(dic["bucket"], "Unity/GameData/" + fileName, memoryStream);
            request.SetSign(TimeUtils.GetCurrentTime(TimeUnit.Seconds), 600);
            try
            {
                PutObjectResult result = await Task.Run(() => cosXml.PutObject(request));
                GameEntry.LogError($"文件 {fileName} 上传成功！");
            }
            catch (Exception ex)
            {
                GameEntry.LogError($"{fileName} 上传状态：<color=red>失败</color>，错误：{ex.Message}");
            }
        }
    }

    //从云端拉数据
    public async Task<(DownloadStatus, byte[])> DownloadGameData(string userId)
    {
        CosXml cosXml = CreateCosXml();
        string fileName = $"{userId}.bin";  // 生成文件名
        var dic = SecurityUtil.GetSecretKeyDic();
        string bucketName = dic["bucket"];
        string filePath = "Unity/GameData/" + fileName;  // COS 上的文件路径
        string localDir = Application.persistentDataPath;
        string localFilePath = Path.Combine(localDir, fileName);
        GetObjectRequest request = new GetObjectRequest(bucketName, filePath, localDir, fileName);
        try
        {
            GetObjectResult result = await Task.Run(() => cosXml.GetObject(request));
            if (result.httpCode == 200)
            {
                GameEntry.LogError($"文件 {fileName} 下载并保存到 {localFilePath} 成功！");
                byte[] fileContent = File.ReadAllBytes(localFilePath);
                return (DownloadStatus.Success, fileContent);
            }
            else if(result.httpCode == 404)
            {
                GameEntry.LogError($"文件 {fileName} 未找到");
                return (DownloadStatus.FileNotFound,null);
            }
            else
            {
                GameEntry.LogError($"{fileName} 下载失败，状态码：{result?.httpCode ?? -1}");
                return (DownloadStatus.UnknownError,null);
            }
        }
        catch (Exception ex)
        {
            GameEntry.LogError($"{fileName} 下载状态：<color=red>失败</color>，错误：{ex.Message}");
            if (ex.Message.Contains("No such file or directory"))
            {
                return (DownloadStatus.FileNotFound,null);
            }
            if (ex.Message.Contains("network"))
            {
                return (DownloadStatus.NetworkError,null);
            }
            return (DownloadStatus.UnknownError,null);
        }
    }

    static CosXml CreateCosXml()
    {
        var dic = SecurityUtil.GetSecretKeyDic();
        CosXmlConfig config = new CosXmlConfig.Builder()
            .SetConnectionTimeoutMs(60000)  //设置连接超时时间，单位毫秒，默认45000ms
            .SetReadWriteTimeoutMs(40000)  //设置读写超时时间，单位毫秒，默认45000ms
            .IsHttps(true)  //设置默认 HTTPS 请求
            .SetAppid("1318826377")
            .SetRegion(dic["region"])
            .Build();

        long durationSecond = 600; //每次请求签名有效时长，单位为秒
        QCloudCredentialProvider qCloudCredentialProvider = new DefaultQCloudCredentialProvider(dic["secretId"], dic["secretKey"], durationSecond);

        return new CosXmlServer(config, qCloudCredentialProvider);
    }

}

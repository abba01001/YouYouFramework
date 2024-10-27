using System;
using System.IO;
using System.Threading.Tasks;
using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using COSXML.Utils;
using Main;
using UnityEngine;
using YouYou;

public class SDKManager : Observable<SDKManager>
{
    public void Init()
    {

    }
    public void OnUpdate()
    {

    }
    
    public async Task UploadGameData(string localFilePath)
    {
        CosXml cosXml = CreateCosXml();

        string relativePath = Path.GetFileName(localFilePath); // 获取文件名
        using (FileStream fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var dic = SecurityUtil.GetSecretKeyDic();
            PutObjectRequest request = new PutObjectRequest(dic["bucket"], "Unity/GameData" + "/" + relativePath, fileStream);
            request.SetSign(TimeUtils.GetCurrentTime(TimeUnit.Seconds), 600);
            try
            {
                PutObjectResult result = await Task.Run(() => cosXml.PutObject(request));
            }
            catch (Exception ex)
            {
                GameEntry.LogError($"{relativePath}      上传状态：<color=red>失败</color>，错误：{ex.Message}");
            }
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

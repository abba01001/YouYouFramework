using System;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using COSXML.Utils;
using Main;
using MessagePack;
using MySql.Data.MySqlClient;
using UnityEngine;
using YouYou;

public class SDKManager : Observable<SDKManager>
{
    private MySqlConnection sqlConnection;
    public void Init()
    {

    }
    public void OnUpdate()
    {

    }

    #region 腾讯云COS
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
                GameEntry.LogError($"游戏数据 {fileName} 上传成功{result.IsSuccessful()}！");
            }
            catch (Exception ex)
            {
                GameEntry.LogError($"{fileName} 上传状态：<color=red>失败</color>，错误：{ex.Message}");
            }
        }
    }
    
    public async Task UploadLogData(string userId)
    {
        MainEntry.Reporter.WriteLogsToFile();

        string filePath = Path.Combine(Application.persistentDataPath, "Logs.txt");
        string zipFilePath = Path.Combine(Application.persistentDataPath, "Logs.txt.zip");
        GameUtil.CompressFile($"{filePath}", null, async () =>
        {
            // 直接使用文件路径打开文件流
            using (FileStream fileStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read))
            {
                // 创建COS客户端实例
                CosXml cosXml = CreateCosXml();
                string fileName = $"{userId}.txt"; // 文件扩展名改为txt
                var dic = SecurityUtil.GetSecretKeyDic();
                // 创建上传请求
                PutObjectRequest request = new PutObjectRequest(dic["bucket"], "Unity/LogData/" + fileName, fileStream);
                request.SetSign(TimeUtils.GetCurrentTime(TimeUnit.Seconds), 600);
                try
                {
                    // 异步上传文件
                    PutObjectResult result = await Task.Run(() => cosXml.PutObject(request));
                    GameEntry.LogError($"日志文件 {fileName} 上传成功！");
                }
                catch (Exception ex)
                {
                    GameEntry.LogError($"{fileName} 上传状态：<color=red>失败</color>，错误：{ex.Message}");
                }
            }
        });
    }

    public async void DownloadGameData(string userId)
    {
        CosXml cosXml = CreateCosXml();
        string fileName = $"{userId}.bin";
        string tempFileName = $"temp_{userId}.bin";
        var dic = SecurityUtil.GetSecretKeyDic();
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
                Constants.IsLoginGame = true;
                GameEntry.Event.Dispatch(Constants.EventName.LoginSuccess);
            }
            else
            {
                GameEntry.Data.InitGameData(null);
                Constants.IsLoginGame = true;
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
        CosXml cosXml = CreateCosXml();
        string fileName = $"{spriteId}.jpg"; // 头像文件名
        var dic = SecurityUtil.GetSecretKeyDic();
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

    static CosXml CreateCosXml()
    {
        var dic = SecurityUtil.GetSecretKeyDic();
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

    #region 腾讯云MySql
    // 初始化数据库连接
    public async Task InitSqlConnect()
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = SecurityUtil.GetSqlKeyDic()["Server"],
            UserID = SecurityUtil.GetSqlKeyDic()["User ID"],
            Password = SecurityUtil.GetSqlKeyDic()["Password"],
            Database = SecurityUtil.GetSqlKeyDic()["Database"],
            Port = UInt32.Parse(SecurityUtil.GetSqlKeyDic()["Port"]),
            CharacterSet = "utf8mb4"
        };

        sqlConnection = new MySqlConnection(builder.ToString());
        await sqlConnection.OpenAsync();
        await sqlConnection.CloseAsync();
        Debug.Log("数据库连接正常");
    }

    // 统一打开和关闭连接
    private async Task<T> ExecuteWithConnectionAsync<T>(Func<MySqlCommand, Task<T>> action,
        bool useLongConnection = false)
    {
        if (sqlConnection.State == ConnectionState.Closed)
            await sqlConnection.OpenAsync();

        await using var cmd = sqlConnection.CreateCommand();
        try
        {
            return await action(cmd);
        }
        finally
        {
            // 如果使用短连接，确保每次都关闭连接
            if (!useLongConnection && sqlConnection.State == ConnectionState.Open)
                await sqlConnection.CloseAsync();
        }
    }

    // 注册新账户
    public async Task RegisterAccountAsync(string accountId, string passWord)
    {
        if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(passWord))
        {
            Debug.LogError("请输入账号和密码");
            return;
        }

        if (await InsertAsync(accountId, passWord))
        {
            GameEntry.Data.IsFirstLoginTime = true;
            Constants.IsLoginGame = true;
            GameEntry.Log(LogCategory.NetWork,"注册用户成功");
            GameEntry.Event.Dispatch(Constants.EventName.LoginSuccess);
        }
        else
        {
            Debug.LogError("注册失败");
        }
    }

    // 登录
    public async Task LoginAsync(string account, string password)
    {
        account = "abc123";
        password = "abc123";
        if (await FindAsync(account))
        {
            var (isValid, uuid) = await ValidateUserAsync(account, password);
            if (isValid)
            {
                GameEntry.Data.UserId = uuid;
                DownloadGameData(GameEntry.Data.UserId);
            }
            else
            {
                GameUtil.LogError("密码错误！");
            }
        }
        else
        {
            await RegisterAccountAsync(account, password);
        }
    }

    // 创建数据表
    public async Task CreateDataTableAsync()
    {
        string query = "CREATE TABLE IF NOT EXISTS register_data (" +
                       "id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                       "user_uuid CHAR(36) NOT NULL UNIQUE, " +
                       "user_account VARCHAR(50) NOT NULL UNIQUE, " +
                       "user_password VARCHAR(88) NOT NULL) " +
                       "CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci";

        await ExecuteWithConnectionAsync(async cmd =>
        {
            cmd.CommandText = query;
            await cmd.ExecuteNonQueryAsync();
            return true;
        });
    }

    // 插入新用户数据
    private async Task<bool> InsertAsync(string user_account, string user_password)
    {
        user_password = SecurityUtil.ConvertBase64Key(user_password); // 确保密码是加密的
        string query =
            "INSERT INTO register_data (user_uuid, user_account, user_password) VALUES(@uuid, @account, @password)";

        return await ExecuteWithConnectionAsync(async cmd =>
        {
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@uuid", GameEntry.Data.UserId);
            cmd.Parameters.AddWithValue("@account", user_account);
            cmd.Parameters.AddWithValue("@password", user_password);
            int result = await cmd.ExecuteNonQueryAsync();
            return result > 0;
        });
    }

    // 查找用户是否存在
    private async Task<bool> FindAsync(string user_account)
    {
        string query = "SELECT COUNT(*) FROM register_data WHERE user_account = @value";

        return await ExecuteWithConnectionAsync(async cmd =>
        {
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@value", user_account);
            int result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return result > 0;
        });
    }
    
    //更新密码
    public async Task<bool> UpdateUserPasswordAsync(string userId, string newPassword)
    {
        string query = "UPDATE register_data SET user_password = @newPassword WHERE user_uuid = @userId";
        return await ExecuteWithConnectionAsync(async cmd =>
        {
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@newPassword", SecurityUtil.ConvertBase64Key(newPassword));
            cmd.Parameters.AddWithValue("@userId", userId);
            int result = await cmd.ExecuteNonQueryAsync();
            return result > 0;
        });
    }

    // 验证用户
    private async Task<(bool isValid, string user_uuid)> ValidateUserAsync(string user_account, string input_password)
    {
        string query = "SELECT user_uuid, user_password FROM register_data WHERE user_account = @account";
        return await ExecuteWithConnectionAsync(async cmd =>
        {
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@account", user_account);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    var user_uuid = reader["user_uuid"].ToString();
                    var stored_password = reader["user_password"].ToString();
                    return (stored_password == SecurityUtil.GetBase64Key(input_password), user_uuid);
                }
            }

            return (false, null); // 用户不存在或密码不匹配
        });
    }

    // 关闭数据库连接
    public async Task CloseConnectionAsync()
    {
        if (sqlConnection.State == ConnectionState.Open)
            await sqlConnection.CloseAsync();
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
            GameUtil.LogError(s);
        }));
    }
    #endregion

    #region TalkingData

    public void InitTalkingData()
    {
        TalkingDataSDK.BackgroundSessionEnabled();
        TalkingDataSDK.InitSDK(Constants.TalkingDataAppid, "101", "");

        //用户获得隐私授权后才能调用StartA()
        TalkingDataSDK.StartA();
        GameEntry.Log(LogCategory.NetWork,"初始化TalkingDataSDK完成");
    }

    #endregion
}

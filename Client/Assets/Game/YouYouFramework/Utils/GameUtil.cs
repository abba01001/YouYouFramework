using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using System.Text;

using Cysharp.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Main;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameUtil
{
    private static readonly System.Random _random = new System.Random();
    private static StringBuilder stringBuilder = new StringBuilder();

    public static int RandomRange(int minInclusive, int maxExclusive)
    {
        return _random.Next(minInclusive, maxExclusive);
    }

    /// <summary>
    /// 获取路径的最后名称
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetLastPathName(string path)
    {
        if (path.IndexOf('/') == -1)
        {
            return path;
        }

        return path.Substring(path.LastIndexOf('/') + 1);
    }
    
    public static async UniTask<GameObject> LoadPrefabClone(string prefabFullPath, Transform parent = null)
    {
        var operation = GameEntry.Loader.DefaultPackage.LoadAssetAsync(prefabFullPath);
        await operation.Task;
        GameObject obj = UnityEngine.Object.Instantiate(operation.AssetObject as GameObject, parent);
        AssetReleaseHandle.Add(operation, obj);
        return obj;
    }
    
    public static string GetRandomString(List<string> stringList)
    {
        // 通过 RandomRange 方法生成一个随机索引
        int randomIndex = RandomRange(0, stringList.Count);
        return stringList[randomIndex];
    }

    public static float RandomRange(float minInclusive, float maxExclusive)
    {
        int precision = 10000;
        System.Random random = new System.Random();
        float number = (float) random.NextDouble() * (maxExclusive - minInclusive) + minInclusive;
        number = Mathf.Round(number * precision) / precision;
        return number;
    }

    public static string TruncateText(Text textComponent, string inputText)
    {
        if (textComponent == null) return inputText;
        float maxWidth = textComponent.rectTransform.rect.width;
        textComponent.text = inputText;
        float textWidth = textComponent.preferredWidth;
        if (textWidth <= maxWidth)
        {
            return inputText;
        }
        stringBuilder.Clear();
        stringBuilder.Append(inputText);
        const int maxTries = 50;
        int attempts = 0;
        while (textComponent.preferredWidth > maxWidth && attempts < maxTries)
        {
            stringBuilder.Length -= 1;
            textComponent.text = stringBuilder.ToString() + "..";
            attempts++;
        }
        return stringBuilder.ToString() + "..";
    }
    
    //调整中心点
    public static void AdjustPivot(RectTransform rectTransform, Vector2 newPivot)
    {
        // 保存当前视觉位置
        Vector2 originalSize = rectTransform.rect.size;
        Vector2 originalPivot = rectTransform.pivot;
        Vector2 originalPosition = rectTransform.anchoredPosition;

        // 更新 pivot
        rectTransform.pivot = newPivot;

        // 计算新的 anchoredPosition
        Vector2 pivotDelta = newPivot - originalPivot;
        Vector2 sizeDelta = originalSize * pivotDelta;

        rectTransform.anchoredPosition = originalPosition + sizeDelta;
    }

    // 测试代码块的执行时间
    public static void TestTime(string tag, System.Action action = null)
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        action?.Invoke();
        stopwatch.Stop();
        float seconds = stopwatch.ElapsedTicks / (float) System.Diagnostics.Stopwatch.Frequency;
        LogError($"{tag} --- 消耗时间: {seconds:F3} 秒");
    }

    public static IEnumerator CheckKeys(Dictionary<(Key, Key?), Action> keyMappings)
    {
        while (true)
        {
            if (Keyboard.current != null)
            {
                foreach (var keyMapping in keyMappings)
                {
                    Key mainKey = keyMapping.Key.Item1;
                    Key? modifierKey = keyMapping.Key.Item2;
                    Action action = keyMapping.Value;

                    if (modifierKey.HasValue)
                    {
                        if (
                            Keyboard.current[modifierKey.Value].isPressed &&
                            Keyboard.current[mainKey].wasPressedThisFrame
                        )
                        {
                            action?.Invoke();
                        }
                    }
                    else
                    {
                        if (Keyboard.current[mainKey].wasPressedThisFrame)
                        {
                            action?.Invoke();
                        }
                    }
                }
            }

            yield return null;
        }
    }

    public static void CopyComponents(GameObject original, GameObject clone)
    {
        // 获取原物体上的所有组件
        Component[] components = original.GetComponents<Component>();
        foreach (Component component in components)
        {
            // 在克隆物体上添加相同类型的组件
            Component clonedComponent = clone.AddComponent(component.GetType());

            // 复制所有公共字段
            FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                field.SetValue(clonedComponent, field.GetValue(component));
            }

            // 复制所有公共属性
            PropertyInfo[] properties = component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite) // 确保属性可写
                {
                    property.SetValue(clonedComponent, property.GetValue(component));
                }
            }
        }
    }

    public static string GetModelPath(int modelId)
    {
        return $"Assets/Game/Download/Prefab/Model/{modelId}.prefab";
    }

    public static Vector3 ParseCoordinates(string coordinateString)
    {
        // 按照逗号分隔字符串并转换为坐标
        string[] parts = coordinateString.Split(',');

        // 转换为浮动类型的 X, Y, Z，并返回一个 Vector3 对象
        float x = float.Parse(parts[0]);
        float y = float.Parse(parts[1]);
        float z = float.Parse(parts[2]);
        return new Vector3(x, y, z);
    }
    
    public static List<int> ParseNumbers(string input)
    {
        List<int> parsedNumbers = new List<int>();
        if (string.IsNullOrEmpty(input)) return parsedNumbers;
        // 使用 Split() 分割字符串，如果没有逗号，结果依然是一个单一元素数组
        string[] numbers = input.Split(',');
        foreach (string num in numbers)
        {
            // 尝试解析每个数字
            if (int.TryParse(num, out int parsedNumber))
            {
                parsedNumbers.Add(parsedNumber);
            }
            else
            {
                LogError($"Invalid number: {num}");
            }
        }
        return parsedNumbers;
    }
    
    // 屏蔽渲染某个层
    public static void BlockSceneLayer(Camera camera,int layer)
    {
        if (camera != null)
        {
            camera.cullingMask &= ~(1 << layer);
        }
    }

    // 恢复渲染某个层
    public static void RestoreSceneLayer(Camera camera,int layer)
    {
        if (camera != null)
        {
            camera.cullingMask |= (1 << layer);
        }
    }

    public static void LoadPropSprite(Image icon)
    {
        float targetHeight = 100;
        float targetWidth = 80;
        float originalWidth = icon.sprite.rect.width;
        float originalHeight = icon.sprite.rect.height;
        float aspectRatio = originalWidth / originalHeight;
        float newWidth;
        float newHeight;
        if (originalHeight >= originalWidth)
        {
            newHeight = targetHeight;
            newWidth = targetHeight * aspectRatio;
        }
        else
        {
            newWidth = targetWidth;
            newHeight = targetWidth / aspectRatio;
        }

        icon.rectTransform.sizeDelta = new Vector2(newWidth, newHeight);
    }

    public static void LogCurTimerLog(string tag = "")
    {
        GameUtil.LogError($"{tag}====> {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
    }
    
    public static void LogError(params object[] messages)
    {
        string combinedMessage = StringUtil.JointString(messages);
        Debug.LogError(combinedMessage);
    }

    public static void Shuffle<T>(List<T> list)
    {
        int count = list.Count;
        for (int i = count - 1; i > 0; i--)
        {
            int r = _random.Next(0, i + 1);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
    
    public static string GetCosABRoot(string assetVersion)
    {
        RuntimePlatform platform = Application.platform;
        GameUtil.LogError(platform);
        string path = "";

        switch (platform)
        {
            case RuntimePlatform.WindowsPlayer:
                path = "/Unity/AssetBundle/" + assetVersion + "/" + "Windows" + "/";
                break;
            case RuntimePlatform.Android:
                path = "/Unity/AssetBundle/" + assetVersion + "/" + "Android" + "/";
                break;
            default:
                Debug.Log("未知平台");
                break;
        }

        return path;
    }

    #region 获取app md5值

    public static string GetSignatureMD5Hash()
    {
        var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
        var PackageManager = new AndroidJavaClass("android.content.pm.PackageManager");


        var packageName = activity.Call<string>("getPackageName");


        var GET_SIGNATURES = PackageManager.GetStatic<int>("GET_SIGNATURES");
        var packageManager = activity.Call<AndroidJavaObject>("getPackageManager");
        var packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, GET_SIGNATURES);
        var signatures = packageInfo.Get<AndroidJavaObject[]>("signatures");


        if (signatures != null && signatures.Length > 0)
        {
            byte[] bytes = signatures[0].Call<byte[]>("toByteArray");
            string str = getSignValidString(bytes);
            GameUtil.LogError("md5码====》", string.Format("<color=#ffffffff><---{0}-{1}----></color>", str, "test1"));
            return str;
        }

        return null;
    }

    private static String getSignValidString(byte[] paramArrayOfByte)
    {
        var MessageDigest = new AndroidJavaClass("java.security.MessageDigest");
        var localMessageDigest = MessageDigest.CallStatic<AndroidJavaObject>("getInstance", "MD5");

        localMessageDigest.Call("update", paramArrayOfByte);
        return toHexString(localMessageDigest.Call<byte[]>("digest"));
    }

    public static String toHexString(byte[] paramArrayOfByte)
    {
        if (paramArrayOfByte == null)
        {
            return null;
        }

        StringBuilder localStringBuilder = new StringBuilder(2 * paramArrayOfByte.Length);
        for (int i = 0;; i++)
        {
            if (i >= paramArrayOfByte.Length)
            {
                return localStringBuilder.ToString();
            }

            String str =
                new AndroidJavaClass("java.lang.Integer").CallStatic<String>("toString", 0xFF & paramArrayOfByte[i],
                    16);
            if (str.Length == 1)
            {
                str = "0" + str;
            }

            localStringBuilder.Append(str);
        }
    }

    #endregion

    // 压缩文件
    public static void CompressFile(string sourceFilePath,Action<float> onProgress,Action onComplete)
    {
        try
        {
            if (string.IsNullOrEmpty(sourceFilePath))
            {
                GameUtil.LogError("Please provide a valid file path before compressing.");
                return;
            }

            string sourceDirectoryName = Path.GetDirectoryName(sourceFilePath);
            string saveFilePath = Path.Combine(sourceDirectoryName, Path.GetFileName(sourceFilePath) + ".zip");

            // 创建目录
            Directory.CreateDirectory(sourceDirectoryName);

            long totalBytes = new FileInfo(sourceFilePath).Length;
            long bytesProcessed = 0;

            using (FileStream fsOut = File.Create(saveFilePath))
            {
                using (ZipOutputStream zipStream = new ZipOutputStream(fsOut))
                {
                    byte[] buffer = new byte[4096];
                    ZipEntry entry = new ZipEntry(Path.GetFileName(sourceFilePath));
                    zipStream.PutNextEntry(entry);

                    using (FileStream fsIn = File.OpenRead(sourceFilePath))
                    {
                        int sourceBytes;
                        while ((sourceBytes = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            zipStream.Write(buffer, 0, sourceBytes);
                            bytesProcessed += sourceBytes;
                            float progress = (float) bytesProcessed / totalBytes;
                            onProgress?.Invoke(progress); // 传递当前进度
                        }
                    }
                    zipStream.Finish();
                }
            }
            // 压缩完成，调用回调
            onComplete?.Invoke();
        }
        catch (Exception ex)
        {
            // 错误发生时调用错误回调
            GameUtil.LogError($"Error compressing file: {ex.Message}");
        }
    }

    // 解压文件方法（不需要进度回调，因为解压通常较小）
    public static void DecompressFile(string zipFilePath, Action<string> onComplete, Action<string> onError)
    {
        try
        {
            if (string.IsNullOrEmpty(zipFilePath))
            {
                onError?.Invoke("Please provide a valid zip file path before decompressing.");
                return;
            }

            string extractDirectory = Path.GetDirectoryName(zipFilePath);
            string outputDirectory = Path.Combine(extractDirectory, "Decompressed");

            // 创建解压目录
            Directory.CreateDirectory(outputDirectory);

            using (ZipInputStream zipStream = new ZipInputStream(File.OpenRead(zipFilePath)))
            {
                ZipEntry entry;
                while ((entry = zipStream.GetNextEntry()) != null)
                {
                    string fullPath = Path.Combine(outputDirectory, entry.Name);

                    if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    else
                    {
                        using (FileStream fsOut = File.Create(fullPath))
                        {
                            byte[] buffer = new byte[4096];
                            int sourceBytes;
                            while ((sourceBytes = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fsOut.Write(buffer, 0, sourceBytes);
                            }
                        }
                    }
                }
            }

            // 解压完成，调用回调
            onComplete?.Invoke(outputDirectory);
        }
        catch (Exception ex)
        {
            // 错误发生时调用错误回调
            onError?.Invoke($"Error decompressing file: {ex.Message}");
        }
    }

    public static Vector3 GetPosFromTrans(Transform startTrans, Transform targetTrans)
    {
        Vector3 buttonGWorldPosition = startTrans.GetComponent<RectTransform>().position;
        Vector3 buttonGInCanvasB = targetTrans.InverseTransformPoint(buttonGWorldPosition);
        return buttonGInCanvasB;
    }

    public static bool CheckFuncUnlock(string funcName)
    {
        foreach (var pair in GameEntry.DataTable.Sys_UnlockFuncDBModel.IdByDic)
        {
            if (pair.Value.FuncName == funcName)
            {
                return GameEntry.Data.RoleLevel >= pair.Value.UnlockLevel;
            }
        }
        GameUtil.LogError($"配置表未配置功能{funcName}");
        return false;
    }

    public static Sys_UnlockFuncEntity GetFuncEntity(string funcName)
    {
        foreach (var pair in GameEntry.DataTable.Sys_UnlockFuncDBModel.IdByDic)
        {
            if (pair.Value.FuncName == funcName)
            {
                return pair.Value;
            }
        }
        return null;
    }
    
    public static bool CheckFuncCanShow(string funcName)
    {
        foreach (var pair in GameEntry.DataTable.Sys_UnlockFuncDBModel.IdByDic)
        {
            if (pair.Value.FuncName == funcName)
            {
                return GameEntry.Data.RoleLevel >= pair.Value.ShowLevel;
            }
        }
        GameUtil.LogError($"配置表未配置功能{funcName}");
        return false;
    }
    
    
    public static void ShowUnlockTip(string btnType)
    {
        Sys_UnlockFuncEntity entity = GameUtil.GetFuncEntity(btnType);
        GameUtil.ShowTip($"达到{entity.UnlockLevel}级开放该功能~");
    }

    public static void SetBtnLock(GameObject obj,bool set)
    {
        if (obj == null) return;
        GameObject unlock = GameUtil.FindObjectByPath(obj.transform, "Lock");
        if (unlock != null)
        {
            unlock.gameObject.MSetActive(set);
        }
    }
    
    public static void SetBtnGray(GameObject obj,bool set)
    {
        if (obj == null) return;
        foreach (var t in obj.GetComponentsInChildren<Image>())
        {
            if (t.name == "Lock") continue;
            t.color = set ? Color.gray : Color.white;
        }
        foreach (var t in obj.GetComponentsInChildren<Text>())
        {
            if (t.name == "Lock") continue;
            t.color = set ? Color.gray : Color.white;
        }
    }

    public static GameObject FindObjectByPath(Transform parent, string path)
    {
        string[] parts = path.Split('/'); // 按 '/' 分割路径
        Transform currentTransform = parent;
        // 遍历路径的每一层
        foreach (string part in parts)
        {
            currentTransform = currentTransform.Find(part);

            if (currentTransform == null)
            {
                MainEntry.LogError(MainEntry.LogCategory.Framework,"找不到物体: " + part);
                return null;
            }
        }
        return currentTransform.gameObject;
    }
    
    public static Vector3 GetCenterPosFromTrans(Transform startTrans, Transform targetTrans)
    {
        RectTransform rectTransform = startTrans.GetComponent<RectTransform>();
        Vector3 localCenter = rectTransform.rect.center;
        Vector3 worldPosition = rectTransform.TransformPoint(localCenter);
        Vector3 localPositionInTarget = targetTrans.InverseTransformPoint(worldPosition);
        return localPositionInTarget;
    }

    public static void ShowTip(string text)
    {
        TipModel model = MainEntry.ClassObjectPool.Dequeue<TipModel>();
        model.text = text;
        GameEntry.UI.OpenUIForm<FormTip>("FormTip",model);
        MainEntry.ClassObjectPool.Enqueue(model);
    }
    
    public static IEnumerator LocationInfoCoroutine(Action<string> callback)
    {
        var publicIpReq = new UnityWebRequest(Constants.ProvinceUrl, UnityWebRequest.kHttpVerbGET);
        publicIpReq.downloadHandler = new DownloadHandlerBuffer();

        yield return publicIpReq.SendWebRequest();
        if (!string.IsNullOrEmpty(publicIpReq.error))
        {
            Debug.Log($"获取省份信息失败：{publicIpReq.error}");
            yield break;
        }
        var res = publicIpReq.downloadHandler.text;
        GameUtil.LogError(res);
    }
}

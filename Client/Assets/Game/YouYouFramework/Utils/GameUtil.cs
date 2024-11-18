using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using System.Text;
using YouYou;
using Cysharp.Threading.Tasks;
using Main;
using Unity.VisualScripting;
using UnityEngine.UI;

public class GameUtil
{
    private static readonly System.Random _random = new System.Random();
    
    /// <summary>
    /// 加载FBX嵌入的所有动画
    /// </summary>
    public static AnimationClip[] LoadInitRoleAnimationsByFBX(string path)
    {
#if EDITORLOAD && UNITY_EDITOR
        UnityEngine.Object[] objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
        List<AnimationClip> clips = new List<AnimationClip>();
        foreach (var item in objs)
        {
            if (item is AnimationClip) clips.Add(item as AnimationClip);
        }
        return clips.ToArray();
#else
        AssetInfoEntity m_CurrAssetEnity = GameEntry.Loader.AssetInfo.GetAssetEntity(path);
        AssetBundle bundle = GameEntry.Loader.LoadAssetBundle(m_CurrAssetEnity.AssetBundleFullPath);
        return bundle.LoadAllAssets<AnimationClip>();
#endif
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

    /// <summary>
    /// 加载Prefab并克隆
    /// </summary>
    public static GameObject LoadPrefabClone(string prefabFullPath, Transform parent = null)
    {
        AssetReferenceEntity referenceEntity = GameEntry.Loader.LoadMainAsset(prefabFullPath);
        if (referenceEntity != null)
        {
            GameObject obj = UnityEngine.Object.Instantiate(referenceEntity.Target as GameObject, parent);
            AutoReleaseHandle.Add(referenceEntity, obj);
            return obj;
        }
        return null;
    }
    public static async UniTask<GameObject> LoadPrefabCloneAsync(string prefabFullPath, Transform parent = null)
    {
        AssetReferenceEntity referenceEntity = await GameEntry.Loader.LoadMainAssetAsync(prefabFullPath);
        if (referenceEntity != null)
        {
            GameObject obj = UnityEngine.Object.Instantiate(referenceEntity.Target as GameObject, parent);
            AutoReleaseHandle.Add(referenceEntity, obj);
            return obj;
        }
        return null;
    }

    public static int RandomRange(int minInclusive, int maxExclusive)
    {
        return _random.Next(minInclusive, maxExclusive);
    }
    
    public static float RandomRange(float minInclusive, float maxExclusive)
    {
        int precision = 10000;
        System.Random random = new System.Random();
        float number = (float)random.NextDouble() * (maxExclusive - minInclusive) + minInclusive;
        number = Mathf.Round(number * precision) / precision;
        return number;
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
        float seconds = stopwatch.ElapsedTicks / (float)System.Diagnostics.Stopwatch.Frequency;
        LogError($"{tag} --- 消耗时间: {seconds:F3} 秒");
    }
    
    public static IEnumerator CheckKeys(Dictionary<(KeyCode, KeyCode?), Action> keyMappings)
    {
        while (true)
        {
            foreach (var keyMapping in keyMappings)
            {
                KeyCode mainKey = keyMapping.Key.Item1;
                KeyCode? modifierKey = keyMapping.Key.Item2; // 可选修饰键
                Action action = keyMapping.Value;
                if (modifierKey.HasValue)
                {
                    // 检查修饰键和功能键是否同时按下
                    if (Input.GetKey(modifierKey.Value) && Input.GetKeyDown(mainKey))
                    {
                        action?.Invoke();
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
    
    public static void PlayAnimation(Animator animator, string animName, int layer, Action endCall = null, bool disableAnimator = true)
    {
        animator.enabled = true;
        animator.Play(animName, layer, 0);
        GameEntry.Time.Yield(() =>
        {
            GameEntry.Time.CreateTimer(GameEntry.Instance.gameObject, TimeSpan.FromSeconds(animator.GetCurrentAnimatorStateInfo(0).length).Seconds, () =>
            {
                if (disableAnimator)
                {
                    animator.enabled = false;
                }
                endCall?.Invoke();
            });
        });
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
    
    public static void LogError(params object[] messages)
    {
        string combinedMessage = StringUtil.JointString(messages);
        Debug.LogError(combinedMessage);
    }

    public static string GetCosABRoot(string assetVersion)
    {
        RuntimePlatform platform = Application.platform;
        GameUtil.LogError(platform);
        string path = "";

        switch (platform)
        {
            case RuntimePlatform.WindowsPlayer:
                path = "/Unity/AssetBundle/"+ assetVersion+"/"+ "Windows"+ "/";
                break;
            case RuntimePlatform.Android:
                path = "/Unity/AssetBundle/" + assetVersion+"/" + "Android" + "/";
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
            GameUtil.LogError("md5码====》",string.Format("<color=#ffffffff><---{0}-{1}----></color>", str, "test1"));
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

}
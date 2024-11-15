using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Cysharp.Threading.Tasks;
using Main;
using UnityEngine.Networking;
using Sirenix.OdinInspector;

public class AssetVersionComparerWindow : EditorWindow
{
    private string localVersionFilePath;
    private string cloudVersionFilePath;

    // 本地资源版本字典
    private Dictionary<string, VersionFileEntity> m_LocalAssetsVersionDic = new Dictionary<string, VersionFileEntity>();

    // 云端资源版本字典
    private Dictionary<string, VersionFileEntity> m_CDNVersionDic = new Dictionary<string, VersionFileEntity>();

    // 当前是否正在加载云端资源
    private bool isLoadingCloudAssets = false;

    // 滚动视图的位置
    private Vector2 localScrollPos = Vector2.zero;
    private Vector2 cloudScrollPos = Vector2.zero;

    // 分页：本地和云端独立的当前页码
    private int currentPageLocal = 0;
    private int currentPageCloud = 0;

    [GUIColor(0.3f, 0.6f, 0.9f)] // 设置整个窗口的背景色
    [Title("资源版本对比工具", TitleAlignment = TitleAlignments.Centered, HorizontalLine = true)]
    public void OnEnable()
    {
        // 在OnEnable中初始化路径
        localVersionFilePath = $"{Application.persistentDataPath}/{YFConstDefine.VersionFileName}";
        cloudVersionFilePath =
            $"{SystemModel.Instance.CurrChannelConfig.EditorRealSourceUrl}{YFConstDefine.VersionFileName}";
        // 在打开窗口时，异步加载云端资源
        LoadAssetsAsync();
    }

    public async void LoadAssetsAsync()
    {
        // 标记正在加载
        isLoadingCloudAssets = true;
        Repaint();

        await GetCloudAssetFiles();

        // 加载完毕后刷新界面
        isLoadingCloudAssets = false;
        Repaint();
    }

    public async Task GetCloudAssetFiles()
    {
        m_CDNVersionDic.Clear();
        m_LocalAssetsVersionDic.Clear();
        StringBuilder sbr = StringHelper.PoolNew();
        string url = sbr.AppendFormatNoGC(cloudVersionFilePath).ToString();

        try
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = 2;
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    m_CDNVersionDic = GetAssetBundleVersionList(request.downloadHandler.data);
                    // 排序字典
                    m_CDNVersionDic = m_CDNVersionDic.OrderBy(kvp => kvp.Value.AssetBundleName)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    m_CDNVersionDic = m_CDNVersionDic
                        .Where(kvp =>
                            kvp.Key != "Android" && kvp.Key != "AssetInfo.bytes" && kvp.Key != "AssetInfo.json")
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }
        }
        catch (Exception ex)
        {
            // 捕获异常并打印错误信息
            GameUtil.LogError($"获取云端版本文件时发生异常: {ex.Message}");
        }
        
        // 本地文件存在时读取
        if (File.Exists(localVersionFilePath))
        {
            m_LocalAssetsVersionDic = GetAssetBundleVersionList();
            m_LocalAssetsVersionDic = m_LocalAssetsVersionDic.OrderBy(kvp => kvp.Value.AssetBundleName)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }

    // 处理前缀替换的方法
    private string ReplacePrefixWithEllipsis(string fileName)
    {
        const string prefix = "game/download/";
        if (fileName.StartsWith(prefix))
        {
            return "..." + fileName.Substring(prefix.Length); // 替换前缀为省略号
        }

        return fileName; // 没有匹配前缀则返回原文件名
    }


    private static Dictionary<string, VersionFileEntity> GetAssetBundleVersionList(byte[] buffer)
    {
        buffer = ZlibHelper.DeCompressBytes(buffer);
        Dictionary<string, VersionFileEntity> dic = new Dictionary<string, VersionFileEntity>();
        MMO_MemoryStream ms = new MMO_MemoryStream(buffer);
        int len = ms.ReadInt();

        for (int i = 0; i < len; i++)
        {
            if (i == 0)
            {
                ms.ReadUTF8String().Trim(); // 跳过第一个元素（假设是备注信息）
            }
            else
            {
                VersionFileEntity entity = new VersionFileEntity
                {
                    AssetBundleName = ms.ReadUTF8String(),
                    MD5 = ms.ReadUTF8String(),
                    Size = ms.ReadULong(),
                    IsFirstData = ms.ReadByte() == 1,
                    IsEncrypt = ms.ReadByte() == 1
                };

                dic[entity.AssetBundleName] = entity;
            }
        }

        return dic;
    }

    private Dictionary<string, VersionFileEntity> GetAssetBundleVersionList()
    {
        string json = IOUtil.GetFileText(localVersionFilePath);
        return json.ToObject<Dictionary<string, VersionFileEntity>>();
    }

    [GUIColor(0.7f, 0.7f, 0.7f)]
    [Title("资源对比", TitleAlignment = TitleAlignments.Centered)]
    [TabGroup("资源对比", "本地资源", Order = 0)]
    [TabGroup("资源对比", "云端资源", Order = 1)]
    private void OnGUI()
    {
        // 显示加载状态
        if (isLoadingCloudAssets)
        {
            GUILayout.Label("正在加载云端资源...", EditorStyles.boldLabel);
        }

        // 创建一个水平布局，左右两边
        EditorGUILayout.BeginHorizontal();

        // 左边显示本地资源列表
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2));
        GUILayout.Label("本地资源列表", EditorStyles.boldLabel);

        // 使用滚动视图显示本地资源
        localScrollPos = EditorGUILayout.BeginScrollView(localScrollPos, GUILayout.ExpandHeight(true));
        DisplayResourceList(m_LocalAssetsVersionDic, "local");
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

        // 右边显示云端资源列表
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2));
        GUILayout.Label("云端资源列表", EditorStyles.boldLabel);

        // 使用滚动视图显示云端资源
        cloudScrollPos = EditorGUILayout.BeginScrollView(cloudScrollPos, GUILayout.ExpandHeight(true));
        DisplayResourceList(m_CDNVersionDic, "cloud");
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

// 优化过的资源列表显示方法
    private void DisplayResourceList(Dictionary<string, VersionFileEntity> versionDic, string type)
    {
        if (versionDic == null || versionDic.Count == 0)
        {
            GUILayout.Label(type == "local" ? "本地没有资源数据" : "云端未检测到资源数据");
            return;
        }

        // 设置每列的固定宽度
        float maxFileNameWidth = 300f; // 文件名最大宽度
        float md5Width = 100f; // MD5列宽度，调整为100
        float fileSizeWidth = 100f; // 文件大小列宽度
        float isFirstDataWidth = 60f; // 是否是初始数据列宽度
        float isEncryptWidth = 60f; // 是否加密列宽度
        float statusWidth = 100f; // 状态列宽度

        // 计算文件名列宽度，基于文件名的实际宽度，但不超过最大宽度
        float calculatedFileNameWidth = 0f;
        foreach (var kvp in versionDic)
        {
            float width = GUI.skin.label.CalcSize(new GUIContent(kvp.Value.AssetBundleName)).x + 20f; // 增加一些padding
            calculatedFileNameWidth = Mathf.Max(calculatedFileNameWidth, width);
        }

        float fileNameWidth = Mathf.Min(calculatedFileNameWidth, maxFileNameWidth); // 如果宽度超出最大限制则使用最大值

        // 显示表头
        if (versionDic == m_LocalAssetsVersionDic || versionDic == m_CDNVersionDic)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("文件名", GUILayout.Width(fileNameWidth));
            GUILayout.Label("MD5", GUILayout.Width(md5Width));
            GUILayout.Label("文件大小", GUILayout.Width(fileSizeWidth));
            GUILayout.Label("初始数据", GUILayout.Width(isFirstDataWidth));
            GUILayout.Label("是否加密", GUILayout.Width(isEncryptWidth));
            if (versionDic == m_LocalAssetsVersionDic)
            {
                GUILayout.Label("状态", GUILayout.Width(statusWidth));
            }
            EditorGUILayout.EndHorizontal();
        }

        // 分页：每页显示一定数量的项
        int itemsPerPage = 35;
        var currentPageItems = versionDic.Skip(itemsPerPage * (type == "local" ? currentPageLocal : currentPageCloud))
            .Take(itemsPerPage);

        foreach (var kvp in currentPageItems)
        {
            var entity = kvp.Value;
            EditorGUILayout.BeginHorizontal();
            // 文件名列
            GUILayout.Label(TruncateString(ReplacePrefixWithEllipsis(entity.AssetBundleName), maxFileNameWidth),GUILayout.Width(fileNameWidth));
            GUILayout.Label(entity.MD5, GUILayout.Width(md5Width));
            GUILayout.Label(FormatFileSize(entity.Size), GUILayout.Width(fileSizeWidth));
            GUILayout.Label(entity.IsFirstData ? "是" : "否", GUILayout.Width(isFirstDataWidth));
            GUILayout.Label(entity.IsEncrypt ? "是" : "否", GUILayout.Width(isEncryptWidth));

            // 使用自定义的statusStyle来显示状态，并根据状态颜色变化
            if (type == "local")
            {
                string status = GetResourceStatus(entity, kvp.Key);
                // 如果是本地资源且状态为"旧资源"或"废弃资源"，则字体颜色设置为红色
                GUIStyle statusStyle = new GUIStyle(GUI.skin.label);
                if (type == "local" && (status == "旧资源" || status == "废弃资源" || status == "未知状态"))
                {
                    statusStyle.normal.textColor = Color.red; // 设置字体为红色
                }
                
                GUILayout.Label(status, statusStyle, GUILayout.Width(statusWidth));
            }

            EditorGUILayout.EndHorizontal();
        }

        // 分页控件
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("上一页") &&
            ((type == "local" && currentPageLocal > 0) || (type == "cloud" && currentPageCloud > 0)))
        {
            if (type == "local")
                currentPageLocal--;
            else
                currentPageCloud--;
        }

        if (GUILayout.Button("下一页") && ((type == "local" && (currentPageLocal + 1) * itemsPerPage < versionDic.Count) ||
                                        (type == "cloud" && (currentPageCloud + 1) * itemsPerPage < versionDic.Count)))
        {
            if (type == "local")
                currentPageLocal++;
            else
                currentPageCloud++;
        }

        EditorGUILayout.EndHorizontal();
    }

// 判断本地资源的状态
    private string GetResourceStatus(VersionFileEntity localEntity, string fileName)
    {
        if (m_CDNVersionDic.Count == 0)
        {
            return "未知状态";
        }
        if (m_CDNVersionDic.ContainsKey(fileName))
        {
            // 云端资源存在，检查MD5值
            var cloudEntity = m_CDNVersionDic[fileName];
            if (localEntity.MD5 != cloudEntity.MD5)
            {
                return "旧资源"; // 本地MD5与云端MD5不一致，视为旧资源
            }
            else
            {
                return "正常资源"; // MD5一致
            }
        }
        else
        {
            return "废弃资源"; // 云端没有此资源，视为废弃资源
        }
    }


    // 用于截断字符串并在超出时显示省略号
    private string TruncateString(string text, float maxWidth)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        float textWidth = style.CalcSize(new GUIContent(text)).x;

        // 如果文本宽度超过最大宽度，截断并加上省略号
        if (textWidth > maxWidth)
        {
            string truncatedText = text;
            while (style.CalcSize(new GUIContent(truncatedText + "...")).x > maxWidth)
            {
                truncatedText = truncatedText.Substring(0, truncatedText.Length - 1);
            }

            return truncatedText + "...";
        }

        return text; // 如果不需要截断，直接返回文本
    }

    // 格式化文件大小，保留2位小数，自动选择适合的单位
    private string FormatFileSize(ulong size)
    {
        if (size >= 1024 * 1024 * 1024) // GB
            return $"{size / (1024.0 * 1024 * 1024):0.00} GB";
        else if (size >= 1024 * 1024) // MB
            return $"{size / (1024.0 * 1024):0.00} MB";
        else if (size >= 1024) // KB
            return $"{size / 1024.0:0.00} KB";
        else // B
            return $"{size} B";
    }

    // 打开窗口
    [MenuItem("Tools/资源版本对比工具")]
    public static void ShowWindow()
    {
        AssetVersionComparerWindow window = GetWindow<AssetVersionComparerWindow>("资源版本对比工具");
        window.minSize = new Vector2(600, 200);
        window.Show();
    }
}
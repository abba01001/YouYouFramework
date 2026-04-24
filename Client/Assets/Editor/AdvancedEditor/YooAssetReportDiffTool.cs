#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

// 🔥 新增：平台枚举（替代原来的硬编码字符串）
public enum PlatformType
{
    Android,
    Windows,
    WebGL
}

[System.Serializable]
public class YooAssetReport
{
    public Summary Summary;
    public List<AssetInfo> AssetInfos;
}

[System.Serializable]
public class Summary
{
    public string BuildPackageVersion;
    public int AssetFileTotalCount;
    public long AllBundleSize;
}

[System.Serializable]
public class AssetInfo
{
    public string AssetPath;
    public string MainBundleName;
    public long MainBundleSize;
}

public class YooAssetReportDiffTool : EditorWindow
{
    // ************************ 核心配置 ************************
    private readonly string LOCAL_REPORT_FOLDER = "D:\\Study\\MyFramework\\Client\\TempProjectData\\";
    private const string SERVER_BASE_URL = "http://storage.abba01001.cn/private_files/ServerBundles";
    
    // 🔥 替换：枚举选择平台（默认Android）
    private PlatformType _platform = PlatformType.Android;
    
    private const float BUTTON_WIDTH = 400f;
    // **********************************************************

    private string _localReportPath;
    private string _serverReportText;
    private string _serverReportFileName;
    private bool _isLoading = false;
    private float _downloadProgress = 0f;

    private CancellationTokenSource _cts;

    private Vector2 _leftScroll;
    private Vector2 _rightScroll;
    private Vector2 _modifyScroll;

    private List<AssetInfo> _localOnly = new();
    private List<AssetInfo> _serverOnly = new();
    private List<AssetInfo> _modified = new();
    private int _sameCount;

    public static void OpenWindow()
    {
        var window = GetWindow<YooAssetReportDiffTool>("YooAsset 资源差异对比");
        window.minSize = new Vector2(800, 850);
        window.LoadLocalReportAuto();
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _isLoading = false;
        _downloadProgress = 0f;
    }

    private void LoadLocalReportAuto()
    {
        _localReportPath = GetLatestReportFile(LOCAL_REPORT_FOLDER);
        if (string.IsNullOrEmpty(_localReportPath))
            EditorUtility.DisplayDialog("警告", "未找到本地Report文件！", "确定");
    }

    private string GetLatestReportFile(string folder)
    {
        try
        {
            if (!Directory.Exists(folder)) return null;
            var files = Directory.GetFiles(folder, "*.report")
                .Where(f => Path.GetFileName(f).StartsWith("DefaultPackage_"))
                .ToList();
            return files.Count == 0 ? null : files.OrderByDescending(f => File.GetLastWriteTime(f)).First();
        }
        catch { return null; }
    }

    private async void GetServerReportByVersionFile()
    {
        if (_isLoading) return;

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _isLoading = true;
        _downloadProgress = 0f;
        Repaint();

        try
        {
            string version = Application.version;
            // 🔥 枚举转字符串，自动匹配平台路径
            string platformStr = _platform.ToString();
            string versionFileUrl = $"{SERVER_BASE_URL}/{platformStr}/{version}/DefaultPackage.version";

            using (UnityWebRequest reqVersion = UnityWebRequest.Get(versionFileUrl))
            {
                reqVersion.SetRequestHeader("User-Agent", "Mozilla/5.0");
                reqVersion.timeout = 0;
                var op = reqVersion.SendWebRequest();

                while (!op.isDone)
                {
                    if (token.IsCancellationRequested) throw new OperationCanceledException();
                    _downloadProgress = 0.1f;
                    Repaint();
                    await System.Threading.Tasks.Task.Yield();
                }

                if (reqVersion.result != UnityWebRequest.Result.Success)
                {
                    EditorUtility.DisplayDialog("错误", "获取时间戳失败！", "确定");
                    goto EndDownload;
                }

                string versionStamp = reqVersion.downloadHandler.text.Trim();
                _serverReportFileName = $"DefaultPackage_{versionStamp}.report";
                string reportUrl = $"{SERVER_BASE_URL}/{platformStr}/{version}/{_serverReportFileName}";

                using (UnityWebRequest reqReport = UnityWebRequest.Get(reportUrl))
                {
                    reqReport.SetRequestHeader("User-Agent", "Mozilla/5.0");
                    reqReport.timeout = 0;
                    var reportOp = reqReport.SendWebRequest();

                    while (!reportOp.isDone)
                    {
                        if (token.IsCancellationRequested) throw new OperationCanceledException();
                        _downloadProgress = 0.1f + reqReport.downloadProgress * 0.9f;
                        Repaint();
                        await System.Threading.Tasks.Task.Yield();
                    }

                    if (reqReport.result != UnityWebRequest.Result.Success)
                    {
                        EditorUtility.DisplayDialog("错误", "下载Report失败！", "确定");
                    }
                    else
                    {
                        _serverReportText = reqReport.downloadHandler.text;
                        _downloadProgress = 1f;
                        Repaint();
                        EditorUtility.DisplayDialog("成功", $"已下载：\n{_serverReportFileName}", "确定");
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("✅ 下载已取消（窗口关闭）");
        }
        catch
        {
            EditorUtility.DisplayDialog("错误", "网络请求异常！", "确定");
        }

    EndDownload:
        _isLoading = false;
        _downloadProgress = 0f;
        _cts?.Dispose();
        _cts = null;
        Repaint();
    }

    private string GetTotalSizeMB(List<AssetInfo> list)
    {
        if (list == null || list.Count == 0) return "0.00";
        long totalBytes = list.Sum(item => item.MainBundleSize);
        double totalMB = totalBytes / (1024d * 1024d);
        return totalMB.ToString("F2");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);
        var titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
        EditorGUILayout.LabelField("YooAsset 资源差异对比", titleStyle);
        EditorGUILayout.Space(5);
        DrawLine(Color.gray);
        EditorGUILayout.Space(8);

        // 🔥 新增：平台枚举选择框
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("选择平台：", GUILayout.Width(80));
        _platform = (PlatformType)EditorGUILayout.EnumPopup(_platform, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(8);

        DrawFileStatus();
        EditorGUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        if (_isLoading)
        {
            Rect progressRect = EditorGUILayout.GetControlRect(GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(32));
            EditorGUI.ProgressBar(progressRect, _downloadProgress, $"⏳ 下载中 {(_downloadProgress*100):F0}%");
        }
        else
        {
            if (GUILayout.Button("🔍 获取服务端文件", GUILayout.Width(BUTTON_WIDTH), GUILayout.Height(32)))
                GetServerReportByVersionFile();
        }

        if (GUILayout.Button("🔍 执行差异分析", GUILayout.Height(32)))
            AnalyzeDiff();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        DrawLine(Color.gray);
        EditorGUILayout.Space(8);
        DrawTotalInfo();
        EditorGUILayout.Space(8);
        DrawResultPanel();
        EditorGUILayout.Space(8);
        DrawModifyPanel();
    }

    #region UI绘制
    private void DrawLine(Color c)
    {
        Rect r = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(r, c);
    }

    private void DrawFileStatus()
    {
        EditorGUILayout.BeginHorizontal();
        float w = (position.width - 20) / 2;

        EditorGUILayout.BeginVertical("GroupBox", GUILayout.Width(w));
        EditorGUILayout.LabelField("本地 Report", EditorStyles.boldLabel);
        string localName = string.IsNullOrEmpty(_localReportPath) ? "未找到" : Path.GetFileName(_localReportPath);
        EditorGUILayout.LabelField("✅ " + localName);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("GroupBox", GUILayout.Width(w));
        EditorGUILayout.LabelField("服务端 Report", EditorStyles.boldLabel);
        string tip = string.IsNullOrEmpty(_serverReportText) ? $"⌛ {_platform} {Application.version}" : $"✅ {_serverReportFileName}";
        EditorGUILayout.LabelField(tip);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawTotalInfo()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"📊 完全一致：{_sameCount} 个", GUILayout.Width(180));
        GUILayout.Label($"🆕 本地独有：{_localOnly.Count} 个", GUILayout.Width(180));
        GUILayout.Label($"❌ 服务端独有：{_serverOnly.Count} 个", GUILayout.Width(180));
        GUILayout.Label($"🔄 资源变更：{_modified.Count} 个", GUILayout.Width(180));
        EditorGUILayout.EndHorizontal();
    }

    private void DrawResultPanel()
    {
        EditorGUILayout.BeginHorizontal();
        float w = (position.width - 20) / 2;

        EditorGUILayout.BeginVertical("GroupBox", GUILayout.Width(w));
        EditorGUILayout.LabelField($"🟦 本地独有（服务端缺失）| 总计：{GetTotalSizeMB(_localOnly)} MB", EditorStyles.boldLabel);
        DrawLine(Color.cyan);
        _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll, GUILayout.Height(380));
        foreach (var a in _localOnly)
            EditorGUILayout.LabelField($"• {a.AssetPath}    [{FormatSize(a.MainBundleSize)}]");
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("GroupBox", GUILayout.Width(w));
        EditorGUILayout.LabelField($"🟥 服务端独有（本地缺失）| 总计：{GetTotalSizeMB(_serverOnly)} MB", EditorStyles.boldLabel);
        DrawLine(Color.magenta);
        _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll, GUILayout.Height(380));
        foreach (var a in _serverOnly)
            EditorGUILayout.LabelField($"• {a.AssetPath}    [{FormatSize(a.MainBundleSize)}]");
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawModifyPanel()
    {
        EditorGUILayout.BeginVertical("GroupBox");
        EditorGUILayout.LabelField($"🟨 版本不一致（资源变更）| 总计：{GetTotalSizeMB(_modified)} MB", EditorStyles.boldLabel);
        DrawLine(Color.yellow);
        _modifyScroll = EditorGUILayout.BeginScrollView(_modifyScroll, GUILayout.Height(160));
        foreach (var a in _modified)
            EditorGUILayout.LabelField($"• {a.AssetPath}    [{FormatSize(a.MainBundleSize)}]");
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    #endregion

    #region 核心逻辑
    private string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes}B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024f:F2}KB";
        return $"{bytes / (1024f * 1024f):F2}MB";
    }

    private string ReadLocalText()
    {
        if (string.IsNullOrEmpty(_localReportPath) || !File.Exists(_localReportPath)) return null;
        try { return File.ReadAllText(_localReportPath); }
        catch { return null; }
    }

    private void AnalyzeDiff()
    {
        _localOnly.Clear();
        _serverOnly.Clear();
        _modified.Clear();
        _sameCount = 0;

        string localText = ReadLocalText();
        if (string.IsNullOrEmpty(localText)) { EditorUtility.DisplayDialog("错误", "本地report为空！", "确定"); return; }
        if (string.IsNullOrEmpty(_serverReportText)) { EditorUtility.DisplayDialog("错误", "请先获取服务端文件！", "确定"); return; }

        var local = JsonUtility.FromJson<YooAssetReport>(localText);
        var server = JsonUtility.FromJson<YooAssetReport>(_serverReportText);

        var localDict = local.AssetInfos.ToDictionary(x => x.AssetPath);
        var serverDict = server.AssetInfos.ToDictionary(x => x.AssetPath);

        foreach (var p in localDict) if (!serverDict.ContainsKey(p.Key)) _localOnly.Add(p.Value);
        foreach (var p in serverDict) if (!localDict.ContainsKey(p.Key)) _serverOnly.Add(p.Value);

        foreach (var (path, li) in localDict)
        {
            if (serverDict.TryGetValue(path, out var si))
            {
                if (li.MainBundleName != si.MainBundleName || li.MainBundleSize != si.MainBundleSize)
                    _modified.Add(li);
                else _sameCount++;
            }
        }

        EditorUtility.DisplayDialog("分析完成",
            $"本地：{local.Summary.BuildPackageVersion}\n服务端：{server.Summary.BuildPackageVersion}\n\n" +
            $"一致：{_sameCount} | 本地新增：{_localOnly.Count} | 服务端新增：{_serverOnly.Count} | 变更：{_modified.Count}", "确定");
    }
    #endregion
}
#endif
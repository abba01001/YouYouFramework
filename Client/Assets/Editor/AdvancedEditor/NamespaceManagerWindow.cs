using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class NamespaceManagerWindow : EditorWindow
{
    // --- 数据持久化结构 ---
    [Serializable]
    private class BackupData { public List<BackupItem> History = new List<BackupItem>(); }

    [Serializable]
    private class BackupItem
    {
        public string TimeStamp;
        public string TargetNamespace;
        public List<FileRecord> Records = new List<FileRecord>();
        [NonSerialized] public bool IsExpanded; // 仅用于 UI 展开状态
    }

    [Serializable]
    private class FileRecord
    {
        public string ClassName;
        public string RelativePath;
        public string OriginalNamespace;
    }

    private class ClassSelection
    {
        public string Name;
        public string Path;
        public bool Selected;
    }

    // --- 变量 ---
    private string targetNamespace = "Global";
    private string addNamespaceInput = "NewNamespace";
    private List<ClassSelection> scanResults = new List<ClassSelection>();
    private BackupData backupData = new BackupData();
    private Vector2 scrollPos;
    private int tabIndex = 0;
    private string[] tabs = { "移除域名", "恢复历史 (撤销)", "添加域名" };

    private readonly string backupPath = "Temp/NamespaceBackup.json";

    public static void ShowWindow() => GetWindow<NamespaceManagerWindow>("域名管理器");

    private void OnEnable() { LoadBackup(); }

    private void OnGUI()
    {
        tabIndex = GUILayout.Toolbar(tabIndex, tabs);
        EditorGUILayout.Space(10);

        switch (tabIndex)
        {
            case 0: DrawRemoveTab(); break;
            case 1: DrawRestoreTab(); break;
            case 2: DrawAddTab(); break;
        }
    }

    #region Tab 0: 移除域名
    private void DrawRemoveTab()
    {
        GUILayout.Label("扫描并移除指定的 Namespace", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        targetNamespace = EditorGUILayout.TextField("目标 Namespace", targetNamespace);
        if (GUILayout.Button("扫描项目", GUILayout.Width(80))) ScanClasses(true);
        EditorGUILayout.EndHorizontal();

        DrawClassList("执行移除并备份", ExecuteRemoval);
    }
    #endregion

    #region Tab 1: 恢复历史 (修复布局报错版)
    private void DrawRestoreTab()
    {
        GUILayout.Label("历史操作记录 (恢复后自动删除记录)", EditorStyles.boldLabel);
        if (backupData.History.Count == 0)
        {
            GUILayout.Label("无备份记录", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        // 核心修复：记录待操作的任务，在布局结束后执行
        int indexToRestore = -1;
        int indexToRemove = -1;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        for (int i = backupData.History.Count - 1; i >= 0; i--)
        {
            var item = backupData.History[i];
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            // 使用 Foldout 展开详情
            item.IsExpanded = EditorGUILayout.Foldout(item.IsExpanded, $"[{item.TimeStamp}] 域名: {item.TargetNamespace} ({item.Records.Count}个)", true);
            
            if (GUILayout.Button("恢复并删除", GUILayout.Width(100))) 
            {
                indexToRestore = i;
            }
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                indexToRemove = i;
            }
            EditorGUILayout.EndHorizontal();

            if (item.IsExpanded)
            {
                EditorGUI.indentLevel++;
                foreach (var rec in item.Records)
                {
                    EditorGUILayout.LabelField($"• {rec.ClassName}", rec.RelativePath, EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
        EditorGUILayout.EndScrollView();

        // --- 核心修复：在 EndScrollView 之后，下一帧渲染前执行逻辑 ---
        if (indexToRestore != -1)
        {
            RestoreVersion(backupData.History[indexToRestore], indexToRestore);
            GUIUtility.ExitGUI(); // 强制退出当前 GUI 渲染循环，防止布局不一致
        }

        if (indexToRemove != -1)
        {
            backupData.History.RemoveAt(indexToRemove);
            SaveBackup();
            GUIUtility.ExitGUI();
        }
    }
    #endregion

    #region Tab 2: 添加域名
    private void DrawAddTab()
    {
        GUILayout.Label("扫描完全没有 Namespace 的类", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        addNamespaceInput = EditorGUILayout.TextField("欲添加的 Namespace", addNamespaceInput);
        if (GUILayout.Button("扫描无域名类", GUILayout.Width(100))) ScanClasses(false);
        EditorGUILayout.EndHorizontal();

        DrawClassList("批量添加 Namespace", ExecuteAddNamespace);
    }
    #endregion

    // --- 核心逻辑 ---

    private void DrawClassList(string actionName, Action action)
    {
        if (scanResults.Count == 0) return;
        EditorGUILayout.Space(5);
        if (GUILayout.Button("全选/反选", GUILayout.Width(100)))
        {
            bool toggle = scanResults.Any(c => !c.Selected);
            scanResults.ForEach(c => c.Selected = toggle);
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox);
        foreach (var info in scanResults)
        {
            EditorGUILayout.BeginHorizontal();
            info.Selected = EditorGUILayout.Toggle(info.Selected, GUILayout.Width(20));
            EditorGUILayout.LabelField(info.Name, GUILayout.Width(180));
            EditorGUILayout.LabelField(info.Path, EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button(actionName, GUILayout.Height(30))) action();
    }

    private void ScanClasses(bool searchNamespace)
    {
        scanResults.Clear();
        string[] allScripts = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        string targetPattern = $@"namespace\s+{Regex.Escape(targetNamespace)}\s*\{{";

        foreach (string path in allScripts)
        {
            string content = File.ReadAllText(path);
            bool hasAnyNS = Regex.IsMatch(content, @"namespace\s+[\w\.]+");

            if (searchNamespace)
            {
                if (Regex.IsMatch(content, targetPattern)) AddToScanResult(path);
            }
            else if (!hasAnyNS && (content.Contains("class ") || content.Contains("struct ")))
            {
                AddToScanResult(path);
            }
        }
    }

    private void AddToScanResult(string path)
    {
        scanResults.Add(new ClassSelection {
            Name = Path.GetFileNameWithoutExtension(path),
            Path = path.Replace(Application.dataPath, "Assets"),
            Selected = true
        });
    }

    private void ExecuteRemoval()
    {
        BackupItem session = new BackupItem {
            TimeStamp = DateTime.Now.ToString("MM-dd HH:mm:ss"),
            TargetNamespace = targetNamespace
        };

        int count = 0;
        foreach (var info in scanResults.Where(c => c.Selected))
        {
            string fullPath = Path.GetFullPath(info.Path);
            string content = File.ReadAllText(fullPath);
            string pattern = $@"namespace\s+{Regex.Escape(targetNamespace)}\s*\{{(?<s>.*)\}}";
            Match match = Regex.Match(content, pattern, RegexOptions.Singleline | RegexOptions.RightToLeft);

            if (match.Success)
            {
                session.Records.Add(new FileRecord { 
                    ClassName = info.Name, 
                    RelativePath = info.Path, 
                    OriginalNamespace = targetNamespace 
                });
                
                string body = match.Groups["s"].Value;
                string[] lines = body.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("    ")) lines[i] = lines[i].Substring(4);
                    else if (lines[i].StartsWith("\t")) lines[i] = lines[i].Substring(1);
                }
                
                string head = content.Substring(0, match.Index).TrimEnd();
                File.WriteAllText(fullPath, head + "\n\n" + string.Join("\n", lines).Trim() + "\n");
                count++;
            }
        }

        if (count > 0)
        {
            backupData.History.Add(session);
            SaveBackup();
        }
        FinishAction(count);
    }

    private void RestoreVersion(BackupItem item, int index)
    {
        int count = 0;
        foreach (var record in item.Records)
        {
            string fullPath = Path.GetFullPath(record.RelativePath);
            if (!File.Exists(fullPath)) continue;

            string content = File.ReadAllText(fullPath);
            if (!Regex.IsMatch(content, @"namespace\s+"))
            {
                Match firstClass = Regex.Match(content, @"(public|internal|partial|class|struct|interface)");
                if (firstClass.Success)
                {
                    string usings = content.Substring(0, firstClass.Index).Trim();
                    string body = content.Substring(firstClass.Index).Trim();
                    string indentedBody = "    " + body.Replace("\n", "\n    ");

                    StringBuilder sb = new StringBuilder();
                    if (!string.IsNullOrEmpty(usings)) sb.AppendLine(usings).AppendLine();
                    sb.AppendLine($"namespace {record.OriginalNamespace}\n{{");
                    sb.AppendLine(indentedBody);
                    sb.Append("}");
                    File.WriteAllText(fullPath, sb.ToString());
                    count++;
                }
            }
        }
        
        // 恢复后移除记录
        backupData.History.RemoveAt(index);
        SaveBackup();
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("恢复成功", $"已将 {count} 个类还原至域名 {item.TargetNamespace}，并清理了历史记录。", "确定");
    }

    private void ExecuteAddNamespace()
    {
        int count = 0;
        foreach (var info in scanResults.Where(c => c.Selected))
        {
            string fullPath = Path.GetFullPath(info.Path);
            string content = File.ReadAllText(fullPath);
            Match firstClass = Regex.Match(content, @"(public|internal|partial|class|struct|interface)");
            if (firstClass.Success)
            {
                string usings = content.Substring(0, firstClass.Index).Trim();
                string body = content.Substring(firstClass.Index).Trim();
                string indentedBody = "    " + body.Replace("\n", "\n    ");
                
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrEmpty(usings)) sb.AppendLine(usings).AppendLine();
                sb.AppendLine($"namespace {addNamespaceInput}\n{{");
                sb.AppendLine(indentedBody);
                sb.Append("}");
                File.WriteAllText(fullPath, sb.ToString());
                count++;
            }
        }
        FinishAction(count);
    }

    private void FinishAction(int count)
    {
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("操作成功", $"处理了 {count} 个文件", "确定");
        scanResults.Clear();
    }

    private void SaveBackup() { File.WriteAllText(backupPath, JsonUtility.ToJson(backupData, true)); }
    private void LoadBackup() { if (File.Exists(backupPath)) backupData = JsonUtility.FromJson<BackupData>(File.ReadAllText(backupPath)); }
}
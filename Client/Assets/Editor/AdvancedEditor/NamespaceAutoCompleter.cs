using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;

namespace Main.Editor
{
    public class NamespaceAutoCompleter : AssetPostprocessor
    {
        // 1. 定义排除列表 (文件名或路径关键字)
        private static readonly HashSet<string> ExcludeFiles = new HashSet<string>
        {
            "AOTGenericReferences.cs",
        };

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                // 2. 基础过滤：仅处理 C# 文件，跳过 Editor 目录
                if (!path.EndsWith(".cs") || path.Contains("/Editor/")) continue;

                // 3. 排除列表检查
                string fileName = Path.GetFileName(path);
                if (ExcludeFiles.Contains(fileName)) continue;

                string fullPath = Path.GetFullPath(path);
                if (!File.Exists(fullPath) || new FileInfo(fullPath).Length == 0) continue;

                string content = File.ReadAllText(fullPath);
                if (!content.Contains("class ") || content.Contains("namespace ")) continue;

                string assemblyName = CompilationPipeline.GetAssemblyNameFromScriptPath(path);
                assemblyName = string.IsNullOrEmpty(assemblyName) || assemblyName == "Assembly-CSharp" 
                    ? "Global" 
                    : assemblyName.Replace(".dll", "");

                var usingMatch = Regex.Match(content, @"^(?:\s*using\s+[\w\.]+;\s*)+");
                string usings = "";
                string body = content;

                if (usingMatch.Success)
                {
                    usings = usingMatch.Value.TrimEnd();
                    body = content.Substring(usingMatch.Length).TrimStart('\r', '\n');
                }

                string nl = content.Contains("\r\n") ? "\r\n" : "\n";
                string[] lines = body.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    string trimmedLine = line.TrimStart();

                    if (string.IsNullOrWhiteSpace(line)) { }
                    else if (trimmedLine.StartsWith("#"))
                    {
                        sb.Append(line);
                    }
                    else
                    {
                        sb.Append("    ");
                        sb.Append(line);
                    }

                    if (i < lines.Length - 1) sb.Append(nl);
                }

                StringBuilder finalContent = new StringBuilder();
                finalContent.Append(usings).Append(nl).Append(nl);
                finalContent.Append("namespace ").Append(assemblyName).Append(nl);
                finalContent.Append("{").Append(nl);
                finalContent.Append(sb.ToString());
                if (!sb.ToString().EndsWith(nl)) finalContent.Append(nl);
                finalContent.Append("}");

                try
                {
                    File.WriteAllText(fullPath, finalContent.ToString(), Encoding.UTF8);
                    string localPath = path;
                    EditorApplication.delayCall += () => AssetDatabase.ImportAsset(localPath);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"NamespaceAutoCompleter 写入失败: {e.Message}");
                }
            }
        }
    }
}
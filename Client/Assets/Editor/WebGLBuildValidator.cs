using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class WebGLBuildValidator
{
    // [PostProcessBuild] 标签让该方法在打包完成后自动触发
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        // 仅处理 WebGL 平台
        if (target != BuildTarget.WebGL) return;

        Debug.Log($"[WebGLValidator] 开始验证 WebGL 构建产物: {pathToBuiltProject}");

        // 1. 定位 Build 文件夹
        string buildDirPath = Path.Combine(pathToBuiltProject, "Build");
        if (!Directory.Exists(buildDirPath))
        {
            Debug.LogError("[WebGLValidator] 错误：找不到 Build 目录！");
            return;
        }

        // 2. 寻找编译后的 framework.js 文件
        string[] jsFiles = Directory.GetFiles(buildDirPath, "*.framework.js");
        if (jsFiles.Length == 0)
        {
            // 部分旧版本或特定配置可能直接叫 [项目名].js
            jsFiles = Directory.GetFiles(buildDirPath, "*.js");
        }

        if (jsFiles.Length == 0)
        {
            Debug.LogError("[WebGLValidator] 错误：在 Build 目录下未找到任何 JS 编译产物。");
            return;
        }

        string targetJsFile = jsFiles[0];
        string fileContent = File.ReadAllText(targetJsFile);

        // 3. 核心验证：检查你的关键函数是否被编译并拼接进去
        bool hasPlay = fileContent.Contains("_PlayPattern");
        bool hasRegister = fileContent.Contains("_RegisterPattern");

        if (hasPlay && hasRegister)
        {
            Debug.Log($"<color=green>[WebGLValidator] 验证成功！HapticPlugin 已成功编译进文件: {Path.GetFileName(targetJsFile)}</color>");
        }
        else
        {
            Debug.LogError($"<color=red>[WebGLValidator] 验证失败！未在编译产物中找到 HapticPlugin 的核心函数！请检查 .jslib 是否放在 Assets/Plugins/WebGL/ 目录下。</color>");
        }
    }
}
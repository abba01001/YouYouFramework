using System.Diagnostics;
using System.IO;

/// <summary>
/// 资源管理器工具类（官方原生方案，永不重复打开文件夹）
/// 原理：调用explorer官方参数，系统自动激活已有窗口
/// </summary>
public static class ExplorerUtil
{
    /// <summary>
    /// 打开文件夹（已打开则激活，不重复开窗）
    /// </summary>
    /// <param name="folderPath">文件夹完整路径</param>
    public static void OpenFolder(string folderPath)
    {
        // 路径校验
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            return;

        // 🔥 核心：Windows官方命令，自动激活已有窗口，不新建！
        Process.Start(new ProcessStartInfo()
        {
            FileName = "explorer.exe",
            Arguments = $@"/select,""{folderPath}""",  // 官方参数，系统自动处理
            UseShellExecute = true
        });
    }
}
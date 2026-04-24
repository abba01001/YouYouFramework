using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// 编辑器进程管理工具类
/// </summary>
public static class EditorProcessUtil
{
    #region Win32 API (用于窗口激活)
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr hWnd);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    private const int SW_RESTORE = 9;
    #endregion

    /// <summary>
    /// 运行一个 CMD 命令（不显示窗口）
    /// </summary>
    public static string RunCommand(string command, bool createNoWindow = true)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C " + command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = createNoWindow
            };

            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[ProcessUtil] 命令执行失败: {command}\n{e.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 强杀占用特定端口的进程
    /// </summary>
    public static void KillProcessOnPort(int port)
    {
        string output = RunCommand($"netstat -ano | findstr :{port}");
        if (string.IsNullOrEmpty(output)) return;

        string[] lines = output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string pid = parts.Last();

            if (int.TryParse(pid, out int pidInt) && pidInt != 0)
            {
                RunCommand($"taskkill /PID {pidInt} /F");
                Debug.Log($"[ProcessUtil] 已清理端口 {port} 上的进程 (PID: {pidInt})");
            }
        }
    }

    /// <summary>
    /// 启动一个外部可执行文件
    /// </summary>
    public static void StartExecutable(string exePath, string args = "", string workDir = "")
    {
        string fullPath = Path.GetFullPath(exePath);
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"[ProcessUtil] 找不到文件: {fullPath}");
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = fullPath,
            Arguments = args,
            WorkingDirectory = string.IsNullOrEmpty(workDir) ? Path.GetDirectoryName(fullPath) : workDir,
            UseShellExecute = true
        };

        Process.Start(startInfo);
    }

    /// <summary>
    /// 启动 Python HTTP 服务器
    /// </summary>
    public static void StartPythonServer(int port, string directory)
    {
        string fullDir = Path.GetFullPath(directory);
        // 使用 /K 可以让窗口报错时停留在那里，方便排查；/C 则是运行完或出错直接关掉
        string cmd = $"python -m http.server {port} --bind 0.0.0.0 --directory \"{fullDir}\"";
        
        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/C " + cmd,
            UseShellExecute = true,
            CreateNoWindow = false
        });
    }
    
    public static void OpenFolderSmart(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) return;

        string fullPath = Path.GetFullPath(folderPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string folderName = Path.GetFileName(fullPath);

        // 如果路径是根目录（如 D:\），folderName 会是空的，此时 fallback 到完整路径
        if (string.IsNullOrEmpty(folderName)) folderName = fullPath;

        IntPtr existingWindow = IntPtr.Zero;

        EnumWindows((hWnd, lParam) =>
        {
            // --- 兼容性核心：安全过滤 ---
            StringBuilder className = new StringBuilder(256);
            GetClassName(hWnd, className, className.Capacity);
            string cn = className.ToString();

            // 排除 Windows 桌面、任务栏、Shell 核心等窗口类名
            // CabinetWClass 是标准资源管理器窗口的类名
            if (cn != "CabinetWClass" && cn != "ExploreWClass") 
                return true; 

            StringBuilder sb = new StringBuilder(256);
            GetWindowText(hWnd, sb, sb.Capacity);
            string title = sb.ToString();

            // 匹配标题
            if (title.Equals(folderName, StringComparison.OrdinalIgnoreCase))
            {
                existingWindow = hWnd;
                return false; 
            }
            return true;
        }, IntPtr.Zero);

        if (existingWindow != IntPtr.Zero)
        {
            try 
            {
                if (IsIconic(existingWindow)) ShowWindow(existingWindow, SW_RESTORE);
                SetForegroundWindow(existingWindow);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[ExplorerUtil] 激活窗口失败，改用新建模式: {ex.Message}");
                OpenNewWindow(fullPath);
            }
        }
        else
        {
            OpenNewWindow(fullPath);
        }
    }

    private static void OpenNewWindow(string path)
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = "explorer.exe",
            Arguments = $"\"{path}\"",
            UseShellExecute = true
        });
    }
}
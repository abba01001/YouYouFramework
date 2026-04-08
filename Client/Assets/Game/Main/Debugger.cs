using System;
using UnityEngine;
using System.Diagnostics;
using System.Text;
using Object = UnityEngine.Object;

/// <summary>
/// 全局调试模块 - 追求极致简洁
/// 所有的调用在 Release 版本（非 Development Build）下会被编译器完全移除
/// </summary>
public static class Debugger
{
    // 全局运行时开关
    public static bool EnableLog = true;
    
    // 复用 StringBuilder 减少 GC (仅主线程使用)
    private static readonly StringBuilder stringBuilder = new StringBuilder();

    #region Log 接口

    // 单个对象调用，支持传入 context 以便在编辑器点击定位
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message, Object context = null)
    {
        if (!EnableLog) return;
        UnityEngine.Debug.Log($"{GetTimeStamp()}<color=#00FF00>[LOG]</color> {message}", context);
    }

    // 多参数任意拼接
    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void Log(params object[] messages)
    {
        if (!EnableLog) return;
        UnityEngine.Debug.Log($"{GetTimeStamp()}<color=#00FF00>[LOG]</color> {JointString(messages)}");
    }

    #endregion

    #region Warning 接口

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(params object[] messages)
    {
        if (!EnableLog) return;
        UnityEngine.Debug.LogWarning($"{GetTimeStamp()}<color=#FFFF00>[WARN]</color> {JointString(messages)}");
    }

    #endregion

    #region Error 接口

    [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
    public static void LogError(params object[] messages)
    {
        UnityEngine.Debug.LogError($"{GetTimeStamp()}<color=#FF0000>[ERROR]</color> {JointString(messages)}");
    }

    #endregion

    #region 内部工具

    private static string GetTimeStamp()
    {
        // 渲染格式：[2026-04-03 16:30:05.123] 
        return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] ";
    }

    private static string JointString(params object[] values)
    {
        if (values == null || values.Length == 0) return string.Empty;
        
        stringBuilder.Clear();
        for (int i = 0; i < values.Length; i++)
        {
            stringBuilder.Append(values[i]);
        }
        return stringBuilder.ToString();
    }

    #endregion
}
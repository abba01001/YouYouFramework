using NLog;

using System;

/// <summary>
/// nLog使用帮助类
/// </summary>
public class LoggerHelper
{
    // 通过 LogManager 获取一个全局的记录器实例
    private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

    // 单例模式，确保全局只有一个 LoggerHelper 实例
    private static LoggerHelper _obj;
    public static LoggerHelper Instance => _obj ?? (_obj = new LoggerHelper());

    private LoggerHelper()
    {
        Info("日志框架启动");
    }

    #region Debug，调试
    public void Debug(string msg) => _logger.Debug(msg);
    public void Debug(string msg, Exception err) => _logger.Debug(err, msg);
    #endregion

    #region Info，信息
    public void Info(string msg) => _logger.Info(msg);
    public void Info(string msg, Exception err) => _logger.Info(err, msg);
    #endregion

    #region Warn，警告
    public void Warn(string msg) => _logger.Warn(msg);
    public void Warn(string msg, Exception err) => _logger.Warn(err, msg);
    #endregion

    #region Trace，追踪
    public void Trace(string msg) => _logger.Trace(msg);
    public void Trace(string msg, Exception err) => _logger.Trace(err, msg);
    #endregion

    #region Error，错误
    public void Error(string msg) => _logger.Error(msg);
    public void Error(string msg, Exception err) => _logger.Error(err, msg);
    #endregion

    #region Fatal,致命错误
    public void Fatal(string msg) => _logger.Fatal(msg);
    public void Fatal(string msg, Exception err) => _logger.Fatal(err, msg);
    #endregion
}

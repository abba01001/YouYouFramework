using System;
using Protocols;

public static class ResponseHandler
{
    // 用于处理响应的方法
    public static void HandleResponse(string methodName, BaseMessage message)
    {
        // 通过反射调用对应的方法
        var method = typeof(ResponseHandler).GetMethod(methodName);
        if (method != null)
        {
            method.Invoke(null, new object[] { message }); // 调用方法，传递参数
        }
        else
        {
            Console.WriteLine($"未找到方法: {methodName}");
        }
    }

    // 示例处理方法，接收 BaseMessage 作为参数
    public static void s2c_handle_request_heart_beat(BaseMessage message)
    {
        // 处理心跳的逻辑
        Console.WriteLine("处理心跳请求...");
        // 处理传入的 BaseMessage
        // 例如，提取数据并处理
        // var data = message.Data;
    }

    public static void s2c_handle_other(BaseMessage message)
    {
        // 处理其他请求的逻辑
        Console.WriteLine("处理其他请求...");
        // 处理传入的 BaseMessage
    }
}
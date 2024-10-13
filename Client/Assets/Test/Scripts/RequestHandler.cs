using System;
using Protocols;

public static class RequestHandler
{
    // 用于发送请求的方法
    public static void SendRequest(string methodName, BaseMessage message)
    {
        // 通过反射调用对应的方法
        var method = typeof(RequestHandler).GetMethod(methodName);
        if (method != null)
        {
            method.Invoke(null, new object[] { message }); // 调用方法，传递参数
        }
        else
        {
            Console.WriteLine($"未找到方法: {methodName}");
        }
    }

    // 示例请求方法，接收 BaseMessage 作为参数
    public static void c2s_request_heart_beat(BaseMessage message)
    {
        // 发送心跳的逻辑
        Console.WriteLine("发送心跳请求...");
        // 例如，调用发送消息的方法
        // SendMessage(message);
    }

    public static void c2s_request_other(BaseMessage message)
    {
        // 发送其他请求的逻辑
        Console.WriteLine("发送其他请求...");
        // 例如，调用发送消息的方法
        // SendMessage(message);
    }
}
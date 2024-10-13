using System;
using System.Text;

public abstract class BaseData
{
    /// <summary>
    /// 获得自定义数据类的字节长度
    /// </summary>
    /// <returns>长度</returns>
    public abstract int GetDataBytesLength();

    /// <summary>
    /// 序列化数据类
    /// </summary>
    /// <returns>返回序列化后的字节数组</returns>
    public abstract byte[] SerializeData();

    public abstract int ReadingData(byte[] bytes, int beginIndex = 0);

    /// <summary>
    /// 序列化int类型数据
    /// </summary>
    /// <param name="bytes">序列化后存放的位置</param>
    /// <param name="data">需要序列化的数据</param>
    /// <param name="index">从bytes中的哪个位置开始存</param>
    protected void SerializeInt(byte[] bytes, int data, ref int index)
    {
        BitConverter.GetBytes(data).CopyTo(bytes, index);
        index += 4;
    }

    /// <summary>
    /// 序列化long类型数据
    /// </summary>
    /// <param name="bytes">序列化后存放的位置</param>
    /// <param name="data">需要序列化的数据</param>
    /// <param name="index">从bytes中的哪个位置开始存</param>
    protected void SerializeLong(byte[] bytes, long data, ref int index)
    {
        BitConverter.GetBytes(data).CopyTo(bytes, index);
        index += 8;
    }

    /// <summary>
    /// 序列化string类型数据
    /// </summary>
    /// <param name="bytes">序列化后存放的位置</param>
    /// <param name="data">需要序列化的数据</param>
    /// <param name="index">从bytes中的哪个位置开始存</param>
    protected void SerializeString(byte[] bytes, string data, ref int index)
    {
        byte[] strBytes = Encoding.UTF8.GetBytes(data);

        //因为string类型长度是不固定的，所以序列化string类型需要先保存string数据转化成字节数组的长度
        SerializeInt(bytes, strBytes.Length, ref index);

        strBytes.CopyTo(bytes, index);
        index += strBytes.Length;
    }

    /// <summary>
    /// 序列化自定义类数据
    /// </summary>
    /// <param name="bytes">序列化后存放的位置</param>
    /// <param name="data">需要序列化的数据</param>
    /// <param name="index">从bytes中的哪个位置开始存</param>
    protected void SerializeCustomClass(byte[] bytes, BaseData data, ref int index)
    {
        data.SerializeData().CopyTo(bytes,index);
        index += data.GetDataBytesLength();
    }

    /// <summary>
    /// 反序列化Int类型
    /// </summary>
    /// <param name="bytes">数据数组</param>
    /// <param name="startIndex">开始下标</param>
    /// <returns></returns>
    protected int ReadingInt(byte[] bytes, ref int startIndex)
    {
        int value = BitConverter.ToInt32(bytes, startIndex);
        startIndex += 4;
        return value;
    }

    /// <summary>
    /// 反序列化long类型
    /// </summary>
    /// <param name="bytes">数据数组</param>
    /// <param name="startIndex">开始下标</param>
    /// <returns></returns>
    protected long ReadingLong(byte[] bytes, ref int startIndex)
    {
        long value = BitConverter.ToInt64(bytes, startIndex);
        startIndex += 8;
        return value;
    }

    /// <summary>
    /// 反序列化string类型
    /// </summary>
    /// <param name="bytes">数据数组</param>
    /// <param name="startIndex">开始下标</param>
    /// <returns></returns>
    protected string ReadingString(byte[] bytes, ref int startIndex)
    {
        int strLength = ReadingInt(bytes, ref startIndex);
        string str = Encoding.UTF8.GetString(bytes, startIndex, strLength);
        startIndex += strLength;
        return str;
    }

    /// <summary>
    /// 反序列化自定义类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bytes">数据数组</param>
    /// <param name="startIndex">开始下标</param>
    /// <returns></returns>
    protected T ReadingCustomClass<T>(byte[] bytes, ref int startIndex) where T : BaseData, new()
    {
        T value = new T();
        int valueLength = value.ReadingData(bytes, startIndex);
        startIndex += valueLength;
        return value;
    }
}

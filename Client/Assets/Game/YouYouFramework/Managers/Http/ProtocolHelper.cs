using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;
using Microsoft.VisualBasic;
using Protocols.Item;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Protocols;

public static class ProtocolHelper
{
    private const string SECURITYKEY = "3ZkPqF9hDjW8q2Z7";//钥匙
    // 获得自定义数据类的字节长度
    public static int GetDataBytesLength()
    {
        // 示例实现，具体实现请根据您的需求
        return 0; // 替换为实际字节长度
    }

    // 序列化数据类
    public static byte[] SerializeData()
    {
        // 示例实现，具体实现请根据您的需求
        return new byte[0]; // 替换为实际序列化数据
    }

    public static int ReadingData(byte[] bytes, int beginIndex = 0)
    {
        // 示例实现，具体实现请根据您的需求
        return 0; // 替换为实际读取数据长度
    }

    // 序列化int类型数据
    public static void SerializeInt(byte[] bytes, int data, ref int index)
    {
        BitConverter.GetBytes(data).CopyTo(bytes, index);
        index += 4;
    }

    // 序列化long类型数据
    public static void SerializeLong(byte[] bytes, long data, ref int index)
    {
        BitConverter.GetBytes(data).CopyTo(bytes, index);
        index += 8;
    }

    // 序列化string类型数据
    public static void SerializeString(byte[] bytes, string data, ref int index)
    {
        byte[] strBytes = Encoding.UTF8.GetBytes(data);
        SerializeInt(bytes, strBytes.Length, ref index);
        strBytes.CopyTo(bytes, index);
        index += strBytes.Length;
    }

    // 序列化自定义类数据
    public static void SerializeCustomClass(byte[] bytes, BaseData data, ref int index)
    {
        data.SerializeData().CopyTo(bytes, index);
        index += data.GetDataBytesLength();
    }

    // 反序列化Int类型
    public static int ReadingInt(byte[] bytes, ref int startIndex)
    {
        int value = BitConverter.ToInt32(bytes, startIndex);
        startIndex += 4;
        return value;
    }

    // 反序列化long类型
    public static long ReadingLong(byte[] bytes, ref int startIndex)
    {
        long value = BitConverter.ToInt64(bytes, startIndex);
        startIndex += 8;
        return value;
    }

    // 反序列化string类型
    public static string ReadingString(byte[] bytes, ref int startIndex)
    {
        int strLength = ReadingInt(bytes, ref startIndex);
        string str = Encoding.UTF8.GetString(bytes, startIndex, strLength);
        startIndex += strLength;
        return str;
    }

    // 反序列化自定义类
    public static T ReadingCustomClass<T>(byte[] bytes, ref int startIndex) where T : BaseData, new()
    {
        T value = new T();
        int valueLength = value.ReadingData(bytes, startIndex);
        startIndex += valueLength;
        return value;
    }

    // 加密方法
    public static string Encrypt(byte[] plainBytes)
    {
        byte[] keyArray = Encoding.UTF8.GetBytes(SECURITYKEY);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = keyArray;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            aesAlg.GenerateIV(); // 每次加密生成一个新的 IV
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                // 在加密数据之前先写入 IV
                msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                    csEncrypt.FlushFinalBlock();
                }

                byte[] encrypted = msEncrypt.ToArray();
                return Convert.ToBase64String(encrypted); // 返回 Base64 字符串
            }
        }
    }

    // 解密方法
    public static byte[] Decrypt(string cipherText)
    {
        byte[] cipherTextArray = Convert.FromBase64String(cipherText);
        byte[] keyArray = Encoding.UTF8.GetBytes(SECURITYKEY);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = keyArray;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            // 从密文中提取 IV
            byte[] iv = new byte[aesAlg.BlockSize / 8];
            byte[] actualCipherText = new byte[cipherTextArray.Length - iv.Length];

            Array.Copy(cipherTextArray, 0, iv, 0, iv.Length); // 提取 IV
            Array.Copy(cipherTextArray, iv.Length, actualCipherText, 0, actualCipherText.Length); // 提取真正的密文

            aesAlg.IV = iv;
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(actualCipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (MemoryStream msOutput = new MemoryStream())
                    {
                        csDecrypt.CopyTo(msOutput);
                        return msOutput.ToArray(); // 返回解密后的字节数组
                    }
                }
            }
        }
    }

    // 将 object[] 转换为 byte[]
    public static byte[] ObjectArrayToByteArray(object[] data)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                foreach (var item in data)
                {
                    if (item is int)
                    {
                        writer.Write((int)item);
                    }
                    else if (item is long)
                    {
                        writer.Write((long)item);
                    }
                    else if (item is string)
                    {
                        writer.Write(((string)item).Length);
                        writer.Write(Encoding.UTF8.GetBytes((string)item));
                    }
                    // 可以根据需要添加其他类型的处理
                }
                return ms.ToArray();
            }
        }
    }

    // 将 byte[] 转换为 object[]
    public static object[] ByteArrayToObjectArray(byte[] bytes)
    {
        using (MemoryStream ms = new MemoryStream(bytes))
        {
            using (BinaryReader reader = new BinaryReader(ms))
            {
                // 假设你知道数据类型的顺序
                var dataList = new System.Collections.Generic.List<object>();
                while (ms.Position < ms.Length)
                {
                    // 示例处理：假设先处理 int、long 和 string
                    // 处理 int
                    if (ms.Position + 4 <= ms.Length)
                    {
                        dataList.Add(reader.ReadInt32());
                    }

                    // 处理 long
                    if (ms.Position + 8 <= ms.Length)
                    {
                        dataList.Add(reader.ReadInt64());
                    }

                    // 处理 string
                    if (ms.Position + 4 <= ms.Length)
                    {
                        int length = reader.ReadInt32();
                        if (ms.Position + length <= ms.Length)
                        {
                            string str = Encoding.UTF8.GetString(reader.ReadBytes(length));
                            dataList.Add(str);
                        }
                    }
                }
                return dataList.ToArray();
            }
        }
    }

    public static void UnpackData<T>(BaseMessage message, Action<T> action) where T : IMessage<T>, new()
    {
        if (message == null)
        {
            return;
        }

        try
        {
            // 解包为目标类型（如 ItemData）
            T unpackedData = new T();
            unpackedData.MergeFrom(message.Data); // 直接从消息的 Data 解包

            // 执行回调，将解包后的数据传递给回调
            action?.Invoke(unpackedData);
        }
        catch (InvalidProtocolBufferException ex)
        {
            Console.WriteLine($"解包失败: {ex.Message}");
        }
    }
}

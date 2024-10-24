using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using YouYou;

public sealed class SecurityUtil
{
    #region xorScale 异或因子

    /// <summary>
    /// 异或因子
    /// </summary>
    private static readonly byte[] xorScale = new byte[]
    {
        45, 66, 38, 55, 23, 254, 9, 165, 90, 19, 41, 45, 201, 58, 55, 37, 254, 185, 165, 169, 19, 171
    }; //.data文件的xor加解密因子

    #endregion

    private SecurityUtil()
    {
    }

    /// <summary>
    /// 对数组进行异或
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static byte[] Xor(byte[] buffer)
    {
        //------------------
        //第3步：xor解密
        //------------------
        int iScaleLen = xorScale.Length;
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte) (buffer[i] ^ xorScale[i % iScaleLen]);
        }

        return buffer;
    }
    
    public static string Encrypt(string plainText)
    {
        byte[] keyArray = Encoding.UTF8.GetBytes(Constants.SECURITYKEY);
        byte[] toEncryptArray = Encoding.UTF8.GetBytes(plainText);

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
                    csEncrypt.Write(toEncryptArray, 0, toEncryptArray.Length);
                    csEncrypt.FlushFinalBlock();
                }

                byte[] encrypted = msEncrypt.ToArray();
                return Convert.ToBase64String(encrypted);
            }
        }
    }


    public static string Decrypt(string cipherText)
    {
        byte[] cipherTextArray = Convert.FromBase64String(cipherText);
        byte[] keyArray = Encoding.UTF8.GetBytes(Constants.SECURITYKEY);

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
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }

    public static Dictionary<string, string> GetSecretKeyDic(string type)
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();
        string decryptedData = null;
        if (type == "editor")
        {
            string fullSavePath = Path.Combine(Application.dataPath, "PackageTool/SecretKey.bytes");
            if (File.Exists(fullSavePath))
            {
                // 读取 .bytes 文件的内容
                byte[] bytes = File.ReadAllBytes(fullSavePath);
                decryptedData = DecryptSecretKey(bytes);
            }
        }
        else
        {
            string fullSavePath = "Assets/PackageTool/TencentCloudKey.bytes";
            AssetReferenceEntity referenceEntity = GameEntry.Loader.LoadMainAsset(fullSavePath);
            if (referenceEntity != null)
            {
                TextAsset obj = UnityEngine.Object.Instantiate(referenceEntity.Target as TextAsset);
                AutoReleaseHandle.Add(referenceEntity, null);
                decryptedData = DecryptSecretKey(obj.bytes);
            }
        }
        if (decryptedData != null)
        {
            dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedData);
            // foreach (var pair in dic)
            // {
            //     GameUtil.LogError($"{pair.Key}---{pair.Value}");
            // }
        }
        return dic;
    }

    // 解密 AES 加密的数据
    public static string DecryptSecretKey(byte[] cipherText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(Constants.SECURITYKEY);
            byte[] iv = new byte[Constants.BLOCK_SIZE];

            // 提取 IV
            Array.Copy(cipherText, 0, iv, 0, Constants.BLOCK_SIZE);

            aesAlg.Key = keyArray;
            aesAlg.IV = iv;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(cipherText, Constants.BLOCK_SIZE, cipherText.Length - Constants.BLOCK_SIZE))
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
            {
                return srDecrypt.ReadToEnd();
            }
        }
    }

    // 从字节数组解析和解密数据
    public static (string secretId, string secretKey) ParseTencentCloudKeyFile(byte[] fileData)
    {
        using (MemoryStream ms = new MemoryStream(fileData))
        using (BinaryReader reader = new BinaryReader(ms))
        {
            // 读取 SecretId
            int secretIdLength = reader.ReadInt32();
            byte[] encryptedSecretId = reader.ReadBytes(secretIdLength);
            string secretId = DecryptSecretKey(encryptedSecretId);

            // 读取 SecretKey
            int secretKeyLength = reader.ReadInt32();
            byte[] encryptedSecretKey = reader.ReadBytes(secretKeyLength);
            string secretKey = DecryptSecretKey(encryptedSecretKey);

            return (secretId, secretKey);
        }
    }
    
}

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

    private static Dictionary<string, string> secretKeys;
    public static Dictionary<string, string> GetSecretKeyDic()
    {
        string fullSavePath = "";
#if UNITY_EDITOR
        fullSavePath = Path.Combine(Application.dataPath, "Game/Download/Key/SecretKey.bytes");
        if (File.Exists(fullSavePath))
        {
            // 读取 .bytes 文件的内容
            byte[] bytes = File.ReadAllBytes(fullSavePath);
            string decryptedData = DecryptSecretKey(bytes);
            secretKeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedData);
        }
        return secretKeys;
#endif
        if (secretKeys == null)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            fullSavePath = "Assets/Game/Download/Key/SecretKey.bytes";
            AssetReferenceEntity referenceEntity = GameEntry.Loader.LoadMainAsset(fullSavePath);
            if (referenceEntity != null)
            {
                TextAsset obj = UnityEngine.Object.Instantiate(referenceEntity.Target as TextAsset);
                AutoReleaseHandle.Add(referenceEntity, null);
                string decryptedData = DecryptSecretKey(obj.bytes);
                secretKeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedData);
            }
        }
        return secretKeys;
    }

    private static Dictionary<string, string> sqlKeys;
    public static Dictionary<string, string> GetSqlKeyDic()
    {
        string fullSavePath = "";
#if UNITY_EDITOR
        fullSavePath = Path.Combine(Application.dataPath, "Game/Download/Key/SqlKey.bytes");
        if (File.Exists(fullSavePath))
        {
            // 读取 .bytes 文件的内容
            byte[] bytes = File.ReadAllBytes(fullSavePath);
            string decryptedData = DecryptSecretKey(bytes);
            secretKeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedData);
        }
        return sqlKeys;
#endif
        if (sqlKeys == null)
        {
            fullSavePath = "Assets/Game/Download/Key/SqlKey.bytes";
            AssetReferenceEntity referenceEntity = GameEntry.Loader.LoadMainAsset(fullSavePath);
            if (referenceEntity != null)
            {
                TextAsset obj = UnityEngine.Object.Instantiate(referenceEntity.Target as TextAsset);
                AutoReleaseHandle.Add(referenceEntity, null);
                string decryptedData = DecryptSecretKey(obj.bytes);
                sqlKeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedData);
            }
        }
        return sqlKeys;
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

    
    // 使用 SHA256 哈希密码并返回 Base64 编码的字符串。
    public static string GetBase64Key(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashedBytes = sha256.ComputeHash(passwordBytes);
            return Convert.ToBase64String(hashedBytes);
        }
    }

    // 将输入的明文密码转换为 Base64 编码的哈希密码。
    public static string ConvertBase64Key(string plainPassword)
    {
        return GetBase64Key(plainPassword);
    }
    
}

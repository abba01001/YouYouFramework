using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;

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
        byte[] keyArray = Encoding.UTF8.GetBytes(Constants.SecurityKey);
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
        byte[] keyArray = Encoding.UTF8.GetBytes(Constants.SecurityKey);

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

}

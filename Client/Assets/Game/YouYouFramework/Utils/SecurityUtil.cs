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
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(toEncryptArray, 0, toEncryptArray.Length);
                    csEncrypt.Close();
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
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using (MemoryStream msDecrypt = new MemoryStream(cipherTextArray))
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

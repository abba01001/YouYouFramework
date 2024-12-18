using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TCPServer.Core;
using Constants = TCPServer.Core.Constants;

namespace TCPServer.Utils
{
    public class KeyUtils
    {
        static Dictionary<string, string> sqlKeys;
        public static string GetSqlKey(string key)
        {
            if(sqlKeys == null)
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Key", "SqlKey.bytes");
                Console.WriteLine(filePath);
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"The file at {filePath} does not exist.");
                }
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string decryptedData = DecryptSecretKey(encryptedData);
                sqlKeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedData);
            }
            return sqlKeys[key];
        }

        static Dictionary<string, string> tokenKeys;
        public static string GetTokenKey(string key)
        {
            if (tokenKeys == null)
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Key", "TokenKey.bytes");
                Console.WriteLine(filePath);
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"The file at {filePath} does not exist.");
                }
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string decryptedData = DecryptSecretKey(encryptedData);
                tokenKeys = JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedData);
            }
            return tokenKeys[key];
        }

        public static string DecryptSecretKey(byte[] cipherText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                byte[] keyArray = Encoding.UTF8.GetBytes(Constants.SECURITYKEY);
                byte[] iv = new byte[Constants.BLOCK_SIZE];

                // 提取 IV (Initialization Vector)
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

    }
}

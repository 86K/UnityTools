using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Tools
{
    /// <summary>
    /// Use a string of length 16 or 24 or 32 as a key to encrypt or decrypt string or text file.
    /// </summary>
    public static class KeyCrypto
    {
        /// <summary>
        /// 加密text类型文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key"></param>
        public static void EncryptTextFile(string path, string key)
        {
            path = path.Replace("file:///", "");
            if (File.Exists(path))
            {
                StreamReader streamReader = File.OpenText(path);
                string decryptData = streamReader.ReadToEnd();
                streamReader.Close();

                string enctyptData = EncryptString(decryptData, key);

                StreamWriter streamWriter = File.CreateText(path);
                streamWriter.Write(enctyptData);
                streamWriter.Close();
            }
            else
            {
                Debug.Log("There is not exist file path : " + path);
            }
        }
        
        /// <summary>
        /// 解密text类型文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key"></param>
        public static void DecryptTextFile(string path, string key)
        {
            path = path.Replace("file:///", "");
            if (File.Exists(path))
            {
                StreamReader streamReader = File.OpenText(path);
                string encryptData = streamReader.ReadToEnd();
                streamReader.Close();

                string decryptData = DecryptString(encryptData, key);

                StreamWriter streamWriter = File.CreateText(path);
                streamWriter.Write(decryptData);
                streamWriter.Close();
            }
            else
            {
                Debug.Log("There is not exist file path : " + path);
            }
        }
        
        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string EncryptString(string data, string key)
        {
            if (!KeyLengthIsFit(key))
            {
                return "";
            }

            byte[] bytes = Encoding.UTF8.GetBytes(key);
            RijndaelManaged rm = new RijndaelManaged();
            rm.Key = bytes;
            rm.Mode = CipherMode.ECB;
            rm.Padding = PaddingMode.PKCS7;

            ICryptoTransform cryptoTransform = rm.CreateEncryptor();
            String encryptStr = string.Empty;

            try
            {
                byte[] decryptData = Encoding.UTF8.GetBytes(data);
                byte[] encryptData = cryptoTransform.TransformFinalBlock(decryptData, 0, decryptData.Length);
                encryptStr = Convert.ToBase64String(encryptData, 0, encryptData.Length);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }

            return encryptStr;
        }
        
        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DecryptString(string data, string key)
        {
            if (!KeyLengthIsFit(key))
            {
                return "";
            }

            byte[] bytes = Encoding.UTF8.GetBytes(key);
            RijndaelManaged rm = new RijndaelManaged();
            rm.Key = bytes;
            rm.Mode = CipherMode.ECB;
            rm.Padding = PaddingMode.PKCS7;

            ICryptoTransform cryptoTransform = rm.CreateDecryptor();
            String decryptStr = string.Empty;

            try
            {
                byte[] encryptBytes = Convert.FromBase64String(data);
                byte[] decryptBytes = cryptoTransform.TransformFinalBlock(encryptBytes, 0, encryptBytes.Length);
                decryptStr = Encoding.UTF8.GetString(decryptBytes);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }

            return decryptStr;
        }

        static bool KeyLengthIsFit(string key)
        {
            int len = key.Length;
            if (len == 16 || len == 24 || len == 32)
            {
                return true;
            }

            Debug.LogError("Please use a key of length 16 or 24 or 32.");
            return false;
        }
    }
}
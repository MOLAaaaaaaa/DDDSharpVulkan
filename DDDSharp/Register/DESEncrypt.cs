using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RegisterAndEncrypt
{
    public class DESEncrypt
    {
        //字符必须8位
        byte[] Key = null;
        byte[] IV = null;
        public string errMessage = "";
        public DESEncrypt(string keyword) //字符必须时整数
        {
            if (keyword.Length == 4)
            {
                Key = Encoding.Unicode.GetBytes(keyword);
                IV = Encoding.Unicode.GetBytes(keyword);
            }
            else if (keyword.Length == 8)
            {
                Key = Encoding.ASCII.GetBytes(keyword);
                IV = Encoding.ASCII.GetBytes(keyword);
            }
            if (Key.Length != 8 || IV.Length != 8)
            {
                throw new Exception("key length not matched 8.");
            }
        }
        public string EncryptString(string str, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.Default.GetBytes(str);
            des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);// 密匙
            des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);// 初始化向量
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            var retB = Convert.ToBase64String(ms.ToArray());
            return retB;
        }
        public string DecryptString(string pToDecrypt, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Convert.FromBase64String(pToDecrypt);
            des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            // 如果两次密匙不一样，这一步可能会引发异常
            cs.FlushFinalBlock();
            return System.Text.Encoding.Default.GetString(ms.ToArray());
        }

        public string Encrypt(string content)
        {
            try
            {
                var data = Encoding.Unicode.GetBytes(content);
                var ms = new MemoryStream();
                var cs = new CryptoStream(ms, new DESCryptoServiceProvider().CreateEncryptor(Key, IV), CryptoStreamMode.Write);
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return "";
            }
        }

        public string Decrypt(string content)
        {
            try
            {
                var data = Convert.FromBase64String(content);
                var ms = new MemoryStream();
                var cs = new CryptoStream(ms, new DESCryptoServiceProvider().CreateDecryptor(Key, IV), CryptoStreamMode.Write);
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
                return Encoding.Unicode.GetString(ms.ToArray());
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return "";
            }
        }
    }
}

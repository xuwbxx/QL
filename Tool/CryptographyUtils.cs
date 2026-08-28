using System.Security.Cryptography;
using System.Text;

namespace Tool
{
    public class CryptographyUtils
    {

        #region 3DES 加密 解密
        /// <summary>
        /// 3DES 加密
        /// </summary>
        /// <param name="text">加密数据</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">向量</param>        
        /// <param name="cipherMode">指定用于加密的块密码模式</param>
        /// <param name="paddingMode">指定在消息数据块比加密操作所需的全部字节数短时应用的填充类型</param>
        /// <returns>加密后的文本</returns>
        public static string TripleDESEncrypt(string text, string key, string iv)
        {
            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
                byte[] inputBytes = Encoding.UTF8.GetBytes(text);

                using (TripleDES tripleDes = TripleDES.Create())
                {
                    tripleDes.Key = keyBytes;
                    tripleDes.IV = ivBytes;
                    tripleDes.Mode = CipherMode.CBC;
                    tripleDes.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform encryptor = tripleDes.CreateEncryptor())
                    using (MemoryStream memoryStream = new MemoryStream())
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        byte[] encryptedBytes = memoryStream.ToArray();
                        return Convert.ToBase64String(encryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public static string TripleDESDecrypt(string cipherText, string key, string iv)
        {
            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
                byte[] inputBytes = Convert.FromBase64String(cipherText);

                using (TripleDES tripleDes = TripleDES.Create())
                {
                    tripleDes.Key = keyBytes;
                    tripleDes.IV = ivBytes;
                    tripleDes.Mode = CipherMode.CBC;
                    tripleDes.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform decryptor = tripleDes.CreateDecryptor())
                    using (MemoryStream memoryStream = new MemoryStream(inputBytes))
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    using (MemoryStream outputStream = new MemoryStream()) // 新增输出流
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        // 循环读取直到无数据
                        while ((bytesRead = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            outputStream.Write(buffer, 0, bytesRead);
                        }
                        byte[] decryptedBytes = outputStream.ToArray();
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        #endregion

        #region 普通加密

        public static string DESEncrypt(string text, string key, string iv, CipherMode cipherMode = CipherMode.ECB, PaddingMode paddingMode = PaddingMode.PKCS7)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] bytes2 = Encoding.UTF8.GetBytes(key);
            byte[] bytes3 = Encoding.UTF8.GetBytes(iv);
            using MemoryStream memoryStream = new MemoryStream();
            using (DES desAlg = DES.Create())
            {
                desAlg.Padding = paddingMode;
                desAlg.Mode = cipherMode;
                using CryptoStream cryptoStream = new CryptoStream(memoryStream, desAlg.CreateEncryptor(bytes2, bytes3), CryptoStreamMode.Write);
                cryptoStream.Write(bytes, 0, bytes.Length);
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public static string DESDecrypt(string text, string key, string iv, CipherMode cipherMode = CipherMode.ECB, PaddingMode paddingMode = PaddingMode.PKCS7)
        {
            byte[] array = Convert.FromBase64String(text);
            byte[] bytes = Encoding.UTF8.GetBytes(key);
            byte[] bytes2 = Encoding.UTF8.GetBytes(iv);
            using MemoryStream memoryStream = new MemoryStream();
            using (DES desAlg = DES.Create())
            {
                desAlg.Padding = paddingMode;
                desAlg.Mode = cipherMode;
                using CryptoStream cryptoStream = new CryptoStream(memoryStream, desAlg.CreateDecryptor(bytes, bytes2), CryptoStreamMode.Write);
                cryptoStream.Write(array, 0, array.Length);
            }

            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        #endregion


        #region base64 加密解密

        public static string Base64Encrypt(string source)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(source);
            try
            {
                return Convert.ToBase64String(bytes);
            }
            catch
            {
                return source;
            }
        }

        public static string Base64Decrypt(string source)
        {
            byte[] bytes = Convert.FromBase64String(source);
            try
            {
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return source;
            }
        }

        #endregion


        #region MD5加密
        public static string Encrypt(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }
        #endregion
    }
}

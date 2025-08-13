using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using UnityGameFramework.Runtime;

namespace LeeFramework.Scripts.GameSave
{
    public class GameSaveSerializer
    {
        private const int KeySize = 32; // 256位
        private const int IVSize = 16;  // 128位
        private const int SaltSize = 32; // 256位
        private const int Iterations = 100000; // PBKDF2迭代次数

        private static readonly byte[] MasterKey;
        private static readonly byte[] Salt;
        private static readonly bool UseCompression = true;
        private static readonly bool UseEncryption = true;
        private static readonly bool UseIntegrityCheck = true;

        static GameSaveSerializer()
        {
            // 在实际应用中，这些值应该从安全的配置系统或密钥管理服务中获取
            string masterKeyString = "YourMasterKey12345678901234567890123456789012";
            string saltString = "YourSalt12345678901234567890123456789012";

            // 使用PBKDF2派生密钥
            using (var deriveBytes = new Rfc2898DeriveBytes(masterKeyString, Encoding.UTF8.GetBytes(saltString), Iterations))
            {
                MasterKey = deriveBytes.GetBytes(KeySize);
            }

            // 生成随机盐值
            Salt = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(Salt);
            }
        }

        /// <summary>
        /// 加密数据
        /// </summary>
        public static byte[] EncryptData(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                Log.Warning("Data is null or empty.");
                return null;
            }

            try
            {
                byte[] processedData = data;

                // 压缩数据
                if (UseCompression)
                {
                    processedData = CompressData(processedData);
                }

                // 加密数据
                if (UseEncryption)
                {
                    processedData = EncryptDataInternal(processedData);
                }

                // 添加完整性校验
                if (UseIntegrityCheck)
                {
                    processedData = AddIntegrityCheck(processedData);
                }

                return processedData;
            }
            catch (Exception exception)
            {
                Log.Error("Encrypt data failed with exception '{0}'.", exception);
                return null;
            }
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        public static byte[] DecryptData(byte[] encryptedData)
        {
            if (encryptedData == null || encryptedData.Length == 0)
            {
                Log.Warning("Encrypted data is null or empty.");
                return null;
            }

            try
            {
                byte[] processedData = encryptedData;

                // 验证完整性
                if (UseIntegrityCheck)
                {
                    if (!VerifyIntegrity(processedData, out processedData))
                    {
                        Log.Error("Data integrity check failed.");
                        return null;
                    }
                }

                // 解密数据
                if (UseEncryption)
                {
                    processedData = DecryptDataInternal(processedData);
                }

                // 解压数据
                if (UseCompression)
                {
                    processedData = DecompressData(processedData);
                }

                return processedData;
            }
            catch (Exception exception)
            {
                Log.Error("Decrypt data failed with exception '{0}'.", exception);
                return null;
            }
        }

        /// <summary>
        /// 计算数据的哈希值用于完整性校验
        /// </summary>
        public static string CalculateHash(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                Log.Warning("Data is null or empty.");
                return string.Empty;
            }

            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(data);
                    return Convert.ToBase64String(hashBytes);
                }
            }
            catch (Exception exception)
            {
                Log.Error("Calculate hash failed with exception '{0}'.", exception);
                return string.Empty;
            }
        }

        /// <summary>
        /// 添加完整性校验
        /// </summary>
        private static byte[] AddIntegrityCheck(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                // 计算HMAC
                byte[] hmac = CalculateHmac(data);
                
                // 写入HMAC长度
                writer.Write(hmac.Length);
                
                // 写入HMAC
                writer.Write(hmac);
                
                // 写入数据
                writer.Write(data);
                
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 验证完整性
        /// </summary>
        private static bool VerifyIntegrity(byte[] data, out byte[] originalData)
        {
            originalData = null;

            try
            {
                using (MemoryStream ms = new MemoryStream(data))
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    // 读取HMAC长度
                    int hmacLength = reader.ReadInt32();
                    
                    // 读取HMAC
                    byte[] storedHmac = reader.ReadBytes(hmacLength);
                    
                    // 读取数据
                    originalData = reader.ReadBytes((int)(ms.Length - ms.Position));
                    
                    // 计算数据的HMAC
                    byte[] calculatedHmac = CalculateHmac(originalData);
                    
                    // 比较HMAC
                    return CompareHmac(storedHmac, calculatedHmac);
                }
            }
            catch (Exception exception)
            {
                Log.Error("Verify integrity failed with exception '{0}'.", exception);
                return false;
            }
        }

        /// <summary>
        /// 计算HMAC
        /// </summary>
        private static byte[] CalculateHmac(byte[] data)
        {
            using (HMACSHA256 hmac = new HMACSHA256(MasterKey))
            {
                return hmac.ComputeHash(data);
            }
        }

        /// <summary>
        /// 比较HMAC
        /// </summary>
        private static bool CompareHmac(byte[] hmac1, byte[] hmac2)
        {
            if (hmac1.Length != hmac2.Length)
            {
                return false;
            }

            for (int i = 0; i < hmac1.Length; i++)
            {
                if (hmac1[i] != hmac2[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 压缩数据
        /// </summary>
        private static byte[] CompressData(byte[] data)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(output, CompressionLevel.Optimal))
                {
                    gzip.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }

        /// <summary>
        /// 解压数据
        /// </summary>
        private static byte[] DecompressData(byte[] compressedData)
        {
            using (MemoryStream input = new MemoryStream(compressedData))
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
                {
                    gzip.CopyTo(output);
                }
                return output.ToArray();
            }
        }

        /// <summary>
        /// 内部加密方法
        /// </summary>
        private static byte[] EncryptDataInternal(byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = MasterKey;
                
                // 生成随机IV
                byte[] iv = new byte[IVSize];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(iv);
                }
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream())
                {
                    // 写入IV
                    ms.Write(iv, 0, iv.Length);

                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 内部解密方法
        /// </summary>
        private static byte[] DecryptDataInternal(byte[] encryptedData)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = MasterKey;

                // 从加密数据中读取IV
                byte[] iv = new byte[IVSize];
                Array.Copy(encryptedData, 0, iv, 0, IVSize);
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        // 跳过IV部分
                        cs.Write(encryptedData, IVSize, encryptedData.Length - IVSize);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
            }
        }
    }
}
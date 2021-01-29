using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Tenet.Infrastructure.Helpers
{
    public static class Encryption
    {
        public static string encrypt_string(string plainText, byte[] key, byte[] iv)
        {
            using Aes aesAlg = Aes.Create();
            Debug.Assert(aesAlg != null, nameof(aesAlg) + " != null");
            
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msEncrypt = new MemoryStream();
            using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }
            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public static string decrypt_string(string cipherText, byte[] key, byte[] iv)
        {
            string plaintext = null;

            using Aes aesAlg = Aes.Create();
            Debug.Assert(aesAlg != null, nameof(aesAlg) + " != null");
            
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor();

            using MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new StreamReader(csDecrypt);
            plaintext = srDecrypt.ReadToEnd();

            return plaintext;
        }

        public static string iv_key()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public static string GetSha256Hash(string input)
        {
            var sha256Hash = SHA256.Create();

            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            foreach (var t in bytes)
            {
                sBuilder.Append(t.ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static string Encrypt(string message, string encKey, string iv)
        {
            byte[] key = Encoding.Default.GetBytes(GetSha256Hash(encKey).Substring(0, 32));

            return encrypt_string(message, key, Encoding.Default.GetBytes(GetSha256Hash(iv).Substring(0, 16)));
        }

        public static string Decrypt(string message, string encKey, string iv)
        {
            byte[] key = Encoding.Default.GetBytes(GetSha256Hash(encKey).Substring(0, 32));

            return decrypt_string(message, key, Encoding.UTF8.GetBytes(GetSha256Hash(iv).Substring(0, 16)));
        }
    }
}

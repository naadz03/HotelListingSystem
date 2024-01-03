using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace HotelListingSystem.Engines
{
    public static class SecurityEncryption
    {
        public const string Key = "!@#$%^&*({]|";
        private const int SaltSize = 16; // Size of the salt used for key derivation
        private const int KeySize = 32; // Size of the derived key in bytes (256 bits)
        private const int Iterations = 10000; // Number of iterations for key derivation


        public static string EncryptPassword(string password)
        {
            byte[] encryptedBytes;

            byte[] salt = GenerateSalt();
            byte[] derivedKey = DeriveKey(Key, salt);

            using (Aes aes = Aes.Create())
            {
                aes.Key = derivedKey;
                aes.IV = new byte[16]; // Set a random initialization vector

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                using (var memoryStream = new System.IO.MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(passwordBytes, 0, passwordBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    encryptedBytes = memoryStream.ToArray();
                }
            }

            return Convert.ToBase64String(encryptedBytes);
        }


        public static string DecryptPassword(string encryptedPassword)
        {
            byte[] passwordBytes;

            byte[] salt = GenerateSalt();
            byte[] derivedKey = DeriveKey(Key, salt);

            using (Aes aes = Aes.Create())
            {
                aes.Key = derivedKey;
                aes.IV = new byte[16]; // Set the same initialization vector used for encryption

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                byte[] encryptedBytes = Convert.FromBase64String(encryptedPassword);

                using (var memoryStream = new System.IO.MemoryStream(encryptedBytes))
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    passwordBytes = new byte[encryptedBytes.Length];
                    int decryptedByteCount = cryptoStream.Read(passwordBytes, 0, passwordBytes.Length);
                    Array.Resize(ref passwordBytes, decryptedByteCount);
                }
            }

            return Encoding.UTF8.GetString(passwordBytes);
        }



        private static byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[SaltSize];
                rng.GetBytes(salt);
                return salt;
            }
        }

        private static byte[] DeriveKey(string key, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(key, salt, Iterations))
            {
                return pbkdf2.GetBytes(KeySize);
            }
        }

    }
}
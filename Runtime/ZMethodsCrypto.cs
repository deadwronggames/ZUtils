using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsCrypto
    {
        private const string CRYPTO_IV = "I+MoT3p5C/EBj6GCZfaDjw==";
        private const string CRYPTO_KEY = "y/cE5Ef7E/zZIaZ8KffuUfHMrmI/UZHQBAgx9YWaTv8="; 
        
        // encrypt string into byte array
        public static byte[] Encrypt(string data)
        {
            using Aes aesProvider = CreateAesProvider();
            using ICryptoTransform cryptoTransform = aesProvider.CreateEncryptor();
            
            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, cryptoTransform, CryptoStreamMode.Write);
            using StreamWriter writer = new(cryptoStream);

            writer.Write(data);
            writer.Flush();
            cryptoStream.FlushFinalBlock();
        
            return memoryStream.ToArray();
        }

        // decrypt byte array to a string
        public static string Decrypt(byte[] encryptedData)
        {
            using Aes aesProvider = CreateAesProvider();
            using ICryptoTransform cryptoTransform = aesProvider.CreateDecryptor();
            
            using MemoryStream memoryStream = new(encryptedData);
            using CryptoStream cryptoStream = new(memoryStream, cryptoTransform, CryptoStreamMode.Read);
            using StreamReader reader = new(cryptoStream);

            return reader.ReadToEnd();
        }
        
        // can be used to create new parameters
        public static void GenerateAndLogCryptoParameters()
        {
            using Aes aesProvider = Aes.Create();
            Debug.Log($"Initialization Vector: {Convert.ToBase64String(aesProvider.IV)}");
            Debug.Log($"Key: {Convert.ToBase64String(aesProvider.Key)}");
        }

        private static Aes CreateAesProvider()
        {
            Aes aesProvider = Aes.Create();
            aesProvider.IV = Convert.FromBase64String(CRYPTO_IV);
            aesProvider.Key = Convert.FromBase64String(CRYPTO_KEY);

            return aesProvider;
        }
    }
}
using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsCrypto
    {
        private const string KEY_FOLDER_PATH = "Assets/Resources/CryptoKey";
        private const string KEY_FILE_NAME = "CryptoKeyContainer";
        
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
        
        public static Aes CreateAesProvider()
        {
            (string iv, string key) = GetCryptoPair();
            
            Aes aesProvider = Aes.Create();
            aesProvider.IV = Convert.FromBase64String(iv);
            aesProvider.Key = Convert.FromBase64String(key);

            return aesProvider;
        }
        
        private static (string iv, string key) GetCryptoPair()
        {
            CryptoKeyContainer keyContainer = Resources.Load<CryptoKeyContainer>($"CryptoKey/{KEY_FILE_NAME}");

            return (keyContainer == null) ? 
                CreateCryptoPair() : 
                (keyContainer.IV, keyContainer.Key);
        }

        private static (string iv, string key) CreateCryptoPair()
        {
#if UNITY_EDITOR
            // Create new pair
            using Aes aesProvider = Aes.Create();
            string newIV = Convert.ToBase64String(aesProvider.IV);
            string newKey = Convert.ToBase64String(aesProvider.Key);

            // Create folder to save pair to
            if (!Directory.Exists(KEY_FOLDER_PATH))
                Directory.CreateDirectory(KEY_FOLDER_PATH);
            
            // Create SO save instance
            CryptoKeyContainer instance = ScriptableObject.CreateInstance<CryptoKeyContainer>();
            (instance.IV, instance.Key) = (newIV, newKey);
            string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{Path.Combine(KEY_FOLDER_PATH, $"{KEY_FILE_NAME}.asset")}");
            
            AssetDatabase.CreateAsset(instance, fullPath);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = instance;

            return (instance.IV, instance.Key);
            
#else
            "Can not create crypto pair in build. Returning null.".Log(level: ZMethodsDebug.LogLevel.Critical);
            return (null, null);
#endif
        }
    }
}

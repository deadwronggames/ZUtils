using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsFileIO
    {
        public static bool TryWriteFile(string filePath, string fileContent, bool doEncryptFile = false, bool doOverwriteExisting = false, bool doAppend = false)
        {
            // checks
            if (doAppend && doOverwriteExisting) throw new ArgumentException("Cannot both append and overwrite the file. Please choose one option.");
            bool isFileExisting = File.Exists(filePath);
            const long maxFileSize = 10 * 1024 * 1024; // 10 MB
            if (isFileExisting && doAppend && new FileInfo(filePath).Length > maxFileSize) throw new InvalidOperationException($"The log file {filePath} exceeds the maximum size of 10MB.");
            if (!doOverwriteExisting && !doAppend && File.Exists(filePath)) return false; 
            
            // write
            try
            {
                FileMode fileMode = (doAppend) ? FileMode.Append : FileMode.Create; 
                using FileStream stream = new(filePath, fileMode, FileAccess.Write, FileShare.None);
                if (doEncryptFile)
                {
                    byte[] encryptedData = ZMethodsCrypto.Encrypt(fileContent);
                    stream.Write(encryptedData, 0, encryptedData.Length);
                }
                else
                {
                    using StreamWriter writer = new(stream);
                    writer.Write(fileContent);
                }

                return true;
            }
            
            catch (Exception e)
            {
                Debug.LogWarning($"{nameof(ZMethodsFileIO)}.{nameof(TryReadFile)}: File could not be written to {filePath}:\n{e}");
                return false;
            }
        }
        
        public static bool TryReadFile(string filePath, out string fileContent, bool isFileEncrypted = false)
        {
            try
            {
                using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                if (isFileEncrypted)
                {
                    byte[] encryptedData = new byte[stream.Length];
                    _ = stream.Read(encryptedData, 0, encryptedData.Length);
                    fileContent = ZMethodsCrypto.Decrypt(encryptedData);
                }
                else
                {
                    using StreamReader reader = new(stream);
                    fileContent = reader.ReadToEnd();
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{nameof(ZMethodsFileIO)}.{nameof(TryReadFile)}: File at {filePath} could not be read:\n{e}");
                fileContent = null;
                return false;
            }
        }

        public static TContent DeserializeFromJson<TContent>(TextAsset file) => JsonDeserialize<TContent>(file.text);
        public static TContent DeserializeFromJson<TContent>(string filePath)
        {
            if (!TryReadFile(filePath, out string fileContent))
            {
                Debug.LogWarning($"{nameof(ZMethodsFileIO)}.{nameof(TryReadFile)}: Failed to parse JSON content from file {filePath}. Returning default.");
                return default;
            }
            
            return JsonDeserialize<TContent>(fileContent);
        }
        public static TContent JsonDeserialize<TContent>(string fileContent)
        {
            try
            {
                TContent deserialized = JsonConvert.DeserializeObject<TContent>(fileContent);
                return deserialized;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(ZMethodsFileIO)}.{nameof(TryReadFile)}: Failed to parse JSON content. Returning default.\n{ex.Message}");
                return default;
            }
        }

        public static TValue[] GetArrayFromTextAsset<TValue>(TextAsset textAsset, Func<string, TValue> parseFunc, bool doAllowNull = false)
        {
            if (doAllowNull && textAsset == null) return null;

            // split the text into lines and parse each line using the provided parse function
            return GetLines(textAsset.text)
                .Select(line =>
                {
                    try { return parseFunc(line); }
                    catch (Exception ex) { throw new FormatException($"Unable to parse '{line}' to {typeof(TValue)}: {ex.Message}", ex); }
                }).ToArray();
        }

        /// <param name="csvTextAsset">File to be read.</param>
        /// <param name="defaultString">string used for empty values</param>
        /// <returns>The first column provides the outer key, the first line the inner keys.</returns>
        public static Dictionary<string, Dictionary<string, string>> GetNestedDictsFromCSVSpreadSheet(TextAsset csvTextAsset, string defaultString = "")
        {
            string[] lines = GetLines(csvTextAsset.text);
            Dictionary<string, Dictionary<string, string>> result = new();
            
            if (lines.Length == 0) return result;

            // the first line contains the headers
            string[] headers = lines[0].Split(',');

            // loop through each line (skipping the header row)
            for (int i = 1; i < lines.Length; i++)
            {
                Dictionary<string, string> innerDict = new();
                string[] lineEntries = lines[i].Split(','); // CAREFUL! don't have other commas in fields

                // skip rows with missing columns
                if (lineEntries.Length < headers.Length) continue;  
                
                // populate the inner dictionary with headers as keys and corresponding row values (skipping the first column)
                for (int j = 1; j < lineEntries.Length; j++)
                    innerDict[headers[j]] = (string.IsNullOrEmpty(lineEntries[j])) ? defaultString : lineEntries[j];

                // add the inner dictionary to the result dictionary with the outer key
                string outerKey = lineEntries[0];
                result[outerKey] = innerDict;
            }

            return result;
        }

        private static string[] GetLines(string contentString)
        {
            return contentString.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
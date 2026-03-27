using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Mosambi.Core
{
    public class SecureFileSaveManager<T> : ISaveManager<T> where T : new()
    {
        private readonly MosambiSecuritySettings _settings;
        public T Data { get; private set; }

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, _settings.saveFileName);

        public SecureFileSaveManager(MosambiSecuritySettings settings)
        {
            _settings = settings;

            // Safety Check: AES-256 requires 32 chars
            if (_settings.encryptionKey.Length < 32)
                Debug.LogWarning("[Mosambi] Encryption Key is too short! AES-256 needs 32 characters.");

            Load();
        }

        public void Load()
        {
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string encryptedJson = File.ReadAllText(SaveFilePath);
                    string decryptedJson = Decrypt(encryptedJson, _settings.encryptionKey, _settings.initializationVector);
                    Data = JsonUtility.FromJson<T>(decryptedJson);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Decryption failed. Data may be corrupted. Error: {e.Message}");
                    Data = new T();
                }
            }
            else
            {
                Data = new T();
            }
        }

        public void Save()
        {
            if (Data == null) return;

            string json = JsonUtility.ToJson(Data);
            string encrypted = Encrypt(json, _settings.encryptionKey, _settings.initializationVector);
            File.WriteAllText(SaveFilePath, encrypted);
        }

        public void ClearData()
        {
            if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath);
            Data = new T();
            Save();
        }

        #region AES Encryption Logic

        private string Encrypt(string plainText, string keyString, string ivString)
        {
            // DEFENSIVE SIZING: Forces Key to 32 chars and IV to 16 chars
            keyString = keyString.PadRight(32).Substring(0, 32);
            ivString = ivString.PadRight(16).Substring(0, 16);

            byte[] iv = Encoding.UTF8.GetBytes(ivString);
            byte[] key = Encoding.UTF8.GetBytes(keyString);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        private string Decrypt(string cipherText, string keyString, string ivString)
        {
            // DEFENSIVE SIZING: Forces Key to 32 chars and IV to 16 chars
            keyString = keyString.PadRight(32).Substring(0, 32);
            ivString = ivString.PadRight(16).Substring(0, 16);

            byte[] iv = Encoding.UTF8.GetBytes(ivString);
            byte[] key = Encoding.UTF8.GetBytes(keyString);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        #endregion
    }
}
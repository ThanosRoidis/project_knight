using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Shared
{
    public class JsonDataService : IDataService
    {

        private const string KEY = "JgqBRM+u+CBJnj1dcwvZu8fzfSKlJUUJl1Xan5ighiw=";
        private const string IV = "YGMqCm1pV4OHu+ZEPog4xQ==";

        public bool SaveData<T>(string path, T Data, bool encrypted)
        {
            try {
                if (File.Exists(path)) {
                    Debug.Log("Data exists. Deleting old file and writing a new one!");
                    File.Delete(path);
                } else {
                    Debug.Log("Writing file for the first time!");
                }
                if (encrypted) {
                    using FileStream stream = File.Create(path);
                    WriteEncryptedData(Data, stream);
                    stream.Close();
                } else {
                    File.WriteAllText(path, JsonConvert.SerializeObject(Data));
                }
                return true;
            } catch (Exception e) {
                Debug.LogError($"Unable to save data due to: {e.Message} {e.StackTrace}");
                return false;
            }
        }

        private void WriteEncryptedData<T>(T Data, FileStream Stream)
        {
            using Aes aesProvider = Aes.Create();
            aesProvider.Key = Convert.FromBase64String(KEY);
            aesProvider.IV = Convert.FromBase64String(IV);
            using ICryptoTransform cryptoTransform = aesProvider.CreateEncryptor();
            using CryptoStream cryptoStream = new CryptoStream(
                Stream,
                cryptoTransform,
                CryptoStreamMode.Write
            );

            // You can uncomment the below to see a generated value for the IV & key.
            // You can also generate your own if you wish
            //Debug.Log($"Initialization Vector: {Convert.ToBase64String(aesProvider.IV)}");
            //Debug.Log($"Key: {Convert.ToBase64String(aesProvider.Key)}");
            cryptoStream.Write(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(Data)));
        }


        public T LoadData<T>(string path, bool encrypted)
        {
            if (!File.Exists(path)) {
                Debug.LogError($"Cannot load file at {path}. File does not exist!");
                throw new FileNotFoundException($"{path} does not exist!");
            }

            try {
                T data;
                if (encrypted) {
                    data = ReadEncryptedData<T>(path);
                } else {
                    string dataTxt = File.ReadAllText(path);
                    data = JsonConvert.DeserializeObject<T>(dataTxt);
                }
                return data;
            } catch (Exception e) {
                Debug.LogError($"Failed to load data due to: {e.Message} {e.StackTrace}");
                throw e;
            }
        }

        private T ReadEncryptedData<T>(string Path)
        {
            byte[] fileBytes = File.ReadAllBytes(Path);
            using Aes aesProvider = Aes.Create();

            aesProvider.Key = Convert.FromBase64String(KEY);
            aesProvider.IV = Convert.FromBase64String(IV);

            using ICryptoTransform cryptoTransform = aesProvider.CreateDecryptor(
                aesProvider.Key,
                aesProvider.IV
            );
            using MemoryStream decryptionStream = new MemoryStream(fileBytes);
            using CryptoStream cryptoStream = new CryptoStream(
                decryptionStream,
                cryptoTransform,
                CryptoStreamMode.Read
            );
            using StreamReader reader = new StreamReader(cryptoStream);

            string result = reader.ReadToEnd();

            Debug.Log($"Decrypted result (if the following is not legible, probably wrong key or iv): {result}");
            return JsonConvert.DeserializeObject<T>(result);
        }

    }
}
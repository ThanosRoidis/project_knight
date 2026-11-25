using Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveDataContainer
{
    // Necessary for Newton.json
    public SaveDataContainer() { }

    public SaveDataContainer(ConsistentData data)
    {

    }
}

namespace Shared
{
    public abstract class ConsistentData : Singleton<ConsistentData>
    {
        private static string _saveBaseEncrypted = "save";
        private static string _saveBaseRaw = "save_raw";
        private static string _saveDateFormat = "yyyyMMddHHmmss";
        private static int _saveLimits = 10;


        protected virtual void FromContainer(SaveDataContainer container)
        {
        }


        public static string GetSaveDirectory()
        {
#if UNITY_EDITOR
            string saveDirectoryPath = Path.Join(Directory.GetParent(Application.dataPath).FullName, "Saves");
#else
        string saveDirectoryPath = Application.persistentDataPath;
#endif

            return saveDirectoryPath;
        }

        private void SaveDataEncryption(string timestamp, bool encrypted)
        {
            string saveBase = encrypted ? _saveBaseEncrypted : _saveBaseRaw;

            // Save the encrypted file
            string saveFilename = $"{saveBase}_{timestamp}.json";
            string saveFilepath = Path.Join(GetSaveDirectory(), saveFilename);
            long startTime = DateTime.Now.Ticks;
            IDataService dataService = new JsonDataService();
            bool ok = SaveContainer(dataService, saveFilepath, encrypted);
            if (ok) {
                long SaveTime = DateTime.Now.Ticks - startTime;
                Debug.Log($"Saved game at file {saveFilename} (Save Time: {(SaveTime / TimeSpan.TicksPerMillisecond):N4}ms)");
            } else {
                Debug.LogError("Could not save file!");
            }
            // Delete old save files
            List<string> allSaves = GetSaveFilesOrderedOldestToNewest(GetSaveDirectory(), saveBase);
            while (allSaves.Count > _saveLimits) {
                File.Delete(allSaves[0]);
                allSaves.RemoveAt(0);
                Debug.Log($"Deleted file {Path.GetFileName(allSaves[0])}");
            }
        }

        public void SaveData()
        {
            string now = DateTime.Now.ToString(_saveDateFormat);

            bool encrypted = !(Application.isEditor || Debug.isDebugBuild);
            SaveDataEncryption(now, encrypted);
        }

        public bool LoadData()
        {
            IDataService dataService = new JsonDataService();

            bool encrypted = !(Application.isEditor || Debug.isDebugBuild);
            string saveBase = encrypted ? _saveBaseEncrypted : _saveBaseRaw;

            long startTime = DateTime.Now.Ticks;
            try {
                List<string> allSaves = GetSaveFilesOrderedOldestToNewest(GetSaveDirectory(), saveBase);
                string saveFilepath = allSaves[allSaves.Count - 1];
                SaveDataContainer container = LoadContainer(dataService, saveFilepath, encrypted);
                FromContainer(container);
                long loadTime = DateTime.Now.Ticks - startTime;
                Debug.Log($"Load game from file {Path.GetFileName(saveFilepath)} (Load Time: {(loadTime / TimeSpan.TicksPerMillisecond):N4}ms)");
                return true;
            } catch (Exception e) {
                Debug.LogError($"Exception loading ConsistentData: : {e.Message}");
                return false;
            }
        }


        public static bool SaveFileExists()
        {
            bool encrypted = !(Application.isEditor || Debug.isDebugBuild);
            string saveBase = encrypted ? _saveBaseEncrypted : _saveBaseRaw;
            long startTime = DateTime.Now.Ticks;
            List<string> allSaves = GetSaveFilesOrderedOldestToNewest(GetSaveDirectory(), saveBase);
            return allSaves.Count > 0;
        }


        public static ConsistentData GetConsistentData()
        {
            ConsistentData saveData = null;
            GameObject saveDataGO = GameObject.FindGameObjectWithTag("SAVE_DATA");
            if (saveDataGO != null) {
                saveData = saveDataGO.GetComponent<ConsistentData>();
            }
            return saveData;
        }


        public static List<string> GetSaveFilesOrderedOldestToNewest(string directoryPath, string saveBase)
        {
            var results = new List<(DateTime time, string path)>();

            if (!Directory.Exists(directoryPath))
                return new List<string>();

            foreach (var filePath in Directory.EnumerateFiles(directoryPath, $"{saveBase}_*.json")) {
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                // Expected format: save_yyyyMMddHHmmss
                if (fileName.Length != $"{saveBase}_{_saveDateFormat}".Length)
                    continue;

                string timestamp = fileName.Substring($"{saveBase}_".Length);

                if (!DateTime.TryParseExact(
                        timestamp,
                        _saveDateFormat,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime fileTime))
                    continue;

                results.Add((fileTime, filePath));
            }

            results.Sort((a, b) => a.time.CompareTo(b.time));

            var orderedPaths = new List<string>(results.Count);
            foreach (var entry in results)
                orderedPaths.Add(entry.path);

            return orderedPaths;
        }


        // Use DataService.LoadData<SaveDataContainer>(dataService, saveFilepath, encryptionEnabled) with the correct type for SaveDataContainer
        /// <summary>
        /// Load a container from file
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="saveFilepath"></param>
        /// <returns></returns>
        protected abstract SaveDataContainer LoadContainer(IDataService dataService, string saveFilepath, bool encryptionEnabled);
        
        
        /// <summary>
        /// Create a container from data and store to a file
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="saveFilepath"></param>
        /// <returns></returns>
        protected abstract bool SaveContainer(IDataService dataService, string saveFilepath, bool encryptionEnabled);
    }
}

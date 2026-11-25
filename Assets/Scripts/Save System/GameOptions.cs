using Shared;
using System;
using System.IO;
using UnityEngine;

[Serializable]
public class GameOptionsContainer
{
    public bool screenBorders;
    public bool crtFilter;
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;

    // Necessary for Newton.json
    public GameOptionsContainer() : base()
    {
    }

    public GameOptionsContainer(GameOptions gameOptions)
    {
        this.screenBorders = gameOptions.screenBorders;
        this.crtFilter = gameOptions.crtFilter;
        this.masterVolume = gameOptions.masterVolume;
        this.musicVolume = gameOptions.musicVolume;
        this.sfxVolume = gameOptions.sfxVolume;
    }
}

namespace Shared
{
    public abstract class GameOptions : Singleton<GameOptions>
    {
        [Tooltip("Filename used to persist the options (placed in the same save directory as ConsistentData)")]
        public string fileName = "game_options.json";

        [Header("Gameplay")]
        public bool screenBorders = true;
        public bool crtFilter = true;

        [Header("Audio")]
        [Range(0f, 100f)] public float masterVolume = 50f;
        [Range(0f, 100f)] public float musicVolume = 50f;
        [Range(0f, 100f)] public float sfxVolume = 50f;

        // Generic version: populate from a container of type T
        protected virtual void FromContainer(GameOptionsContainer data)
        {
            this.screenBorders = data.screenBorders;
            this.crtFilter = data.crtFilter;
            this.masterVolume = data.masterVolume;
            this.musicVolume = data.musicVolume;
            this.sfxVolume = data.sfxVolume;
        }

        private string GetFilePath(string overrideFilename = null)
        {
            string fname = string.IsNullOrEmpty(overrideFilename) ? fileName : overrideFilename;
            string dir = ConsistentData.GetSaveDirectory();
            // Ensure directory exists
            try {
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            } catch { }
            return Path.Join(dir, fname);
        }

        /// <summary>
        /// Save options to a single non-encrypted JSON file. Returns true on success.
        /// Generic overload allows saving a derived container type.
        /// </summary>
        public bool SaveToFile(string overrideFilename = null)
        {
            string path = GetFilePath(overrideFilename);
            IDataService dataService = new JsonDataService();
            try {
                bool ok = SaveContainer(dataService, path);
                if (!ok) Debug.LogError($"Failed to save GameOptions to {path}");
                return ok;
            } catch (Exception e) {
                Debug.LogError($"Exception saving GameOptions: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load options from the JSON file and apply them to this ScriptableObject. Returns true on success.
        /// </summary>
        public bool LoadFromFile(string overrideFilename = null)
        {
            string path = GetFilePath(overrideFilename);
            IDataService dataService = new JsonDataService();
            try {
                GameOptionsContainer container = LoadContainer(dataService, path);
                FromContainer(container);
                return true;
            } catch (Exception e) {
                Debug.LogError($"Exception loading GameOptions: {e.Message}");
                return false;
            }
        }


        // Use DataService.LoadData<GameOptionsContainer>(dataService, saveFilepath) with the correct type for GameOptionsContainer
        /// <summary>
        /// Load a container from file
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="saveFilepath"></param>
        /// <returns></returns>
        protected abstract GameOptionsContainer LoadContainer(IDataService dataService, string saveFilepath);

        /// <summary>
        /// Create a container from data and store to a file
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="saveFilepath"></param>
        /// <returns></returns>
        protected abstract bool SaveContainer(IDataService dataService, string saveFilepath);
    }
}
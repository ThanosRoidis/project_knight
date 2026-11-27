using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;
using Shared;
using Boxerino;

[Serializable]
public class SaveDataContainer : BaseSaveDataContainer
{
    public float timePlayedRealtime = 0f;
    // Necessary for Newton.json
    public SaveDataContainer() : base()
    {

    }

    public SaveDataContainer(BaseConsistentData data) : base(data)
    {
        ConsistentData boxerinoData = (ConsistentData)data;
        this.timePlayedRealtime = boxerinoData.timePlayedRealtime;
    }
}


namespace Boxerino
{
    public class ConsistentData : BaseConsistentData
    {
        public static new ConsistentData Instance { get => (ConsistentData)BaseConsistentData.Instance; }

        // This is the username that will be used for Unity Authentication. It is set to the SteamID if available, otherwise it will be set to the last saved username.
        public float timePlayedRealtime = 0f;


        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

        }

        protected override void FromContainer(BaseSaveDataContainer container)
        {
            SaveDataContainer boxerinoContainer = (SaveDataContainer)container;
            timePlayedRealtime = boxerinoContainer.timePlayedRealtime;

        }

        protected override BaseSaveDataContainer LoadContainer(IDataService dataService, string saveFilepath, bool encryptionEnabled)
        {
            SaveDataContainer container = dataService.LoadData<SaveDataContainer>(saveFilepath, encryptionEnabled);
            return container;
        }

        protected override bool SaveContainer(IDataService dataService, string saveFilepath, bool encryptionEnabled)
        {
            SaveDataContainer data = new SaveDataContainer(this);
            bool ok = dataService.SaveData(saveFilepath, data, encrypted: false);
            return ok;
        }
    }
}
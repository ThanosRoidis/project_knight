using Boxerino;
using Shared;
using System;
using System.IO;


[Serializable]
public class GameOptionsContainer : BaseGameOptionsContainer
{
    public bool smashBoxOnStartup;

    // Necessary for Newton.json
    public GameOptionsContainer() : base()
    {
    }

    public GameOptionsContainer(BaseGameOptions gameOptions) : base(gameOptions)
    {
        GameOptions boxerinoGameOptions = (GameOptions)gameOptions;
        this.smashBoxOnStartup = boxerinoGameOptions.smashBoxOnStartup;
    }
}


namespace Boxerino
{
    public class GameOptions : BaseGameOptions
    {
        public static new GameOptions Instance { get => (GameOptions)BaseGameOptions.Instance; }

        public bool smashBoxOnStartup;

        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        protected override void FromContainer(BaseGameOptionsContainer container)
        {
            GameOptionsContainer boxerinoContainer = (GameOptionsContainer)container;
            smashBoxOnStartup = boxerinoContainer.smashBoxOnStartup;
        }

        protected override BaseGameOptionsContainer LoadContainer(IDataService dataService, string saveFilepath)
        {
            GameOptionsContainer container = dataService.LoadData<GameOptionsContainer>(saveFilepath, false);
            return container;
        }

        protected override bool SaveContainer(IDataService dataService, string saveFilepath)
        {
            GameOptionsContainer data = new GameOptionsContainer(this);
            bool ok = dataService.SaveData(saveFilepath, data, encrypted: false);
            return ok;
        }
    }
}
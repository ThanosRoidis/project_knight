using System;
using UnityEngine;

namespace Shared
{
    public abstract class BaseGameController : Singleton<BaseGameController>
    {
        [SerializeField] protected GameObject initialSaveDataGO;
        protected BaseConsistentData saveData;
        protected BaseGameOptions gameOptions;

        protected bool gameStarted;
        public bool GameStarted { get => gameStarted; }

        protected bool gameEnded;
        public bool GameEnded { get => gameEnded; }

        public bool GameIsOn{ get => GameStarted && !GameEnded; }


        protected float _gameStartTime;

        public float GameTime { get => Time.time - _gameStartTime; }

        protected Action startGameCallback;
        protected Action endGameCallback;


        public void AddToEndGameCallback(Action callback)
        {
            endGameCallback += callback;
        }
        public void AddToStartGameCallback(Action callback)
        {
            startGameCallback += callback;
        }

        protected override void Awake()
        {
            base.Awake();
            gameStarted = false;
            gameEnded = false;
            InitializeSaveData();
        }


        protected void InitializeSaveData()
        {
            GameObject saveDataGO = GameObject.FindGameObjectWithTag("SAVE_DATA");
            if (saveDataGO == null) {
                print("Consistent data not found, creating new...");
                saveDataGO = Instantiate(initialSaveDataGO);
            }
            saveData = saveDataGO.GetComponent<BaseConsistentData>();
            bool loadRes = saveData.LoadData();
            if (!loadRes) {
                print("Creating new initial save data...");
                saveData.SaveData(); // save initial save data
            }
            gameOptions = saveDataGO.GetComponent<BaseGameOptions>();
            loadRes = gameOptions.LoadFromFile();
            if (!loadRes) {
                print("Creating new game options file data...");
                gameOptions.SaveToFile(); // save initial save data
            }

        }

        /// <summary>
        /// Called by countdown timer after it is completed
        /// </summary>
        public virtual void StartGameplay()
        {
            gameStarted = true;
            _gameStartTime = Time.time;
            //AudioController.Instance.PlayStageMusic(saveData.level.stageMusicClip);
            if (startGameCallback != null)
                startGameCallback();
        }

        /// <summary>
        /// Called when player dies. Shows the Results window after a delay.
        /// </summary>
        public virtual void EndGameplay()
        {
            gameEnded = true;
            AudioController.Instance.PlayGameoverClip();
            AudioController.Instance.StopStageMusic();
            if (endGameCallback != null) 
                endGameCallback();
        }


        public virtual void PauseTime()
        {
            Time.timeScale = 0.0f;
            AudioController.Instance.PauseAllSFX();
            AudioController.Instance.PauseStageMusic();
        }


        public virtual void ContinueTime()
        {
            Time.timeScale = 1.0f;
            AudioController.Instance.ContinueAllSFX();
            AudioController.Instance.PlayStageMusic();
        }

        public abstract void LockPlayerControl();
        public abstract void UnlockPlayerControl();


        void OnApplicationQuit()
        {
            Debug.Log("Application ending after " + Time.time + " seconds");
        }
    }
}
using Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shared
{
    public class SceneController : Singleton<SceneController>
    {
        [SerializeField] private bool _fadeIntro = true;
        [SerializeField] private bool _fadeOutro = true;


        [SerializeField] private string _playScene = "Play";
        [SerializeField] private string _mainMenuScene = "Main Menu Scene";
        [SerializeField] private string _multiplayerScene = "Multiplayer Scene";

        // Start is called before the first frame update
        void Start()
        {
            Time.timeScale = 1.0f;
            if (_fadeIntro) {
                FadeImage.Instance.FadeClear();
            }
        }

        public void LoadMainMenuScene()
        {
            LoadScene(_mainMenuScene);
        }

        public void LoadPlayScene()
        {
            LoadScene(_playScene);
        }

        public void LoadMultiplayerScene()
        {
            LoadScene(_multiplayerScene);
        }


        private void LoadScene(string sceneName)
        {
            AudioController.Instance.StopAllSFX();
            FadeImage.Instance.FadeFill();
            FadeImage.Instance.ExecuteOnFill += () => {
                StartCoroutine(LoadSceneCoroutine(sceneName));
            };
        }

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            if (sceneName == "QUIT") {
                QuitApplication();
            } else {
                yield return StartCoroutine(UnloadAdditiveScene());

                AsyncOperation asyncUnload = SceneManager.LoadSceneAsync(sceneName); // Replace with your scene name
                while (!asyncUnload.isDone) {
                    yield return null; // Wait for the next frame
                }
            }
        }


        private IEnumerator UnloadAdditiveScene()
        {
            //TODO: Fix, VERY hacky solution
            HashSet<string> baseScenes = new HashSet<string> { _playScene, _mainMenuScene, "SplashScreen" };

            for (int sceneIdx = 0; sceneIdx < SceneManager.sceneCount; sceneIdx++) {
                Scene scene = SceneManager.GetSceneAt(sceneIdx);
                if (!baseScenes.Contains(scene.name)) {
                    AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(scene.name); // Replace with your scene name
                    while (!asyncUnload.isDone) {
                        yield return null; // Wait for the next frame
                    }
                }
            }
        }


        public void ExitGame()
        {
            LoadScene("QUIT");
        }

        private static void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif


#if UNITY_STANDALONE_WIN
        Application.Quit();
#endif

        }

    }
}
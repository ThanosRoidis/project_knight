using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

namespace Shared
{
    public abstract class Window : MonoBehaviour
    {
        [SerializeField] protected bool freeze;
        [SerializeField] protected bool lockControl;
        [SerializeField] protected GameObject defaultSelection;
        [SerializeField] protected string windowTitle;
        [Tooltip("Close the window when 'Back' is pressed")]
        [SerializeField] protected bool closeOnBack = true;
        [Tooltip("Time after the window is opened until options become interactable")]
        [SerializeField] protected float interactableDelay = 0.3f;
        [SerializeField] private AudioClip closeClip;
        // Is one of the main windows in the main menu
        [SerializeField] private bool isMainWindow;

        /// <summary>
        /// The parent window that opened this window. Will be reopened when this window closes
        /// (see Close(openParentWindow)). Can be set at runtime by windows that are opened dynamically
        /// from different callers.
        /// </summary>
        [SerializeField] protected Window _parentWindow;


        public string WindowTitle { get => windowTitle; }

        protected GameObject _nextSelectionOnOpen;

        private bool _opened;

        public bool Opened { get => _opened; }
        public bool CloseOnBack { get => closeOnBack; set => closeOnBack = value; }

        protected bool _inGameplayScene;
        private bool _interactableInitialized;

        // Start is called before the first frame update
        public virtual void InitializeWindow(bool inGameplayScene)
        {
            _opened = false;
            _inGameplayScene = inGameplayScene;
            _interactableInitialized = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reopen">If you reopen the window, it will reselect it's previous selected option (when it was closed)</param>
        /// <param name="parentWindow">The parent window that opened this window. Will reopen parent window when closed</param>
        public void Open(bool reopen = false)
        {
            transform.SetAsLastSibling();

            // If it is a simple opening of the window, revert to defaults (and keep track who opened it for future reference)
            // Keep this before OnOpen to do stuff with child classes (MainMenuWindow)
            if (!reopen) {
                _nextSelectionOnOpen = defaultSelection;
            }

            if (!_opened) {
                if (_inGameplayScene) {
                    if (GameController.Instance.GameStarted) {
                        if (freeze) {
                            GameController.Instance.PauseTime();
                            UIController.Instance.TransparencyImage.gameObject.SetActive(true);
                        }
                        if (lockControl) {
                            GameController.Instance.LockPlayerControl();
                        }
                    }
                }
                OnOpen();
                _opened = true;
                UIController.Instance.OpenedWindow = this;
            }
            StartCoroutine(GenericUtils.ExecuteDelayed(() => InitializeSelectable(playSound: false), 
                interactableDelay, realtime: true));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="openParentWindow"> Automatically open the window that opened this window (parent window)</param>
        /// <returns></returns>
        public void Close(bool openParentWindow = true)
        {
            bool success = TryClose(openParentWindow);
            if (!success) {
                Debug.Log("Close failed.");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="openParentWindow"> Automatically open the window that opened this window (parent window)</param>
        /// <returns>Whether the window was opened or not (CanOpen())</returns>
        private bool TryClose(bool openParentWindow = true)
        {
            if (!CanClose()) {
                return false;
            }
            _nextSelectionOnOpen = EventSystem.current.currentSelectedGameObject;

            if (_opened) {
                if (_inGameplayScene) {
                    if (freeze) {
                        UIController.Instance.TransparencyImage.gameObject.SetActive(false);
                        GameController.Instance.ContinueTime();
                    }
                    if (lockControl) {
                        GameController.Instance.UnlockPlayerControl();
                    }
                }

                OnClose();
                _opened = false;
                UIController.Instance.OpenedWindow = null;
            }

            AudioController.Instance.PlaySFX(closeClip);
            if (_parentWindow != null && openParentWindow) {
                _parentWindow.Open(reopen: true);
            }
            return true;
        }

        private void InitializeSelectable(bool playSound = true)
        {
            if (_nextSelectionOnOpen != null) {
                if (!playSound) {
                    SoundTriggerUI soundTrigger = _nextSelectionOnOpen.GetComponent<SoundTriggerUI>();
                    if (soundTrigger != null) {
                        soundTrigger.SkipNextSelectionSound();
                    }
                }
                EventSystem.current.firstSelectedGameObject = _nextSelectionOnOpen;
                _nextSelectionOnOpen.GetComponent<UnityEngine.UI.Selectable>().Select();
            }
            _interactableInitialized = true;
        }

        public virtual bool CanClose()
        {
            return _interactableInitialized;
        }

        protected abstract void OnOpen();
        protected abstract void OnClose();

        public abstract void UnfocusChild(GameObject childGO);

    }
}
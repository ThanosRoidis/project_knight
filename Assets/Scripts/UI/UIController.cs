using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Shared
{
    public class UIController : Singleton<UIController>
    {
        [SerializeField] protected bool _inGameplayScene;
        [SerializeField] protected Image transparencyImage;

        protected Window openedWindow;
        public virtual Window OpenedWindow { get => openedWindow; set => openedWindow = value; }
        public Image TransparencyImage { get => transparencyImage; }

        protected List<Window> windows;

        protected PlayerInput controls;

        protected override void Awake()
        {
            base.Awake();
            windows = new List<Window>() { };

            foreach (var window in windows) {
                window.InitializeWindow(_inGameplayScene);
            }


            controls = new PlayerInput();
            controls.UI.Cancel.performed += context => {
                if (OpenedWindow != null && OpenedWindow.CloseOnBack) {
                    OpenedWindow.Close(true);
                }
            };
        }

        public void CloseAllWindows()
        {
            // In case OpenedWindow is not be part of this.windows
            if (openedWindow != null) {
                openedWindow.Close(openParentWindow:false);
            }
            foreach (var window in windows) {
                if(window.Opened)
                    window.Close(openParentWindow: false);
            }
        }

        public void OnEnable()
        {
            controls.Enable();
        }
        public void OnDisable()
        {
            controls.Disable();
        }


        public static Vector2 ChildNormalizedToParentNormalized(RectTransform child, RectTransform parent, Vector2 normalized)
        {
            // Normalized -> child local space (relative to the child's pivot)
            Vector2 childLocal = Rect.NormalizedToPoint(child.rect, normalized);

            // Child local -> world -> parent local space
            Vector3 world = child.TransformPoint(childLocal);
            Vector2 parentLocal = parent.InverseTransformPoint(world);

            // Parent local -> normalized [0,1] of the parent rect
            return Rect.PointToNormalized(parent.rect, parentLocal);
        }
    }
}
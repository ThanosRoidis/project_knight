using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    private enum CrosshairState
    {
        Default,
        Active
    }

    [SerializeField] private Image crosshairImage;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite activeSprite;

    private CrosshairState currentState;

    private void Awake()
    {
        if (crosshairImage == null) {
            crosshairImage = GetComponent<Image>();
        }

        SetState(CrosshairState.Default);
    }

    private void Update()
    {
        PlayerController playerController = PlayerController.Instance;
        bool hasTarget = playerController != null && playerController.HasCurrentTarget;

        SetState(hasTarget ? CrosshairState.Active : CrosshairState.Default);
    }

    private void SetState(CrosshairState state)
    {
        if (currentState == state || crosshairImage == null) return;

        currentState = state;

        switch (currentState)
        {
            case CrosshairState.Active:
                crosshairImage.sprite = activeSprite;
                break;
            default:
                crosshairImage.sprite = defaultSprite;
                break;
        }
    }
}

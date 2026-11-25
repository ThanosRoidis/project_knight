using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractableUI : Shared.Singleton<InteractableUI>
{
    [SerializeField] private TMP_Text interactTextLabel;
    [SerializeField] private Image interactionProgressFill;
    [SerializeField] private Image holdIcon;
    [SerializeField] private CanvasGroup canvasGroup;

    private Interactable currentInteractable;

    protected override void Awake()
    {
        base.Awake();

        if (canvasGroup == null) {
            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null) {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        Hide();
    }

    public void Show(Interactable interactable)
    {
        if (interactable == null) return;

        currentInteractable = interactable;

        bool hasInteractDuration = interactable.HasInteractDuration;

        if (holdIcon != null) {
            holdIcon.gameObject.SetActive(hasInteractDuration);
        }

        if (interactTextLabel != null) {
            interactTextLabel.text = interactable.InteractText;
        }

        if (interactionProgressFill != null) {
            interactionProgressFill.gameObject.SetActive(hasInteractDuration);
        }

        SetVisible(true);
        SetProgress(interactable, interactable.ProgressNormalized);
    }

    public void Hide(Interactable interactable)
    {
        if (currentInteractable != interactable) return;
        Hide();
    }

    public void Hide()
    {
        currentInteractable = null;
        SetProgressRaw(0f);
        SetVisible(false);
    }

    public void SetProgress(Interactable interactable, float normalized)
    {
        if (currentInteractable != interactable) return;
        SetProgressRaw(normalized);
    }

    private void SetProgressRaw(float normalized)
    {
        if (interactionProgressFill == null || !interactionProgressFill.gameObject.activeSelf) return;
        interactionProgressFill.fillAmount = Mathf.Clamp01(normalized);
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;
    }
}

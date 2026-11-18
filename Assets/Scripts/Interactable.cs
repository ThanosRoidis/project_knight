using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] protected string interactText = "Interact";
    [SerializeField] protected float interactDuration;
    [SerializeField] protected bool resetProgressOnStop = true;

    [Header("UI")]
    [SerializeField] protected GameObject interactUI;
    [SerializeField] protected TMP_Text interactTextLabel;
    [SerializeField] protected Image interactionProgressFill;

    private bool isTargeted;
    private bool isInteracting;
    private float interactionElapsed;

    public string InteractText => interactText;
    public bool IsTargeted => isTargeted;
    public bool IsInteracting => isInteracting;
    public float ProgressNormalized => interactDuration <= 0f ? 1f : Mathf.Clamp01(interactionElapsed / interactDuration);

    protected virtual void Awake()
    {
        InitializeUI();
        SetInteractUIVisible(false);
        SetProgressFill(0f);
    }

    protected virtual void Update()
    {
        if (!isInteracting) return;

        if (interactDuration <= 0f) {
            CompleteInteraction();
            return;
        }

        interactionElapsed += Time.deltaTime;
        SetProgressFill(ProgressNormalized);

        if (interactionElapsed >= interactDuration) {
            CompleteInteraction();
        }
    }

    protected virtual void OnDisable()
    {
        isTargeted = false;
        isInteracting = false;
        interactionElapsed = 0f;
        SetInteractUIVisible(false);
        SetProgressFill(0f);
    }

    public void SetTargeted(bool targeted)
    {
        isTargeted = targeted;

        if (!isTargeted) {
            StopInteraction();
        }

        SetInteractUIVisible(isTargeted);
    }

    public void Interact()
    {
        StartInteraction();
    }

    public void StartInteraction()
    {
        if (!isTargeted || isInteracting) return;

        isInteracting = true;

        if (interactDuration <= 0f) {
            CompleteInteraction();
        }
    }

    public void StopInteraction()
    {
        if (!isInteracting && !resetProgressOnStop) return;

        isInteracting = false;

        if (resetProgressOnStop) {
            interactionElapsed = 0f;
            SetProgressFill(0f);
        }
    }

    public abstract void OnInteract();

    private void InitializeUI()
    {
        if (interactTextLabel != null) {
            interactTextLabel.text = interactText;
        }
    }

    private void CompleteInteraction()
    {
        isInteracting = false;
        interactionElapsed = 0f;
        SetProgressFill(1f);
        OnInteract();

        if (isTargeted) {
            SetProgressFill(0f);
        }
    }

    private void SetInteractUIVisible(bool visible)
    {
        if (interactUI != null) {
            interactUI.SetActive(visible);
        }
    }

    private void SetProgressFill(float normalized)
    {
        if (interactionProgressFill != null) {
            interactionProgressFill.fillAmount = Mathf.Clamp01(normalized);
        }
    }
}

using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] protected string interactText = "Interact";
    [SerializeField] protected float interactDuration;
    [SerializeField] protected bool resetProgressOnStop = true;

    private bool isTargeted;
    private bool isInteracting;
    private float interactionElapsed;

    public string InteractText => interactText;
    public bool HasInteractDuration => interactDuration > 0f;
    public bool IsTargeted => isTargeted;
    public bool IsInteracting => isInteracting;
    public float ProgressNormalized => interactDuration <= 0f ? 1f : Mathf.Clamp01(interactionElapsed / interactDuration);

    protected virtual void Awake()
    {
    }

    protected virtual void Update()
    {
        if (!isInteracting) return;

        if (interactDuration <= 0f) {
            CompleteInteraction();
            return;
        }

        interactionElapsed += Time.deltaTime;
        UpdateProgressUI(ProgressNormalized);

        if (interactionElapsed >= interactDuration) {
            CompleteInteraction();
        }
    }

    protected virtual void OnDisable()
    {
        isTargeted = false;
        isInteracting = false;
        interactionElapsed = 0f;

        InteractableUI ui = InteractableUI.Instance;
        if (ui == null) return;

        ui.Hide(this);
        ui.SetProgress(this, 0f);
    }

    public void SetTargeted(bool targeted)
    {
        isTargeted = targeted;

        if (!isTargeted) {
            StopInteraction();
            InteractableUI.Instance?.Hide(this);
            return;
        }

        InteractableUI ui = InteractableUI.Instance;
        if (ui == null) return;

        ui.Show(this);
        ui.SetProgress(this, ProgressNormalized);
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
            UpdateProgressUI(0f);
        }
    }

    public abstract void OnInteract();

    private void CompleteInteraction()
    {
        isInteracting = false;
        interactionElapsed = 0f;
        UpdateProgressUI(1f);
        OnInteract();

        if (isTargeted) {
            UpdateProgressUI(0f);
        }
    }

    private void UpdateProgressUI(float normalized)
    {
        InteractableUI.Instance?.SetProgress(this, normalized);
    }
}

using UnityEngine;

public class TorchInteractable : Interactable
{
    [Header("Torch")]
    [SerializeField] private Light lightSource;
    [SerializeField] private GameObject flameGO;
    [SerializeField] private Collider interactionCollider;
    private Animator animator;

    private const string OnState = "ON";
    private const string OffState = "OFF";

    private bool isOn;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();

        SetTorchState(false, true);
    }
    void LateUpdate()
    {
        // Make the torch face the camera while keeping it upright
        if (isOn) {
            Vector3 cameraPos = Camera.main.transform.position;
            Vector3 targetPosition = new Vector3(
                cameraPos.x,
                flameGO.transform.position.y, // Keeps the torch locked upright
                cameraPos.z
            );

            flameGO.transform.LookAt(targetPosition);
        }
    }

    public override void OnInteract()
    {
        if (isOn) return;

        SetTorchState(true, false);
        SetTargeted(false);

        if (interactionCollider != null) {
            interactionCollider.enabled = false;
        }
    }

    private void SetTorchState(bool enabled, bool instant)
    {
        isOn = enabled;

        if (lightSource != null) {
            lightSource.enabled = enabled;
        }

        if (animator == null) return;

        string targetState = enabled ? OnState : OffState;

        if (instant) {
            animator.Play(targetState, 0, 0f);
            return;
        }

        animator.CrossFadeInFixedTime(targetState, 0.1f, 0);
    }
}

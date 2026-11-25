using System.Collections;
using UnityEngine;

public class DoorInteractable : Interactable
{
    private enum DoorState
    {
        Normal,
        Locked,
        Broken
    }

    [Header("Door State")]
    [SerializeField] private DoorState state = DoorState.Normal;
    [SerializeField] private bool startsOpen;

    [Header("Door Rotation")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float rotationDuration = 0.35f;

    [Header("Door Interaction Text")]
    [SerializeField] private string openDoorText = "Open Door";
    [SerializeField] private string closeDoorText = "Close Door";
    [SerializeField] private string lockedDoorText = "Locked Door";
    [SerializeField] private string repairDoorText = "Repair Door";

    [Header("Door Repair")]
    [SerializeField] private float repairDuration = 1.5f;

    private bool isOpen;
    private bool isRotating;
    private Quaternion closedRotation;
    private Coroutine rotateCoroutine;

    protected override void Awake()
    {
        base.Awake();

        closedRotation = transform.localRotation;
        isOpen = startsOpen;

        if (isOpen) {
            transform.localRotation = GetOpenRotation();
        }

        ApplyState();
    }

    public override void OnInteract()
    {
        if (isRotating) return;

        switch (state) {
            case DoorState.Locked:
                return;
            case DoorState.Broken:
                SetState(DoorState.Normal);
                return;
            case DoorState.Normal:
                ToggleDoor();
                return;
        }
    }

    private void ToggleDoor()
    {
        Quaternion targetRotation = isOpen ? closedRotation : GetOpenRotation();

        if (rotateCoroutine != null) {
            StopCoroutine(rotateCoroutine);
        }

        rotateCoroutine = StartCoroutine(RotateDoorRoutine(targetRotation));
    }

    private IEnumerator RotateDoorRoutine(Quaternion targetRotation)
    {
        isRotating = true;

        Quaternion startRotation = transform.localRotation;

        if (rotationDuration <= 0f) {
            transform.localRotation = targetRotation;
        }
        else {
            float elapsed = 0f;

            while (elapsed < rotationDuration) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / rotationDuration);
                transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }

            transform.localRotation = targetRotation;
        }

        isOpen = !isOpen;
        isRotating = false;
        rotateCoroutine = null;
        UpdateInteractPrompt();
    }

    private Quaternion GetOpenRotation()
    {
        return closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    private void SetState(DoorState newState)
    {
        state = newState;
        ApplyState();
    }

    private void ApplyState()
    {
        switch (state) {
            case DoorState.Normal:
                interactDuration = 0f;
                break;
            case DoorState.Locked:
                interactDuration = 0f;
                break;
            case DoorState.Broken:
                interactDuration = Mathf.Max(0f, repairDuration);
                break;
        }

        UpdateInteractPrompt();
    }

    private void UpdateInteractPrompt()
    {
        switch (state) {
            case DoorState.Normal:
                interactText = isOpen ? closeDoorText : openDoorText;
                break;
            case DoorState.Locked:
                interactText = lockedDoorText;
                break;
            case DoorState.Broken:
                interactText = repairDoorText;
                break;
        }

        if (IsTargeted) {
            InteractableUI.Instance?.Show(this);
        }
    }
}

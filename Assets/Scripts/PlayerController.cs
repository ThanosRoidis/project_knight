using InputHandling;
using NUnit.Framework.Interfaces;
using Shared;
using System.Collections.Generic;
using UnityEngine;
using EventDirection = Shared.Directions.Direction;
using EventType = InputHandling.EventType;
using InputEvent = InputHandling.Event;
using InputSequence = System.Collections.Generic.List<InputHandling.Event>;

public class PlayerController : MonoBehaviour
{
    [Header("Look")]
    [SerializeField] private float lookSensitivity = 15f;

    [Header("Targeting")]
    [SerializeField] private Camera targetingCamera;
    [SerializeField] private RectTransform crosshair;
    [SerializeField] private float targetingDistance = 100f;
    [SerializeField] private LayerMask targetingMask = ~0;

    private PlayerInput playerInput;
    private PlayerInput.PlayerActions input;

    private Actor actor;
    private MovementController movementController;
    private CombatController combatController;

    [Header("Input Handling")]
    [SerializeField] private float attackInputWindow = 0.5f;
    [SerializeField] private float jumpInputWindow = 0.5f;
    private MoveList moveList;
    private InputBuffer inputBuffer;

    [Header("Interaction")]
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private float interactDistance = 2f;
    private Interactable targettedInteractable;

    private bool inputLocked;
    public bool InputLocked
    {
        get => inputLocked;
        set {
            inputLocked = value;
            if (value) {
                inputBuffer.ClearAll();
                //playerMovement.DirectionalInput = Directions.Direction.None;
                input.Disable();
            } else {
                input.Enable();
            }
        }
    }

    void Awake()
    {
        actor = GetComponent<Actor>();
        movementController = GetComponent<MovementController>();
        combatController = GetComponent<CombatController>();
        moveList = new MoveList();


        if (targetingCamera == null) {
            targetingCamera = Camera.main;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


        playerInput = new PlayerInput();
        input = playerInput.Player;
        inputBuffer = new InputBuffer();
        moveList = new MoveList();
        //interactables = new HashSet<Interactable>();
    }

    void Start()
    {
        AssignInputs();
    }

    void Update()
    {
        UpdateCurrentInteractableTarget();

        inputBuffer.Cleanup();
        moveList.ExecuteMoves(inputBuffer);

        combatController.TickCombat(Time.deltaTime);
    }

    void FixedUpdate()
    {
        movementController.SetMovementInput(input.Move.ReadValue<Vector2>());
        movementController.TickMovement(Time.fixedDeltaTime);
    }

    void LateUpdate()
    {
        Vector2 lookInput = input.Look.ReadValue<Vector2>();
        Vector2 lookRotationDelta = lookInput * (lookSensitivity * Time.deltaTime);
        actor.ApplyLookRotation(lookRotationDelta);
    }

    void OnEnable()
    {
        input.Enable();
    }

    void OnDisable()
    {
        input.Disable();
    }

    void AssignInputs()
    {
        input.Attack.performed += ctx => RegisterInputEventToBuffer(EventType.ATTACK, EventPressType.PRESSED);
        input.Jump.performed += ctx => RegisterInputEventToBuffer(EventType.JUMP, EventPressType.PRESSED);
        input.Defend.performed += ctx => RegisterInputEventToBuffer(EventType.DEFEND, EventPressType.PRESSED);
        input.Defend.canceled += ctx => RegisterInputEventToBuffer(EventType.DEFEND, EventPressType.RELEASED);
        input.Interact.started += ctx => RegisterInputEventToBuffer(EventType.INTERACT, EventPressType.PRESSED);
        input.Interact.canceled += ctx => RegisterInputEventToBuffer(EventType.INTERACT, EventPressType.RELEASED);


        Move attack = new Move("Attack press",
                        new List<InputSequence> { new InputSequence { new InputEvent(EventType.ATTACK, EventPressType.PRESSED) } },
                        inputTimeWindow: attackInputWindow,
                        canPerform: _ => combatController.CanAttack(),
                        perform: inputSequence => {
                            combatController.Attack(BuildAttackContextFromCrosshair());
                        },
                        priority: 10);

        Move jump = new Move("Jump press",
                        new List<InputSequence> { new InputSequence { new InputEvent(EventType.JUMP, EventPressType.PRESSED) } },
                        inputTimeWindow: jumpInputWindow,
                        canPerform: _ => movementController.CanJump(),
                        perform: inputSequence => {
                            movementController.Jump();
                        },
                        priority: 10);



        // Skills
        Move interactPressed = new Move("Interact press",
                new List<InputSequence> { new InputSequence { new InputEvent(EventType.INTERACT, EventPressType.PRESSED) } },
                inputTimeWindow: 1f, //TODO: Not hardcoded | High input window
                canPerform: _ => targettedInteractable != null,
                perform: inputSequence => {
                    Interact();
                },
                priority: 6);

        Move interactReleased = new Move("Interact off",
                new List<InputSequence> { new InputSequence { new InputEvent(EventType.INTERACT, EventPressType.RELEASED) } },
                inputTimeWindow: 1, //TODO: Not hardcoded | High input window
                canPerform: _ => targettedInteractable != null,
                perform: _ => {
                    StopInteract();
                },
                priority: 6);


        // Skills
        //Move defendPressed = new Move("Defend on",
        //        new List<InputSequence> { new InputSequence { new InputEvent(EventType.DEFEND, EventPressType.HELD) } },
        //        inputTimeWindow: 1f, //TODO: Not hardcoded | High input window
        //        canPerform: _ => playerCombat.CanCast(playerSkills[1]),
        //        perform: inputSequence => {
        //            playerCombat.Cast(playerSkills[1]);
        //            defendInputPressed = true;
        //            defendInputPressTime = inputSequence[0].eventTime;
        //        },
        //        priority: 6);

        //Move defendReleased = new Move("Defend off",
        //        new List<InputSequence> { new InputSequence { new InputEvent(EventType.DEFEND, EventPressType.RELEASED) } },
        //        inputTimeWindow: 2f, //TODO: Not hardcoded | High input window
        //        canPerform: _ => { return defendInputPressed; },
        //        perform: _ => {
        //            playerSkills[1].OnInputReleased();
        //            defendInputPressed = false;
        //        },
        //        priority: 6,
        //        viableInput: inputSequence => inputSequence[0].eventTime >= defendInputPressTime); // Make sure that the release was after the press


        moveList.AddMove(attack);
        moveList.AddMove(jump);
        moveList.AddMove(interactPressed);
        moveList.AddMove(interactReleased);
    }

    private void RegisterInputEventToBuffer(EventType eventType, EventPressType pressType)
    {
        if (InputLocked) return;
        inputBuffer.RegisterInputEvent(eventType, pressType, EventDirection.None);
    }

    private CombatController.AttackContext BuildAttackContextFromCrosshair()
    {
        Ray ray = GetCrosshairRay();
        Vector3 aimPoint = ray.origin + ray.direction * targetingDistance;

        if (Physics.Raycast(ray, out RaycastHit hit, targetingDistance, targetingMask)) {
            aimPoint = hit.point;
        }

        return combatController.BuildAttackContextFromAimPoint(aimPoint);
    }

    private Ray GetCrosshairRay()
    {
        Camera cameraToUse = targetingCamera != null ? targetingCamera : Camera.main;

        if (cameraToUse == null) {
            return new Ray(actor.LookDirection.position, actor.LookDirection.forward);
        }

        return cameraToUse.ScreenPointToRay(GetCrosshairScreenPosition());
    }

    private Vector2 GetCrosshairScreenPosition()
    {
        if (crosshair != null) {
            return crosshair.position;
        }

        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    private void UpdateCurrentInteractableTarget()
    {
        Ray ray = GetCrosshairRay();
        Interactable nextInteractable = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableMask)) {
            nextInteractable = hit.transform.GetComponentInParent<Interactable>();
        }

        if (targettedInteractable == nextInteractable) return;

        if (targettedInteractable != null) {
            targettedInteractable.SetTargeted(false);
        }

        targettedInteractable = nextInteractable;

        if (targettedInteractable != null) {
            targettedInteractable.SetTargeted(true);
        }
    }

    public void Interact()
    {
        if (targettedInteractable == null) return;
        targettedInteractable.StartInteraction();
    }

    public void StopInteract()
    {
        if (targettedInteractable == null) return;
        targettedInteractable.StopInteraction();
    }

    void OnDestroy()
    {
        if (targettedInteractable != null) {
            targettedInteractable.SetTargeted(false);
            targettedInteractable = null;
        }
    }
}


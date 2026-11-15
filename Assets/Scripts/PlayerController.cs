using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [Header("Camera")]
    public Transform lookDirection;
    public Camera cam;
    public float sensitivity;

    [Header("Animation")]
    public float blendDuration = 0.2f;


    PlayerInput playerInput;
    PlayerInput.MainActions input;

    MovementController movementController;
    CombatController combatController;
    Animator animator;

    float xRotation = 0f;

    void Awake()
    {
        movementController = GetComponent<MovementController>();
        combatController = GetComponent<CombatController>();
        animator = GetComponent<Animator>();

        playerInput = new PlayerInput();
        input = playerInput.Main;
        AssignInputs();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (combatController != null) {
            combatController.attackOrigin = lookDirection;
            combatController.attackDirection = lookDirection;
        }
    }

    void Update()
    {
        // Repeat Inputs
        if (input.Attack.IsPressed()) { Attack(); }

        combatController.TickCombat(Time.deltaTime);

        SetAnimations();
    }

    void FixedUpdate()
    {
        movementController.SetMovementInput(input.Movement.ReadValue<Vector2>());
        movementController.TickMovement(Time.fixedDeltaTime);
    }

    void LateUpdate()
    {
        LookInput(input.Look.ReadValue<Vector2>());
    }

    void LookInput(Vector3 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= (mouseY * Time.deltaTime * sensitivity);
        xRotation = Mathf.Clamp(xRotation, -80, 80);

        lookDirection.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime * sensitivity));
    }

    void OnEnable()
    {
        input.Enable();
    }

    void OnDisable()
    {
        input.Disable();
    }

    void Jump()
    {
        movementController.RequestJump();
    }

    void Attack()
    {
        if (combatController.Attack()) {
            currentAnimationState = string.Empty;
        }
    }

    void AssignInputs()
    {
        input.Jump.performed += ctx => Jump();
        input.Attack.started += ctx => Attack();
    }

    // ---------- //
    // ANIMATIONS //
    // ---------- //

    public const string IDLE = "IDLE";
    public const string WALK = "WALK";

    string currentAnimationState;

    public void ChangeAnimationState(string newState)
    {
        // STOP THE SAME ANIMATION FROM INTERRUPTING WITH ITSELF //
        if (currentAnimationState == newState) return;

        Debug.Log("Play " + newState);
        // PLAY THE ANIMATION //
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, blendDuration, 0);
    }

    void SetAnimations()
    {
        // If player is not attacking
        if (!combatController.IsAttacking) {
            if (!movementController.IsMoving) {
                ChangeAnimationState(IDLE);
            } else {
                ChangeAnimationState(WALK);
            }
        }
    }
}

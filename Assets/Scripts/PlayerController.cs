using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Look")]
    [SerializeField] private float lookSensitivity = 15f;

    private PlayerInput playerInput;
    private PlayerInput.MainActions input;

    private Actor actor;
    private MovementController movementController;
    private CombatController combatController;

    void Awake()
    {
        actor = GetComponent<Actor>();
        movementController = GetComponent<MovementController>();
        combatController = GetComponent<CombatController>();

        playerInput = new PlayerInput();
        input = playerInput.Main;
        AssignInputs();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (input.Attack.IsPressed()) {
            combatController.Attack();
        }

        combatController.TickCombat(Time.deltaTime);
    }

    void FixedUpdate()
    {
        movementController.SetMovementInput(input.Movement.ReadValue<Vector2>());
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
        input.Jump.performed += ctx => movementController.RequestJump();
        input.Attack.started += ctx => combatController.Attack();
    }
}

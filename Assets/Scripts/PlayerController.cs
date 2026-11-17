using UnityEngine;
using UnityEngine.InputSystem;

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

        if (targetingCamera == null) {
            targetingCamera = Camera.main;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (input.Attack.IsPressed()) {
            combatController.Attack(BuildAttackContextFromCrosshair());
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

    private CombatController.AttackContext BuildAttackContextFromCrosshair()
    {
        Ray ray = GetCrosshairRay();
        Vector3 aimPoint = ray.origin + ray.direction * targetingDistance;

        if (Physics.Raycast(ray, out RaycastHit hit, targetingDistance, targetingMask, QueryTriggerInteraction.Ignore)) {
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

    void AssignInputs()
    {
        input.Jump.performed += ctx => movementController.RequestJump();
        input.Attack.started += ctx => combatController.Attack(BuildAttackContextFromCrosshair());
    }
}

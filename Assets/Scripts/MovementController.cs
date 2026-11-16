using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Actor))]
public class MovementController : MonoBehaviour
{
    [System.Serializable]
    private struct MovementParams
    {
        public float speed;
        public float acceleration;
        public float deceleration;
    }

    [Header("Movement")]
    [SerializeField] private MovementParams forwardMovementParams = new MovementParams { speed = 5f, acceleration = 18f, deceleration = 22f };
    [SerializeField] private MovementParams backwardMovementParams = new MovementParams { speed = 4f, acceleration = 16f, deceleration = 20f };
    [SerializeField] private MovementParams strafeMovementParams = new MovementParams { speed = 4.5f, acceleration = 17f, deceleration = 21f };
    [SerializeField] private MovementParams attackingMovementParams = new MovementParams { speed = 2.75f, acceleration = 12f, deceleration = 16f };

    [Header("Controller")]
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpHeight = 1.2f;

    private CharacterController controller;
    private Actor actor;
    private CombatController combatController;
    private Vector3 playerVelocity;
    private Vector3 movementInput;
    private Vector3 currentLocalVelocity;
    private bool jumpRequested;

    public bool IsMoving => currentLocalVelocity.sqrMagnitude > 0.001f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        actor = GetComponent<Actor>();
        combatController = GetComponent<CombatController>();
    }

    public void SetMovementInput(Vector2 input)
    {
        movementInput.x = input.x;
        movementInput.y = 0f;
        movementInput.z = input.y;
    }

    public void RequestJump()
    {
        jumpRequested = true;
    }

    public void TickMovement(float deltaTime)
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && playerVelocity.y < 0f) {
            playerVelocity.y = -2f;
        }

        if (jumpRequested && isGrounded) {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }

        jumpRequested = false;

        MovementParams activeMovementParams = GetActiveMovementParams();
        Vector3 targetLocalVelocity = GetTargetLocalVelocity(activeMovementParams);

        float movementDelta = movementInput.sqrMagnitude > 0.0001f
            ? activeMovementParams.acceleration
            : activeMovementParams.deceleration;

        currentLocalVelocity = Vector3.MoveTowards(currentLocalVelocity, targetLocalVelocity, movementDelta * deltaTime);
        controller.Move(transform.TransformDirection(currentLocalVelocity) * deltaTime);

        playerVelocity.y += gravity * deltaTime;
        controller.Move(playerVelocity * deltaTime);

        actor.UpdateLocomotionAnimation(IsMoving);
    }

    private MovementParams GetActiveMovementParams()
    {
        if (combatController != null && combatController.IsAttacking) {
            return attackingMovementParams;
        }

        float absForward = Mathf.Abs(movementInput.z);
        float absStrafe = Mathf.Abs(movementInput.x);

        if (absForward >= absStrafe && movementInput.z > 0f) {
            return forwardMovementParams;
        }

        if (absForward >= absStrafe && movementInput.z < 0f) {
            return backwardMovementParams;
        }

        return strafeMovementParams;
    }

    private Vector3 GetTargetLocalVelocity(MovementParams activeMovementParams)
    {
        if (activeMovementParams.speed <= 0f) {
            return Vector3.zero;
        }

        float targetX = movementInput.x * activeMovementParams.speed;
        float targetZ = movementInput.z * activeMovementParams.speed;

        return new Vector3(targetX, 0f, targetZ);
    }
}

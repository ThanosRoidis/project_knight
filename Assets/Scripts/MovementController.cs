using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Actor))]
public class MovementController : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpHeight = 1.2f;

    private CharacterController controller;
    private Actor actor;
    private Vector3 playerVelocity;
    private Vector3 movementInput;
    private bool jumpRequested;

    public bool IsMoving => movementInput.sqrMagnitude > 0.001f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        actor = GetComponent<Actor>();
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

        controller.Move(transform.TransformDirection(movementInput) * moveSpeed * deltaTime);

        playerVelocity.y += gravity * deltaTime;
        controller.Move(playerVelocity * deltaTime);

        actor.UpdateLocomotionAnimation(IsMoving);
    }
}

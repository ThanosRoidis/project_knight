using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class Actor : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private GameObject hitEffect;

    [Header("Body")]
    [SerializeField] private float height = 2f;
    [SerializeField] private bool updateCenter = true;

    [Header("Look")]
    [SerializeField] private Transform lookDirection;
    [SerializeField] private float minLookPitch = -80f;
    [SerializeField] private float maxLookPitch = 80f;


    [Header("Animation")]
    [SerializeField] private float blendDuration = 0.2f;

    private const string IDLE = "IDLE";
    private const string WALK = "WALK";

    private AudioSource audioSource;
    private Animator animator;
    private int currentHealth;

    private string currentAnimationState;
    private float xRotation;
    private bool attacking;

    public Transform LookDirection => lookDirection != null ? lookDirection : transform;

    public float Height
    {
        get => height;
        set {
            height = Mathf.Max(0.01f, value);
            ApplyHeight();
        }
    }

    void OnValidate()
    {
        height = Mathf.Max(0.01f, height);
        ApplyHeight();
    }

    private void ApplyHeight()
    {
        if (TryGetComponent(out CharacterController characterController)) {
            characterController.height = height;

            if (updateCenter) {
                Vector3 center = characterController.center;
                center.y = height * 0.5f;
                characterController.center = center;
            }
        }

        if (TryGetComponent(out NavMeshAgent navMeshAgent)) {
            navMeshAgent.height = height;
        }
    }

    void Awake()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        ApplyHeight();

        if (lookDirection == null) {
            lookDirection = transform;
        }
    }

    public void SetLookDirection(Transform lookTarget)
    {
        if (lookTarget != null) {
            lookDirection = lookTarget;
        }
    }

    public void ApplyLookRotation(Vector2 rotationDelta)
    {
        xRotation -= rotationDelta.y;
        xRotation = Mathf.Clamp(xRotation, minLookPitch, maxLookPitch);

        Vector3 lookEuler = lookDirection.localEulerAngles;
        lookEuler.x = xRotation;
        lookDirection.localEulerAngles = lookEuler;

        transform.Rotate(Vector3.up * rotationDelta.x);
    }

    public void SetAttacking(bool isAttacking)
    {
        attacking = isAttacking;

        if (!attacking) {
            currentAnimationState = string.Empty;
        }
    }

    public void PlayAttackAnimation(string stateName)
    {
        ChangeAnimationState(stateName);
    }

    public void UpdateLocomotionAnimation(bool isMoving)
    {
        if (attacking) return;

        ChangeAnimationState(isMoving ? WALK : IDLE);
    }

    public void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;

        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, blendDuration, 0);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0) { Die(); }
    }

    public void GetHit(Vector3 pos, int damage)
    {
        TakeDamage(damage);

        if (audioSource != null && hitSound != null) {
            audioSource.pitch = 1;
            audioSource.PlayOneShot(hitSound);
        }

        if (hitEffect != null) {
            GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity);
            Destroy(GO, 20);
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}

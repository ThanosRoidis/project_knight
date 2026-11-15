using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Animator))]
public class CombatController : MonoBehaviour
{
    [Header("Attacking")]
    public float attackDistance = 2f;
    public float attackDuration = 0.5833333f;
    public int attackDamage = 1;
    public float attackDamageDelay = 0.2f;
    public float attackResetDelay = 0.1f;
    public LayerMask attackLayer;
    public float blendDuration = 0.1f;

    public Transform attackOrigin;
    public Transform attackDirection;
    public AudioClip swordSwing;

    public bool IsAttacking => attacking;

    public const string ATTACK1 = "ATTACK_1";
    public const string ATTACK2 = "ATTACK_2";

    private Animator animator;
    private AudioSource audioSource;

    private bool attacking = false;
    private bool readyToAttack = true;
    private int attackCount;

    private float attackResetTimer = 0.1f;

    private string currentAnimationState;

    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (attackOrigin == null) {
            attackOrigin = transform;
        }

        if (attackDirection == null) {
            attackDirection = transform;
        }
    }

    public void TickCombat(float deltaTime)
    {
        attackResetTimer -= deltaTime;
        if (!attacking && attackResetTimer <= 0f) {
            attackCount = 0;
            attackResetTimer = Mathf.Infinity;
        }
    }

    public bool Attack()
    {
        if (!readyToAttack || attacking) return false;

        readyToAttack = false;
        attacking = true;

        Invoke(nameof(ResetAttack), attackDuration);
        Invoke(nameof(AttackRaycast), attackDamageDelay);

        if (swordSwing != null) {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(swordSwing);
        }

        if (attackCount == 0) {
            ChangeAnimationState(ATTACK1);
            attackCount = 1;
        } else {
            ChangeAnimationState(ATTACK2);
            attackCount = 0;
        }

        return true;
    }

    void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
        attackResetTimer = attackResetDelay;
        currentAnimationState = string.Empty;
    }

    void AttackRaycast()
    {
        if (Physics.Raycast(attackOrigin.position, attackDirection.forward, out RaycastHit hit, attackDistance, attackLayer)) {
            Actor enemyActor = hit.transform.GetComponentInParent<Actor>();
            if (enemyActor != null) {
                enemyActor.GetHit(hit.point, attackDamage);
            }
        }
    }

    void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;

        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, blendDuration, 0);
    }
}

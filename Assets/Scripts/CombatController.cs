using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Actor))]
public class CombatController : MonoBehaviour
{
    [Header("Attacking")]
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private float attackDuration = 0.5833333f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackDamageDelay = 0.2f;
    [SerializeField] private float attackResetDelay = 0.1f;
    [SerializeField] private LayerMask attackLayer;

    [Header("References")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private Transform attackDirection;
    [SerializeField] private AudioClip swordSwing;

    public bool IsAttacking => attacking;

    private const string ATTACK1 = "ATTACK_1";
    private const string ATTACK2 = "ATTACK_2";

    private Actor actor;
    private AudioSource audioSource;

    private bool attacking = false;
    private bool readyToAttack = true;
    private int attackCount;

    private float attackResetTimer = 0.1f;

    void Awake()
    {
        actor = GetComponent<Actor>();
        audioSource = GetComponent<AudioSource>();

        if (attackOrigin == null) {
            attackOrigin = actor.LookDirection;
        }

        if (attackDirection == null) {
            attackDirection = actor.LookDirection;
        }
        attackOrigin = actor.LookDirection;
        attackDirection = actor.LookDirection;
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
        actor.SetAttacking(true);

        Invoke(nameof(ResetAttack), attackDuration);
        Invoke(nameof(AttackRaycast), attackDamageDelay);

        if (swordSwing != null) {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(swordSwing);
        }

        if (attackCount == 0) {
            actor.PlayAttackAnimation(ATTACK1);
            attackCount = 1;
        } else {
            actor.PlayAttackAnimation(ATTACK2);
            attackCount = 0;
        }

        return true;
    }

    void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
        attackResetTimer = attackResetDelay;
        actor.SetAttacking(false);
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
}

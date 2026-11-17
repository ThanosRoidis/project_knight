using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Actor))]
public class CombatController : MonoBehaviour
{
    public struct AttackContext
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public AttackContext(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }
    }

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
    private AttackContext currentAttackContext;

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

    public AttackContext BuildAttackContextFromAimPoint(Vector3 worldAimPoint)
    {
        Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position;
        Vector3 fallbackDirection = attackDirection != null ? attackDirection.forward : transform.forward;
        Vector3 direction = worldAimPoint - origin;

        if (direction.sqrMagnitude <= 0.0001f) {
            direction = fallbackDirection;
        }

        return new AttackContext(origin, direction.normalized);
    }

    public bool Attack()
    {
        Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position;
        Vector3 direction = attackDirection != null ? attackDirection.forward : transform.forward;
        return Attack(new AttackContext(origin, direction));
    }

    public bool Attack(AttackContext attackContext)
    {
        if (!readyToAttack || attacking) return false;

        if (attackContext.Direction.sqrMagnitude <= 0.0001f) {
            attackContext.Direction = attackDirection != null ? attackDirection.forward : transform.forward;
        }

        currentAttackContext = new AttackContext(attackContext.Origin, attackContext.Direction.normalized);

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
        if (Physics.Raycast(currentAttackContext.Origin, currentAttackContext.Direction, out RaycastHit hit, attackDistance, attackLayer)) {
            Actor enemyActor = hit.transform.GetComponentInParent<Actor>();
            if (enemyActor != null) {
                enemyActor.GetHit(hit.point, attackDamage);
            }
        }
    }
}

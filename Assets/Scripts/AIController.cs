using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(CombatController))]
[RequireComponent(typeof(Actor))]
public class AIController : MonoBehaviour
{
    private enum AIState
    {
        Idle,
        Alert,
        Retreat
    }

    [Header("Perception")]
    [SerializeField] private float hearingRadius = 6f;
    [SerializeField] private float sightRange = 15f;
    [SerializeField] private float sightAngle = 90f;
    [SerializeField] private float perceptionInterval = 0.2f;
    [SerializeField] private float eyeHeight = 1.6f;
    [SerializeField] private float targetHeight = 1.0f;
    [SerializeField] private LayerMask obstacleMask = ~0;

    [Header("Combat")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDelay = 1f;

    [Header("Retreat")]
    [SerializeField] private float leashDistance = 20f;
    [SerializeField] private float retreatReengageRadius = 8f;
    [SerializeField] private float spawnReachedDistance = 0.5f;

    [Header("Movement")]
    [SerializeField] private float turnSpeed = 540f;
    [SerializeField] private float repathInterval = 0.2f;
    [SerializeField] private float waypointReachedDistance = 0.3f;

    private NavMeshAgent agent;
    private MovementController movementController;
    private CombatController combatController;
    private Actor actor;

    private Transform player;
    private AIState state = AIState.Idle;
    private Vector3 spawnPosition;
    private float perceptionTimer;
    private float repathTimer;
    private float lastAttackTime = float.NegativeInfinity;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        movementController = GetComponent<MovementController>();
        combatController = GetComponent<CombatController>();
        actor = GetComponent<Actor>();

        // The agent only plans the path; the MovementController performs the actual movement.
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    void Start()
    {
        spawnPosition = transform.position;

        if (player == null) {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }
    }

    void Update()
    {
        combatController.TickCombat(Time.deltaTime);

        if (player == null) return;

        switch (state) {
            case AIState.Idle:
                UpdateIdle();
                break;
            case AIState.Alert:
                UpdateAlert();
                break;
            case AIState.Retreat:
                UpdateRetreat();
                break;
        }
    }

    void FixedUpdate()
    {
        movementController.TickMovement(Time.fixedDeltaTime);

        if (agent.isOnNavMesh) {
            agent.nextPosition = transform.position;
        }
    }

    private void UpdateIdle()
    {
        movementController.SetMovementInput(Vector2.zero);

        perceptionTimer -= Time.deltaTime;
        if (perceptionTimer > 0f) return;
        perceptionTimer = perceptionInterval;

        if (PerceptionCheck()) {
            EnterAlert();
        }
    }

    private void EnterAlert()
    {
        state = AIState.Alert;
        repathTimer = 0f;
    }

    private void UpdateRetreat()
    {
        float playerToSpawn = Vector3.Distance(player.position, spawnPosition);
        float playerToSelf = Vector3.Distance(player.position, transform.position);

        if (playerToSpawn <= leashDistance && playerToSelf <= retreatReengageRadius) {
            EnterAlert();
            return;
        }

        Vector3 toSpawn = spawnPosition - transform.position;
        toSpawn.y = 0f;

        if (toSpawn.magnitude <= spawnReachedDistance) {
            movementController.SetMovementInput(Vector2.zero);
            if (agent.isOnNavMesh && agent.hasPath) agent.ResetPath();
            state = AIState.Idle;
            perceptionTimer = 0f;
            return;
        }

        MoveAlongPath(spawnPosition);
    }

    private void UpdateAlert()
    {
        if (Vector3.Distance(player.position, spawnPosition) > leashDistance) {
            state = AIState.Retreat;
            repathTimer = 0f;
            return;
        }

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;

        if (distance > attackRange) {
            MoveAlongPath(player.position);
            return;
        }

        movementController.SetMovementInput(Vector2.zero);
        if (agent.isOnNavMesh && agent.hasPath) {
            agent.ResetPath();
        }

        RotateTowards(toPlayer);

        Vector3 attackTargetPoint = player.position + Vector3.up * targetHeight;
        CombatController.AttackContext attackContext = combatController.BuildAttackContextFromAimPoint(attackTargetPoint);

        if (Time.time - lastAttackTime >= attackDelay && combatController.CanAttack()) {
            combatController.Attack(attackContext);
            lastAttackTime = Time.time;
        }
    }

    private void MoveAlongPath(Vector3 destination)
    {
        if (!agent.isOnNavMesh) {
            movementController.SetMovementInput(Vector2.zero);
            return;
        }

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f || !agent.hasPath) {
            repathTimer = repathInterval;
            agent.SetDestination(destination);
        }

        Vector3 direction = agent.steeringTarget - transform.position;
        direction.y = 0f;

        if (agent.pathPending || direction.magnitude < waypointReachedDistance) {
            movementController.SetMovementInput(Vector2.zero);
            return;
        }

        RotateTowards(direction);
        movementController.SetMovementInput(Vector2.up);
    }

    private void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    private bool PerceptionCheck()
    {
        return CanHear() || CanSee();
    }

    private bool CanHear()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= hearingRadius && HasClearLineTo();
    }

    private bool CanSee()
    {
        Vector3 toPlayer = player.position - transform.position;
        if (toPlayer.magnitude > sightRange) return false;

        Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        if (Vector3.Angle(transform.forward, flat) > sightAngle * 0.5f) return false;

        return HasClearLineTo();
    }

    private bool HasClearLineTo()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * targetHeight;
        Vector3 direction = target - origin;

        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, direction.magnitude, obstacleMask, QueryTriggerInteraction.Ignore)) {
            return hit.transform == player || hit.transform.IsChildOf(player);
        }

        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);

        Gizmos.color = Color.green;
        Vector3 left = Quaternion.Euler(0f, -sightAngle * 0.5f, 0f) * transform.forward;
        Vector3 right = Quaternion.Euler(0f, sightAngle * 0.5f, 0f) * transform.forward;
        Gizmos.DrawRay(transform.position, left * sightRange);
        Gizmos.DrawRay(transform.position, right * sightRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, leashDistance);
    }
}

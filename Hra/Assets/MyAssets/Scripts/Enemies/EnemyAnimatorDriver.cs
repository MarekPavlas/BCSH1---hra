using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimatorDriver : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;
    public Transform player;
    public EnemyHealth health;
    public EnemyMeleeDamage meleeDamage;
    public NavMeshAgent agent;

    [Header("Animator Params")]
    public string isMovingParam = "IsMoving";
    public string attackParam = "Attack";
    public string hitParam = "Hit";
    public string deadParam = "Dead";

    [Header("Tuning")]
    public float moveThreshold = 0.05f;
    public float attackRangePadding = 0.1f;
    public float minDamageToTriggerHit = 0.01f;
    public bool debugLogs = false;

    float lastHealth;
    float nextAttackAnimTime;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (health == null)
            health = GetComponent<EnemyHealth>();

        if (meleeDamage == null)
            meleeDamage = GetComponent<EnemyMeleeDamage>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        TryFindPlayer();

        if (health != null)
            lastHealth = health.currentHealth;
    }

    void Update()
    {
        if (animator == null)
            return;

        if (player == null)
            TryFindPlayer();

        bool isDead = health != null && health.currentHealth <= 0f;
        animator.SetBool(deadParam, isDead);

        if (isDead)
        {
            animator.SetBool(isMovingParam, false);
            return;
        }

        UpdateMovingState();
        UpdateHitState();
        UpdateAttackState();
    }

    void TryFindPlayer()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
            player = go.transform;
    }

    void UpdateMovingState()
    {
        bool isMoving = false;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            isMoving = agent.velocity.sqrMagnitude > moveThreshold * moveThreshold;

        animator.SetBool(isMovingParam, isMoving);
    }

    void UpdateHitState()
    {
        if (health == null)
            return;

        if (health.currentHealth < lastHealth - minDamageToTriggerHit)
        {
            animator.SetTrigger(hitParam);

            if (debugLogs)
                Debug.Log($"[EnemyAnimatorDriver] {name} HIT trigger");
        }

        lastHealth = health.currentHealth;
    }

    void UpdateAttackState()
    {
        if (player == null || meleeDamage == null)
            return;

        if (Time.time < nextAttackAnimTime)
            return;

        float attackRange = Mathf.Max(0.1f, meleeDamage.hitRadius + attackRangePadding);
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > attackRange)
            return;

        if (meleeDamage.requireLineOfSight && !HasLineOfSightToPlayer())
            return;

        animator.SetTrigger(attackParam);
        nextAttackAnimTime = Time.time + Mathf.Max(0.05f, meleeDamage.hitInterval);

        if (debugLogs)
            Debug.Log($"[EnemyAnimatorDriver] {name} ATTACK trigger");
    }

    bool HasLineOfSightToPlayer()
    {
        if (player == null || meleeDamage == null)
            return false;

        Vector3 origin = transform.position + Vector3.up * meleeDamage.aimHeightOffset;
        Vector3 target = player.position + Vector3.up * meleeDamage.aimHeightOffset;
        Vector3 dir = target - origin;
        float dist = dir.magnitude;

        if (dist <= 0.01f)
            return true;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, meleeDamage.lineOfSightMask, QueryTriggerInteraction.Ignore))
            return hit.transform == player || hit.transform.IsChildOf(player);

        return true;
    }
}
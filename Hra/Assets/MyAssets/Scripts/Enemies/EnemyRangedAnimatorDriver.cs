using UnityEngine;
using UnityEngine.AI;

public class EnemyRangedAnimatorDriver : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;
    public Transform player;
    public EnemyHealth health;
    public EnemyRangedAI rangedAI;
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
    public bool useFlatDistance = true;
    public bool debugLogs = false;

    float lastHealth;
    float nextAttackAnimTime;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (health == null)
            health = GetComponent<EnemyHealth>();

        if (rangedAI == null)
            rangedAI = GetComponent<EnemyRangedAI>();

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
        if (!CanUseAnimator())
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

    bool CanUseAnimator()
    {
        return animator != null
            && animator.isActiveAndEnabled
            && animator.runtimeAnimatorController != null
            && animator.isInitialized;
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
                Debug.Log($"[EnemyRangedAnimatorDriver] {name} HIT trigger");
        }

        lastHealth = health.currentHealth;
    }

    void UpdateAttackState()
    {
        if (player == null || rangedAI == null)
            return;

        if (Time.time < nextAttackAnimTime)
            return;

        float attackRange = Mathf.Max(0.1f, rangedAI.shootRange + attackRangePadding);
        float dist = GetDistanceToPlayer();

        if (dist > attackRange)
            return;

        if (rangedAI.requireLineOfSight && !HasLineOfSightToPlayer())
            return;

        animator.SetTrigger(attackParam);
        nextAttackAnimTime = Time.time + Mathf.Max(0.05f, rangedAI.fireInterval);

        if (debugLogs)
            Debug.Log($"[EnemyRangedAnimatorDriver] {name} ATTACK trigger");
    }

    float GetDistanceToPlayer()
    {
        if (player == null)
            return float.PositiveInfinity;

        Vector3 a = transform.position;
        Vector3 b = player.position;

        if (useFlatDistance)
        {
            a.y = 0f;
            b.y = 0f;
        }

        return Vector3.Distance(a, b);
    }

    bool HasLineOfSightToPlayer()
    {
        if (player == null || rangedAI == null)
            return false;

        Vector3 origin = transform.position + Vector3.up * rangedAI.aimHeightOffset;
        Vector3 target = player.position + Vector3.up * rangedAI.aimHeightOffset;
        Vector3 dir = target - origin;
        float dist = dir.magnitude;

        if (dist <= 0.01f)
            return true;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, rangedAI.lineOfSightMask, QueryTriggerInteraction.Ignore))
            return hit.transform == player || hit.transform.IsChildOf(player);

        return true;
    }
}

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 3.5f;

    public float repathInterval = 0.1f;

    public float repathMoveThreshold = 0.25f;

    public float stopRepathWithinDistance = 0f;

    [Header("Debug")]
    public bool debugLogs = false;

    private NavMeshAgent agent;
    private float nextRepathTime;
    private Vector3 lastTargetPos;
    private bool hasLastTarget;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        ApplySpeed();

        if (player != null)
        {
            lastTargetPos = player.position;
            hasLastTarget = true;

            if (agent.isOnNavMesh)
                agent.SetDestination(lastTargetPos);
        }
    }

    void Update()
    {
        if (!Mathf.Approximately(agent.speed, moveSpeed))
            ApplySpeed();

        if (player == null) return;

        if (Time.time < nextRepathTime) return;
        nextRepathTime = Time.time + repathInterval;

        if (!agent.isOnNavMesh)
        {
            if (debugLogs)
                Debug.LogWarning($"[EnemyAI] {name} není na NavMesh (isOnNavMesh=false).");
            return;
        }

        if (stopRepathWithinDistance > 0f)
        {
            float d = Vector3.Distance(transform.position, player.position);
            if (d <= stopRepathWithinDistance) return;
        }

        Vector3 ppos = player.position;

        if (!hasLastTarget)
        {
            lastTargetPos = ppos;
            hasLastTarget = true;
            agent.SetDestination(ppos);
            return;
        }

        float thr = Mathf.Max(0.01f, repathMoveThreshold);
        if ((ppos - lastTargetPos).sqrMagnitude >= thr * thr)
        {
            lastTargetPos = ppos;
            agent.SetDestination(ppos);

            if (debugLogs)
                Debug.Log($"[EnemyAI] {name} repath -> target moved (thr={thr:0.00})");
        }
    }

    private void ApplySpeed()
    {
        moveSpeed = Mathf.Max(0.1f, moveSpeed);
        agent.speed = moveSpeed;

        if (debugLogs)
            Debug.Log($"[EnemyAI] {name} speed set to {agent.speed:0.00}");
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
        ApplySpeed();
    }

    public void AddMoveSpeed(float add)
    {
        moveSpeed += add;
        ApplySpeed();
    }

    public void MultiplyMoveSpeed(float mul)
    {
        moveSpeed *= mul;
        ApplySpeed();
    }
}
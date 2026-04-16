using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyRangedAI : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;                 
    public Transform shootPoint;             
    public EnemyProjectile projectilePrefab; 

    [Header("Activation")]
    public bool useDetectRange = true;
    public float detectRange = 20f;

    [Header("Chase")]
    public float stopDistance = 10f;
    public float keepAwayDistance = 6f;
    public float retreatStep = 2.5f;

    [Header("Shooting")]
    public float shootRange = 15f;
    public float fireInterval = 1.2f;
    public float projectileSpeed = 18f;
    public float projectileDamage = 8f;
    public float aimHeightOffset = 1.0f;

    public float muzzleForwardOffset = 0.4f;

    public bool useFlatDistance = true;

    [Header("Line of sight")]
    public bool requireLineOfSight = false;
    public LayerMask lineOfSightMask = ~0;

    [Header("NavMesh safety")]
    public float snapToNavMeshMaxDistance = 5f;
    public float destinationUpdateInterval = 0.15f;

    [Header("Debug")]
    public bool debugLogs = false;

    NavMeshAgent agent;
    float nextFire;
    float nextDestUpdate;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
    }

    void OnEnable()
    {
        ResolveRefs();
        TrySnapToNavMesh();
        agent.stoppingDistance = Mathf.Max(0f, stopDistance);
    }

    void Start()
    {
        ResolveRefs();
        TrySnapToNavMesh();
        agent.stoppingDistance = Mathf.Max(0f, stopDistance);
    }

    void ResolveRefs()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        if (shootPoint == null) shootPoint = transform;
    }

    bool TrySnapToNavMesh()
    {
        if (agent == null || !agent.enabled) return false;
        if (agent.isOnNavMesh) return true;

        if (NavMesh.SamplePosition(transform.position, out var hit, snapToNavMeshMaxDistance, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            return true;
        }
        return false;
    }

    void Update()
    {
        ResolveRefs();
        if (player == null) return;

        if (!agent.isOnNavMesh)
        {
            if (!TrySnapToNavMesh()) return;
        }

        float dist = GetDistanceToPlayer();

        if (useDetectRange && dist > detectRange)
        {
            SafeStop(true);
            return;
        }

        HandleMovement(dist);

        if (dist <= shootRange && Time.time >= nextFire)
        {
            if (!requireLineOfSight || HasLineOfSightIgnoreSelf())
            {
                Shoot();
                nextFire = Time.time + fireInterval;
            }
            else
            {
                if (debugLogs) Debug.Log("[EnemyRangedAI] No line of sight -> not shooting");
            }
        }
        else
        {
            if (debugLogs && dist <= shootRange)
                Debug.Log("[EnemyRangedAI] Waiting for fireInterval...");
        }
    }

    float GetDistanceToPlayer()
    {
        if (!useFlatDistance) return Vector3.Distance(transform.position, player.position);

        Vector3 a = transform.position; a.y = 0f;
        Vector3 b = player.position; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void HandleMovement(float dist)
    {
        if (dist > stopDistance)
        {
            SafeStop(false);

            if (Time.time >= nextDestUpdate)
            {
                nextDestUpdate = Time.time + destinationUpdateInterval;
                agent.SetDestination(player.position);
            }
            return;
        }

        SafeStop(true);

        if (keepAwayDistance > 0f && dist < keepAwayDistance)
        {
            Vector3 away = transform.position - player.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.001f) away = transform.forward;
            away.Normalize();

            Vector3 retreatPos = transform.position + away * retreatStep;
            if (NavMesh.SamplePosition(retreatPos, out var hit, 3f, NavMesh.AllAreas))
            {
                SafeStop(false);
                agent.SetDestination(hit.position);
            }
        }
    }

    void SafeStop(bool stop)
    {
        if (!agent.enabled || !agent.isOnNavMesh) return;
        agent.isStopped = stop;
        if (stop) agent.ResetPath();
    }

    bool HasLineOfSightIgnoreSelf()
    {
        Vector3 origin = shootPoint.position;
        Vector3 target = player.position + Vector3.up * aimHeightOffset;
        Vector3 dir = target - origin;
        float dist = dir.magnitude;

        if (dist <= 0.01f) return true;

        Ray ray = new Ray(origin, dir.normalized);
        var hits = Physics.RaycastAll(ray, dist, lineOfSightMask, QueryTriggerInteraction.Ignore);

        if (hits == null || hits.Length == 0) return true;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            if (h.transform == null) continue;

            if (h.transform.root == transform) continue;

            return (h.transform == player || h.transform.IsChildOf(player));
        }

        return true;
    }

    void Shoot()
    {
        if (projectilePrefab == null)
        {
            if (debugLogs) Debug.LogWarning("[EnemyRangedAI] projectilePrefab is NULL (nepřiřazeno v inspectoru)");
            return;
        }

        Vector3 origin = shootPoint.position;

        Vector3 target = player.position + Vector3.up * aimHeightOffset;
        Vector3 dir = target - origin;
        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;
        dir.Normalize();

        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
        if (flatDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(flatDir.normalized, Vector3.up);

        Vector3 spawnPos = origin + dir * muzzleForwardOffset;

        var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir, Vector3.up));
        proj.Init(dir, projectileSpeed, projectileDamage, owner: transform);

        if (debugLogs) Debug.Log("[EnemyRangedAI] Shoot OK");
    }
}
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeamWeapon : MonoBehaviour, IWeaponLevelApplier
{
    [Header("Refs")]
    public Transform firePoint;
    public LineRenderer line;
    public PlayerStats stats;

    [Header("Base stats")]
    public float baseRange = 25f;
    public float baseDamagePerSecond = 20f;
    public float targetHeightOffset = 1f;

    [Header("Targeting")]
    public string enemyTag = "Enemy";
    public LayerMask enemyMask = ~0;
    public LayerMask obstacleMask = ~0;
    public float baseRetargetInterval = 0.2f;
    public float aimSmoothing = 18f;

    [Header("Visual")]
    [Min(0.001f)] public float beamWidth = 0.05f;
    public bool alwaysShowToHit = true;
    public bool debugLogs = false;

    float currentDamagePerSecond;
    float currentRange;
    float currentRetargetInterval;

    Transform currentTarget;
    float nextRetargetTime;

    Vector3 smoothedEnd;
    bool hasSmoothed;

    float lastBeamWidth = -1f;

    void Awake()
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        if (stats == null)
            stats = GetComponent<PlayerStats>();

        currentDamagePerSecond = baseDamagePerSecond;
        currentRange = baseRange;
        currentRetargetInterval = baseRetargetInterval;

        if (line != null)
        {
            line.positionCount = 2;
            line.enabled = false;
        }

        ApplyBeamWidth(true);
    }

    void OnValidate()
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        ApplyBeamWidth(true);
    }

    public void ApplyWeaponLevel(WeaponLevelTuning tuning, int level)
    {
        currentDamagePerSecond = Mathf.Max(0f, tuning.damage);
        currentRange = Mathf.Max(0.1f, tuning.range);
        currentRetargetInterval = Mathf.Max(0.02f, tuning.fireInterval);

        if (debugLogs)
        {
            Debug.Log(
                $"[Laser] ApplyWeaponLevel lvl={level} " +
                $"dps={currentDamagePerSecond:0.00} range={currentRange:0.00} " +
                $"retarget={currentRetargetInterval:0.00}"
            );
        }
    }

    void Update()
    {
        ApplyBeamWidth();

        if (firePoint == null || line == null)
        {
            if (line != null)
                line.enabled = false;
            return;
        }

        float rangeMultiplier = stats ? Mathf.Max(0.1f, stats.range.Value) : 1f;
        float damageMultiplier = stats ? Mathf.Max(0f, stats.damage.Value) : 1f;
        float attackSpeedMultiplier = stats ? Mathf.Max(0.05f, stats.attackSpeed.Value) : 1f;

        float finalRange = currentRange * rangeMultiplier;
        float finalDps = currentDamagePerSecond * damageMultiplier * attackSpeedMultiplier;
        float retargetInterval = Mathf.Clamp(currentRetargetInterval, 0.02f, 2f);

        if (currentTarget == null || !IsTargetValid(currentTarget, finalRange))
        {
            currentTarget = FindClosestEnemyInRange(finalRange);
            nextRetargetTime = Time.time + retargetInterval;
            hasSmoothed = false;
        }
        else if (Time.time >= nextRetargetTime)
        {
            nextRetargetTime = Time.time + retargetInterval;
            currentTarget = FindClosestEnemyInRange(finalRange);
            hasSmoothed = false;
        }

        if (currentTarget == null)
        {
            line.enabled = false;
            return;
        }

        Vector3 start = firePoint.position;
        Vector3 targetPos = currentTarget.position + Vector3.up * targetHeightOffset;
        Vector3 dir = (targetPos - start).normalized;

        if (dir.sqrMagnitude < 0.0001f)
        {
            line.enabled = false;
            return;
        }

        Vector3 castStart = start + dir * 0.05f;

        bool hitSomething = Physics.Raycast(
            castStart,
            dir,
            out RaycastHit hit,
            finalRange,
            obstacleMask | enemyMask,
            QueryTriggerInteraction.Ignore
        );

        Vector3 end = start + dir * finalRange;
        bool canDamageTarget = false;
        EnemyHealth targetHealth = null;

        if (hitSomething)
        {
            end = hit.point;

            bool isEnemyLayer = (enemyMask.value & (1 << hit.collider.gameObject.layer)) != 0;
            bool isEnemyTag = hit.collider.CompareTag(enemyTag) || hit.collider.transform.root.CompareTag(enemyTag);

            if (isEnemyLayer || isEnemyTag)
            {
                targetHealth = hit.collider.GetComponentInParent<EnemyHealth>();
                canDamageTarget = targetHealth != null && targetHealth.currentHealth > 0f;
            }
        }

        if (canDamageTarget)
        {
            float damageThisFrame = finalDps * Time.deltaTime;
            targetHealth.TakeDamage(damageThisFrame);

            if (debugLogs)
            {
                Debug.Log(
                    $"[Laser] HIT {targetHealth.name} " +
                    $"dmg={damageThisFrame:0.00} dps={finalDps:0.0} range={finalRange:0.0}"
                );
            }
        }

        bool show = alwaysShowToHit ? hitSomething : canDamageTarget;
        if (!show)
        {
            line.enabled = false;
            return;
        }

        if (!hasSmoothed)
        {
            smoothedEnd = end;
            hasSmoothed = true;
        }
        else
        {
            smoothedEnd = Vector3.Lerp(smoothedEnd, end, aimSmoothing * Time.deltaTime);
        }

        line.enabled = true;
        line.SetPosition(0, start);
        line.SetPosition(1, smoothedEnd);
    }

    void ApplyBeamWidth(bool force = false)
    {
        if (line == null)
            return;

        if (!force && Mathf.Approximately(lastBeamWidth, beamWidth))
            return;

        lastBeamWidth = beamWidth;

        line.widthMultiplier = 1f;
        line.widthCurve = AnimationCurve.Constant(0f, 1f, 1f);
        line.startWidth = beamWidth;
        line.endWidth = beamWidth;
    }

    bool IsTargetValid(Transform target, float range)
    {
        if (target == null)
            return false;

        EnemyHealth eh = target.GetComponentInParent<EnemyHealth>();
        if (eh == null || eh.currentHealth <= 0f)
            return false;

        return Vector3.Distance(transform.position, target.position) <= range;
    }

    Transform FindClosestEnemyInRange(float range)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        Transform closest = null;
        float best = Mathf.Infinity;
        Vector3 pos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
                continue;

            EnemyHealth eh = enemy.GetComponentInParent<EnemyHealth>();
            if (eh == null || eh.currentHealth <= 0f)
                continue;

            float dist = Vector3.Distance(pos, enemy.transform.position);
            if (dist < best && dist <= range)
            {
                best = dist;
                closest = enemy.transform;
            }
        }

        return closest;
    }
}

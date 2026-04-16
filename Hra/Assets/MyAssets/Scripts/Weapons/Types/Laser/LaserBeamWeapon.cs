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
    public LayerMask enemyMask;
    public LayerMask obstacleMask;
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
        if (line == null) line = GetComponent<LineRenderer>();
        if (stats == null) stats = GetComponent<PlayerStats>();

        currentDamagePerSecond = baseDamagePerSecond;
        currentRange = baseRange;
        currentRetargetInterval = baseRetargetInterval;

        line.positionCount = 2;
        line.enabled = false;

        ApplyBeamWidth(true);
    }

    void OnValidate()
    {
        if (line == null) line = GetComponent<LineRenderer>();
        ApplyBeamWidth(true);
    }

    void ApplyBeamWidth(bool force = false)
    {
        if (line == null) return;

        if (!force && Mathf.Approximately(lastBeamWidth, beamWidth))
            return;

        lastBeamWidth = beamWidth;

        line.widthMultiplier = 1f;
        line.widthCurve = AnimationCurve.Constant(0f, 1f, 1f);
        line.startWidth = beamWidth;
        line.endWidth = beamWidth;
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

        if (firePoint == null)
        {
            if (line != null) line.enabled = false;
            return;
        }

        float rangeMult = stats ? Mathf.Max(0.1f, stats.range.Value) : 1f;
        float dmgMult = stats ? Mathf.Max(0f, stats.damage.Value) : 1f;

        float range = currentRange * rangeMult;
        float dps = currentDamagePerSecond * dmgMult;
        float retargetInterval = Mathf.Clamp(currentRetargetInterval, 0.02f, 2f);

        if (Time.time >= nextRetargetTime)
        {
            nextRetargetTime = Time.time + retargetInterval;
            currentTarget = FindClosestEnemyInRange(range);
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

        bool hitSomething = Physics.Raycast(
            start,
            dir,
            out RaycastHit hit,
            range,
            obstacleMask | enemyMask,
            QueryTriggerInteraction.Ignore
        );

        Vector3 end = start + dir * range;
        bool canDamageTarget = false;
        EnemyHealth targetHealth = null;

        if (hitSomething)
        {
            end = hit.point;

            bool isEnemyLayer = ((enemyMask.value & (1 << hit.collider.gameObject.layer)) != 0);

            if (isEnemyLayer || hit.collider.CompareTag(enemyTag))
            {
                targetHealth = hit.collider.GetComponentInParent<EnemyHealth>();
                canDamageTarget = targetHealth != null;
            }
        }

        if (canDamageTarget)
        {
            float dmg = dps * Time.deltaTime;
            targetHealth.TakeDamage(dmg);

            if (debugLogs)
            {
                Debug.Log(
                    $"[Laser] HIT {targetHealth.name} dmg={dmg:0.00} dps={dps:0.0} range={range:0.0}"
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

    Transform FindClosestEnemyInRange(float range)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        Transform closest = null;
        float best = Mathf.Infinity;
        Vector3 pos = transform.position;

        foreach (var e in enemies)
        {
            if (e == null) continue;

            float d = Vector3.Distance(pos, e.transform.position);
            if (d < best && d <= range)
            {
                best = d;
                closest = e.transform;
            }
        }

        return closest;
    }
}

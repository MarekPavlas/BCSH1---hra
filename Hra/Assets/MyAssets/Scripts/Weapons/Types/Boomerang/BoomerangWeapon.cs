using System.Collections.Generic;
using UnityEngine;

public class BoomerangWeapon : MonoBehaviour, IWeaponLevelApplier
{
    [Header("Refs")]
    public PlayerStats stats;
    public BoomerangProjectile boomerangPrefab;
    public Transform firePoint;

    [Header("Fire (BASE)")]
    public float baseFireInterval = 1.0f;
    public float baseDamage = 10f;

    [Min(1)] public int maxActiveBoomerangs = 1;

    [Header("Targeting (BASE)")]
    public bool aimAtClosestEnemy = true;
    public LayerMask enemyMask = ~0;
    public float aimRange = 25f;
    public float targetHeightOffset = 1f;

    [Header("Boomerang flight (BASE)")]
    public float outgoingSpeed = 18f;
    public float returnSpeed = 22f;
    public float outgoingTime = 0.45f;
    public float catchDistance = 1.0f;
    public float maxLifeTime = 6f;

    [Header("Debug")]
    public bool debugLogs = false;

    float currentDamage;
    float currentFireInterval;
    float currentRange;
    float currentOutgoingSpeed;
    float currentReturnSpeed;
    int currentMaxActiveBoomerangs;

    float nextFireTime;
    readonly List<BoomerangProjectile> active = new();
    readonly Collider[] enemyHits = new Collider[64];

    void Awake()
    {
        if (stats == null) stats = GetComponent<PlayerStats>();

        currentDamage = baseDamage;
        currentFireInterval = baseFireInterval;
        currentRange = aimRange;
        currentOutgoingSpeed = outgoingSpeed;
        currentReturnSpeed = returnSpeed;
        currentMaxActiveBoomerangs = maxActiveBoomerangs;
    }

    public void ApplyWeaponLevel(WeaponLevelTuning tuning, int level)
    {
        currentDamage = Mathf.Max(0f, tuning.damage);
        currentFireInterval = Mathf.Max(0.05f, tuning.fireInterval);
        currentRange = Mathf.Max(0.1f, tuning.range);
        currentOutgoingSpeed = Mathf.Max(0.1f, tuning.projectileSpeed);

        float returnRatio = outgoingSpeed > 0.001f ? (returnSpeed / outgoingSpeed) : 1f;
        currentReturnSpeed = currentOutgoingSpeed * Mathf.Max(0.1f, returnRatio);

        if (tuning.maxSimultaneous >= 1)
            currentMaxActiveBoomerangs = tuning.maxSimultaneous;
        else
            currentMaxActiveBoomerangs = Mathf.Max(1, tuning.projectileCount);

        if (debugLogs)
        {
            Debug.Log(
                $"[BoomerangWeapon] ApplyWeaponLevel lvl={level} " +
                $"dmg={currentDamage:0.00} interval={currentFireInterval:0.00} " +
                $"range={currentRange:0.00} outSpeed={currentOutgoingSpeed:0.00} " +
                $"returnSpeed={currentReturnSpeed:0.00} maxActive={currentMaxActiveBoomerangs}"
            );
        }
    }

    void Update()
    {
        if (boomerangPrefab == null || firePoint == null) return;

        CleanupActive();

        if (active.Count >= GetMaxActiveEffective())
            return;

        if (aimAtClosestEnemy)
        {
            if (!TryGetClosestEnemyDirection(out Vector3 dir))
                return;

            TryFire(dir);
        }
        else
        {
            TryFire(firePoint.forward);
        }
    }

    int GetMaxActiveEffective()
    {
        return Mathf.Max(1, currentMaxActiveBoomerangs);
    }

    float GetAtkSpeedMult()
    {
        float atk = stats ? stats.attackSpeed.Value : 1f;
        return Mathf.Max(0.05f, atk);
    }

    float GetDamageMult()
    {
        float dmg = stats ? stats.damage.Value : 1f;
        return Mathf.Max(0f, dmg);
    }

    float GetRangeMult()
    {
        float r = stats ? stats.range.Value : 1f;
        return Mathf.Max(0.1f, r);
    }

    float GetProjectileSpeedMult()
    {
        float p = stats ? stats.projectileSpeed.Value : 1f;
        return Mathf.Max(0.1f, p);
    }

    void TryFire(Vector3 dir)
    {
        float atkSpd = GetAtkSpeedMult();
        float interval = currentFireInterval / atkSpd;
        interval = Mathf.Clamp(interval, 0.05f, 10f);

        if (Time.time < nextFireTime) return;

        float finalDamage = currentDamage * GetDamageMult();

        float speedMult = GetProjectileSpeedMult();
        float outSpeed = currentOutgoingSpeed * speedMult;
        float retSpeed = currentReturnSpeed * speedMult;

        nextFireTime = Time.time + interval;

        SpawnBoomerang(dir.normalized, finalDamage, outSpeed, retSpeed);
    }

    bool TryGetClosestEnemyDirection(out Vector3 dir)
    {
        dir = firePoint.forward;

        float range = currentRange * GetRangeMult();

        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            range,
            enemyHits,
            enemyMask,
            QueryTriggerInteraction.Ignore
        );

        if (count <= 0) return false;

        Transform best = null;
        float bestDist = float.PositiveInfinity;
        Vector3 p = transform.position;

        for (int i = 0; i < count; i++)
        {
            var c = enemyHits[i];
            if (c == null) continue;

            Transform t = c.transform.root;
            if (!t.CompareTag("Enemy")) continue;

            float d = Vector3.Distance(p, t.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }

        if (best == null) return false;

        Vector3 targetPos = best.position + Vector3.up * targetHeightOffset;
        Vector3 to = targetPos - firePoint.position;

        if (to.sqrMagnitude < 0.001f) return false;

        dir = to.normalized;
        return true;
    }

    void SpawnBoomerang(Vector3 dir, float damage, float outSpeed, float retSpeed)
    {
        var bo = Instantiate(
            boomerangPrefab,
            firePoint.position,
            Quaternion.LookRotation(dir, Vector3.up)
        );

        bo.Init(
            weapon: this,
            owner: transform,
            catchPoint: firePoint,
            direction: dir,
            damage: damage,
            outgoingSpeed: outSpeed,
            returnSpeed: retSpeed,
            outgoingTime: outgoingTime,
            catchDistance: catchDistance,
            maxLifeTime: maxLifeTime
        );

        active.Add(bo);
        IgnoreCollisionWithPlayer(bo.gameObject);

        if (debugLogs)
        {
            Debug.Log(
                $"[BoomerangWeapon] Fired. active={active.Count}/{GetMaxActiveEffective()} dmg={damage:0.0}"
            );
        }
    }

    void IgnoreCollisionWithPlayer(GameObject proj)
    {
        var projCol = proj.GetComponent<Collider>();
        if (projCol == null) return;

        var myCols = GetComponentsInChildren<Collider>(true);
        foreach (var c in myCols)
        {
            if (c == null) continue;
            Physics.IgnoreCollision(projCol, c, true);
        }
    }

    void CleanupActive()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (active[i] == null)
                active.RemoveAt(i);
        }
    }

    public void NotifyReturned(BoomerangProjectile p)
    {
        if (p == null) return;

        active.Remove(p);

        if (debugLogs)
        {
            Debug.Log(
                $"[BoomerangWeapon] Returned. active={active.Count}/{GetMaxActiveEffective()}"
            );
        }
    }
}

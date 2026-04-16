using UnityEngine;

public class AKWeapon : MonoBehaviour, IWeaponLevelApplier
{
    [Header("Refs")]
    public PlayerStats stats;
    public Bullet bulletPrefab;
    public Transform firePoint;

    [Header("Targeting")]
    public bool aimAtClosestEnemy = true;
    public LayerMask enemyMask = ~0;
    public float aimRange = 25f;
    public float targetHeightOffset = 1f;

    [Header("AK Fire (magazine + reload)")]
    [Min(1)] public int magazineSize = 20;
    [Min(0.1f)] public float bulletsPerSecond = 12f;
    [Min(0f)] public float reloadTime = 1.2f;
    [Range(0f, 15f)] public float spreadDegrees = 3f;

    [Header("Bullet stats (BASE)")]
    public float baseDamage = 6f;
    public float bulletSpeed = 25f;
    public float bulletLifetime = 3f;

    [Header("Debug")]
    public bool debugLogs = false;

    float currentDamage;
    float currentFireInterval;
    float currentRange;
    float currentBulletSpeed;

    int bulletsLeft;
    float nextShotTime;
    bool reloading;
    float reloadEndTime;

    readonly Collider[] enemyHits = new Collider[64];

    void Awake()
    {
        if (stats == null)
            stats = GetComponent<PlayerStats>();

        bulletsLeft = Mathf.Max(1, magazineSize);

        currentDamage = baseDamage;
        currentFireInterval = 1f / Mathf.Max(0.1f, bulletsPerSecond);
        currentRange = aimRange;
        currentBulletSpeed = bulletSpeed;
    }

    void OnValidate()
    {
        magazineSize = Mathf.Max(1, magazineSize);
        bulletsPerSecond = Mathf.Max(0.1f, bulletsPerSecond);
        reloadTime = Mathf.Max(0f, reloadTime);
        aimRange = Mathf.Max(0.1f, aimRange);
        bulletSpeed = Mathf.Max(0.1f, bulletSpeed);
        bulletLifetime = Mathf.Max(0.01f, bulletLifetime);
    }

    public void ApplyWeaponLevel(WeaponLevelTuning tuning, int level)
    {
        currentDamage = Mathf.Max(0f, tuning.damage);
        currentFireInterval = Mathf.Max(0.05f, tuning.fireInterval);
        currentRange = Mathf.Max(0.1f, tuning.range);
        currentBulletSpeed = Mathf.Max(0.1f, tuning.projectileSpeed);

        if (debugLogs)
        {
            Debug.Log(
                $"[AK] ApplyWeaponLevel lvl={level} " +
                $"dmg={currentDamage:0.00} fireInterval={currentFireInterval:0.00} " +
                $"range={currentRange:0.00} bulletSpeed={currentBulletSpeed:0.00}"
            );
        }
    }

    void Update()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        if (reloading)
        {
            if (Time.time >= reloadEndTime)
            {
                reloading = false;
                bulletsLeft = Mathf.Max(1, magazineSize);

                if (debugLogs)
                    Debug.Log("[AK] Reload finished");
            }

            return;
        }

        Vector3 aimDir = firePoint.forward;

        if (aimAtClosestEnemy)
        {
            if (!TryGetClosestEnemyDirection(out aimDir))
                return;
        }

        Vector3 dir = aimAtClosestEnemy ? aimDir : firePoint.forward;

        float atkSpd = stats ? Mathf.Max(0.05f, stats.attackSpeed.Value) : 1f;
        float shotInterval = currentFireInterval / atkSpd;
        shotInterval = Mathf.Clamp(shotInterval, 0.02f, 10f);

        if (Time.time < nextShotTime)
            return;

        nextShotTime = Time.time + shotInterval;
        FireOne(dir);

        bulletsLeft--;

        if (bulletsLeft <= 0)
        {
            reloading = true;
            reloadEndTime = Time.time + reloadTime;

            if (debugLogs)
                Debug.Log("[AK] Reload started");
        }
    }

    void FireOne(Vector3 dir)
    {
        dir = ApplySpread(dir.normalized, spreadDegrees);

        float dmgMult = stats ? Mathf.Max(0f, stats.damage.Value) : 1f;
        float finalDamage = currentDamage * dmgMult;

        float spdMult = stats ? Mathf.Max(0.1f, stats.projectileSpeed.Value) : 1f;
        float finalSpeed = currentBulletSpeed * spdMult;

        var bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(dir, Vector3.up)
        );

        bullet.damage = finalDamage;
        bullet.speed = finalSpeed;
        bullet.lifetime = bulletLifetime;
        bullet.Launch(dir);

        IgnoreCollisionWithPlayer(bullet.gameObject);

        if (debugLogs)
        {
            Debug.Log(
                $"[AK] Shot dmg={finalDamage:0.0} spd={finalSpeed:0.0} left={bulletsLeft}/{magazineSize}"
            );
        }
    }

    Vector3 ApplySpread(Vector3 dir, float degrees)
    {
        if (degrees <= 0.001f)
            return dir;

        Vector2 rnd = Random.insideUnitCircle * degrees;
        Quaternion qYaw = Quaternion.AngleAxis(rnd.x, Vector3.up);

        Vector3 right = Vector3.Cross(Vector3.up, dir);
        if (right.sqrMagnitude < 0.0001f)
            right = Vector3.right;

        right.Normalize();

        Quaternion qPitch = Quaternion.AngleAxis(-rnd.y, right);

        return (qYaw * qPitch * dir).normalized;
    }

    bool TryGetClosestEnemyDirection(out Vector3 dir)
    {
        dir = firePoint != null ? firePoint.forward : transform.forward;

        float rangeMult = stats ? Mathf.Max(0.1f, stats.range.Value) : 1f;
        float finalRange = currentRange * rangeMult;

        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            finalRange,
            enemyHits,
            enemyMask,
            QueryTriggerInteraction.Ignore
        );

        if (count <= 0)
            return false;

        Transform best = null;
        float bestDist = float.PositiveInfinity;
        Vector3 p = transform.position;

        for (int i = 0; i < count; i++)
        {
            var c = enemyHits[i];
            if (c == null)
                continue;

            Transform t = c.transform.root;
            if (!t.CompareTag("Enemy"))
                continue;

            float d = Vector3.Distance(p, t.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }

        if (best == null)
            return false;

        Vector3 targetPos = best.position + Vector3.up * targetHeightOffset;
        Vector3 to = targetPos - firePoint.position;

        if (to.sqrMagnitude < 0.001f)
            return false;

        dir = to.normalized;
        return true;
    }

    void IgnoreCollisionWithPlayer(GameObject proj)
    {
        var projCol = proj.GetComponent<Collider>();
        if (projCol == null)
            return;

        var myCols = GetComponentsInChildren<Collider>(true);
        foreach (var c in myCols)
        {
            if (c == null)
                continue;

            Physics.IgnoreCollision(projCol, c, true);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aimRange);
    }
}

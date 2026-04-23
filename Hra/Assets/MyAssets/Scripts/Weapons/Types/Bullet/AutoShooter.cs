using UnityEngine;

public class AutoShooter : MonoBehaviour, IWeaponLevelApplier
{
    [Header("Refs")]
    public PlayerStats stats;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Base stats")]
    public float baseFireInterval = 1f;
    public float baseDamage = 5f;
    public float baseRange = 25f;
    public int baseProjectiles = 1;
    public float targetHeightOffset = 1f;

    [Header("Bullet stats")]
    public float bulletSpeed = 20f;
    public int basePierceCount = 3;

    [Header("Spread")]
    public float spreadDegrees = 10f;

    [Header("Debug")]
    public bool debugLogs = false;

    float nextFireTime;

    float currentFireInterval;
    float currentDamage;
    float currentRange;
    float currentBulletSpeed;
    int currentProjectiles;

    void Awake()
    {
        if (stats == null)
            stats = GetComponent<PlayerStats>();

        currentFireInterval = baseFireInterval;
        currentDamage = baseDamage;
        currentRange = baseRange;
        currentBulletSpeed = bulletSpeed;
        currentProjectiles = baseProjectiles;
    }

    public void ApplyWeaponLevel(WeaponLevelTuning tuning, int level)
    {
        currentFireInterval = Mathf.Max(0.05f, tuning.fireInterval);
        currentDamage = Mathf.Max(0f, tuning.damage);
        currentRange = Mathf.Max(0.1f, tuning.range);
        currentBulletSpeed = Mathf.Max(0.1f, tuning.projectileSpeed);
        currentProjectiles = Mathf.Max(1, tuning.projectileCount);

        if (debugLogs)
        {
            Debug.Log(
                $"[AutoShooter] Applied level {level}: " +
                $"dmg={currentDamage}, interval={currentFireInterval}, range={currentRange}, " +
                $"speed={currentBulletSpeed}, proj={currentProjectiles}"
            );
        }
    }

    void Update()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        Transform target = FindClosestEnemyInRange(out _);
        if (target == null)
            return;

        float attackSpeedMultiplier = stats != null ? Mathf.Max(0.05f, stats.attackSpeed.Value) : 1f;
        float interval = Mathf.Clamp(currentFireInterval / attackSpeedMultiplier, 0.05f, 10f);

        if (Time.time < nextFireTime)
            return;

        nextFireTime = Time.time + interval;

        float damageMultiplier = stats != null ? Mathf.Max(0f, stats.damage.Value) : 1f;
        float finalDamage = currentDamage * damageMultiplier;

        int projectileCount = Mathf.Clamp(currentProjectiles, 1, 50);

        Shoot(target, finalDamage, projectileCount);
    }

    void Shoot(Transform target, float damage, int projectileCount)
    {
        Vector3 targetPos = target.position + Vector3.up * targetHeightOffset;
        float projectileSpeedMultiplier = stats != null ? Mathf.Max(0.1f, stats.projectileSpeed.Value) : 1f;
        float finalSpeed = currentBulletSpeed * projectileSpeedMultiplier;

        for (int i = 0; i < projectileCount; i++)
        {
            Vector3 dir = (targetPos - firePoint.position).normalized;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            if (projectileCount > 1)
            {
                float t = i / (float)(projectileCount - 1) - 0.5f;
                rot *= Quaternion.Euler(0f, t * spreadDegrees, 0f);
            }

            GameObject go = Instantiate(bulletPrefab, firePoint.position, rot);

            PiercingBullet piercingBullet = go.GetComponent<PiercingBullet>();
            if (piercingBullet != null)
            {
                piercingBullet.Init(
                    go.transform.forward,
                    finalSpeed,
                    damage,
                    basePierceCount,
                    piercingBullet.lifetime,
                    transform.root
                );
            }
            else
            {
                Bullet bullet = go.GetComponent<Bullet>();
                if (bullet != null)
                {
                    bullet.speed = finalSpeed;
                    bullet.damage = damage;
                    bullet.Launch(go.transform.forward);
                }
            }

            IgnoreCollisionWithPlayer(go);
        }
    }

    void IgnoreCollisionWithPlayer(GameObject bullet)
    {
        Collider bulletCol = bullet.GetComponent<Collider>();
        if (bulletCol == null)
            return;

        Collider[] myCols = GetComponentsInChildren<Collider>();
        foreach (Collider c in myCols)
        {
            if (c == null)
                continue;

            Physics.IgnoreCollision(bulletCol, c, true);
        }
    }

    Transform FindClosestEnemyInRange(out float bestDist)
    {
        bestDist = Mathf.Infinity;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Transform best = null;
        float rangeMultiplier = stats != null ? Mathf.Max(0.1f, stats.range.Value) : 1f;
        float finalRange = currentRange * rangeMultiplier;
        Vector3 origin = transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null)
                continue;

            float dist = Vector3.Distance(origin, enemy.transform.position);
            if (dist < bestDist && dist <= finalRange)
            {
                bestDist = dist;
                best = enemy.transform;
            }
        }

        return best;
    }
}

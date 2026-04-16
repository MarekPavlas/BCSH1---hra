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

    private float _nextFireTime;

    private float currentFireInterval;
    private float currentDamage;
    private float currentRange;
    private float currentBulletSpeed;
    private int currentProjectiles;

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
        currentFireInterval = tuning.fireInterval;
        currentDamage = tuning.damage;
        currentRange = tuning.range;
        currentBulletSpeed = tuning.projectileSpeed;
        currentProjectiles = tuning.projectileCount;

        if (debugLogs)
        {
            Debug.Log($"[AutoShooter] Applied level {level}: dmg={currentDamage}, interval={currentFireInterval}, range={currentRange}, speed={currentBulletSpeed}, proj={currentProjectiles}");
        }
    }

    void Update()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        Transform target = FindClosestEnemyInRange(out float dist);
        if (target == null)
            return;

        float atkSpd = stats ? stats.attackSpeed.Value : 1f;
        atkSpd = Mathf.Max(0.05f, atkSpd);

        float interval = currentFireInterval / atkSpd;
        interval = Mathf.Clamp(interval, 0.05f, 10f);

        if (Time.time < _nextFireTime)
            return;

        _nextFireTime = Time.time + interval;

        float rangeMult = stats ? stats.range.Value : 1f;
        float range = currentRange * rangeMult;
        if (dist > range)
            return;

        float dmgMult = stats ? stats.damage.Value : 1f;
        float damage = currentDamage * dmgMult;

        int projBonus = stats ? stats.GetProjectileBonus() : 0;
        int projectiles = Mathf.Clamp(currentProjectiles + projBonus, 1, 50);

        Shoot(target, damage, projectiles);
    }

    void Shoot(Transform target, float damage, int projectiles)
    {
        Vector3 targetPos = target.position + Vector3.up * targetHeightOffset;

        float projSpeedMult = stats ? stats.projectileSpeed.Value : 1f;
        float finalSpeed = currentBulletSpeed * projSpeedMult;

        for (int i = 0; i < projectiles; i++)
        {
            Vector3 dir = (targetPos - firePoint.position).normalized;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            if (projectiles > 1)
            {
                float t = (i / (float)(projectiles - 1) - 0.5f);
                rot *= Quaternion.Euler(0f, t * spreadDegrees, 0f);
            }

            GameObject go = Instantiate(bulletPrefab, firePoint.position, rot);

            var pb = go.GetComponent<PiercingBullet>();
            if (pb != null)
            {
                pb.speed = finalSpeed;
                pb.damage = damage;
                pb.pierceCount = basePierceCount;
            }

            var b = go.GetComponent<Bullet>();
            if (b != null)
            {
                b.speed = finalSpeed;
                b.damage = damage;
                b.Launch(go.transform.forward);
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
        foreach (var c in myCols)
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

        float rangeMult = stats ? stats.range.Value : 1f;
        float range = currentRange * rangeMult;

        Vector3 p = transform.position;

        foreach (var e in enemies)
        {
            if (e == null)
                continue;

            float d = Vector3.Distance(p, e.transform.position);
            if (d < bestDist && d <= range)
            {
                bestDist = d;
                best = e.transform;
            }
        }

        return best;
    }
}

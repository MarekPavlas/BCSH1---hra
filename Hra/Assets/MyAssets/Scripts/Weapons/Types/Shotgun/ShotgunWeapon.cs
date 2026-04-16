using UnityEngine;

public class ShotgunWeapon : MonoBehaviour
{
    [Header("Refs")]
    public PlayerStats stats;                 
    public Transform firePoint;
    public ShotgunPellet pelletPrefab;

    [Header("Targeting")]
    public bool aimAtClosestEnemy = true;
    public float aimRange = 20f;
    public float targetHeightOffset = 1f;
    public LayerMask enemyMask = ~0;
    public string enemyTag = "Enemy";

    [Header("Fire")]
    public float baseFireInterval = 1.0f;     
    public float baseDamagePerPellet = 4f;    
    public float pelletSpeed = 45f;
    public float pelletLifetime = 1.8f;
    [Min(1)] public int pelletCount = 5;

    [Header("Spread (degrees)")]
    public float spreadYaw = 10f;

    public float spreadPitch = 4f;

    public bool randomizePattern = true;

    [Header("Debug")]
    public bool debugLogs = false;

    float nextFireTime;

    readonly Collider[] enemyHits = new Collider[64];

    void Awake()
    {
        if (stats == null) stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (firePoint == null || pelletPrefab == null) return;

        Vector3 dir;
        if (aimAtClosestEnemy)
        {
            if (!TryGetClosestEnemyDirection(out dir))
                return; 
        }
        else
        {
            dir = firePoint.forward;
        }

        float atkSpd = stats ? Mathf.Max(0.05f, stats.attackSpeed.Value) : 1f;
        float interval = Mathf.Clamp(baseFireInterval / atkSpd, 0.05f, 10f);

        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + interval;

        float dmgMult = stats ? Mathf.Max(0f, stats.damage.Value) : 1f;
        float dmgPerPellet = baseDamagePerPellet * dmgMult;

        FireShotgun(dir, dmgPerPellet);
    }

    bool TryGetClosestEnemyDirection(out Vector3 dir)
    {
        dir = firePoint.forward;

        float rangeMult = stats ? Mathf.Max(0.1f, stats.range.Value) : 1f;
        float range = aimRange * rangeMult;

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

            if (!t.CompareTag(enemyTag)) continue;

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

    void FireShotgun(Vector3 aimDir, float dmgPerPellet)
    {
        Quaternion baseRot = Quaternion.LookRotation(aimDir.normalized, Vector3.up);

        for (int i = 0; i < pelletCount; i++)
        {
            float yaw, pitch;

            if (i == 0)
            {
                yaw = 0f;
                pitch = 0f;
            }
            else
            {
                float t = (pelletCount == 1) ? 0.5f : (float)i / (pelletCount - 1);

                yaw = Mathf.Lerp(-spreadYaw, spreadYaw, t);

                float alt = ((i % 2) == 0) ? 1f : -1f;
                pitch = alt * Mathf.Lerp(0f, spreadPitch, Mathf.Clamp01(t));

                if (randomizePattern)
                {
                    yaw += Random.Range(-spreadYaw * 0.15f, spreadYaw * 0.15f);
                    pitch += Random.Range(-spreadPitch * 0.15f, spreadPitch * 0.15f);
                }
            }

            Quaternion spreadRot = baseRot * Quaternion.Euler(pitch, yaw, 0f);
            Vector3 dir = spreadRot * Vector3.forward;

            var pellet = Instantiate(pelletPrefab, firePoint.position, Quaternion.LookRotation(dir, Vector3.up));
            pellet.Launch(dir, pelletSpeed, dmgPerPellet, pelletLifetime, transform.root);
        }

        if (debugLogs)
            Debug.Log($"[Shotgun] Fired pellets={pelletCount} dmg/pellet={dmgPerPellet:0.0}");
    }
}
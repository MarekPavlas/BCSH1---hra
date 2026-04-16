using System.Collections.Generic;
using UnityEngine;

public class WaveEnemySpawner : MonoBehaviour
{
    [Header("Refs")]
    public WaveManager waveManager;
    public WaveScalingConfig scaling;
    public Transform player;

    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Points (optional)")]
    public Transform[] spawnPoints;
    public bool useSpawnPointsIfAvailable = true;

    [Header("Spawn Around Player (fallback)")]
    public float minRadius = 8f;
    public float maxRadius = 15f;

    [Header("Alive limit")]
    public int maxAlive = 20;

    [Header("Wave behavior")]
    public bool infiniteSpawnsDuringWave = false;

    [Header("Debug")]
    public bool debugLogs = true;

    [Header("Gizmos (Scene view)")]
    public bool drawSpawnRings = true;
    public bool drawSpawnPoints = true;
    public float gizmoY = 0.05f;

    readonly List<GameObject> alive = new();

    float nextSpawnTime;
    int spawnedThisWave;
    int spawnTargetThisWave;

    float hpThisWave;
    float dmgThisWave;
    float intervalThisWave;

    void Awake()
    {
        if (waveManager == null)
            waveManager = FindFirstObjectByType<WaveManager>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }
    }

    void OnEnable()
    {
        if (waveManager == null)
            return;

        waveManager.OnWaveStarted += HandleWaveStarted;
        waveManager.OnWaveEnded += HandleWaveEnded;
    }

    void OnDisable()
    {
        if (waveManager == null)
            return;

        waveManager.OnWaveStarted -= HandleWaveStarted;
        waveManager.OnWaveEnded -= HandleWaveEnded;
    }

    void HandleWaveStarted(int wave)
    {
        if (scaling == null)
        {
            Debug.LogWarning("[WaveEnemySpawner] Missing scaling config!");
            return;
        }

        CleanupAlive();

        hpThisWave = scaling.GetEnemyHp(wave);
        dmgThisWave = scaling.GetEnemyDamage(wave);
        intervalThisWave = scaling.GetSpawnInterval(wave);
        spawnTargetThisWave = scaling.GetSpawnCount(wave);

        spawnedThisWave = 0;
        nextSpawnTime = Time.time;

        if (debugLogs)
        {
            Debug.Log(
                $"[WaveEnemySpawner] Wave {wave} START -> " +
                $"HP={hpThisWave:0.0} DMG={dmgThisWave:0.0} " +
                $"Count={(infiniteSpawnsDuringWave ? "INF" : spawnTargetThisWave.ToString())} " +
                $"Interval={intervalThisWave:0.00}s | maxAlive={maxAlive}"
            );
        }
    }

    void HandleWaveEnded(int wave)
    {
        if (debugLogs)
            Debug.Log($"[WaveEnemySpawner] Wave {wave} END -> stop spawning");
    }

    void Update()
    {
        if (waveManager == null || scaling == null)
            return;

        if (!waveManager.IsWaveRunning)
            return;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
            else
                return;
        }

        CleanupAlive();

        if (alive.Count >= maxAlive)
            return;

        if (!infiniteSpawnsDuringWave && spawnedThisWave >= spawnTargetThisWave)
            return;

        if (Time.time < nextSpawnTime)
            return;

        nextSpawnTime = Time.time + intervalThisWave;

        GameObject go = SpawnOne(hpThisWave, dmgThisWave);
        if (go == null)
            return;

        alive.Add(go);
        spawnedThisWave++;
    }

    GameObject SpawnOne(float hp, float dmg)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("[WaveEnemySpawner] enemyPrefabs is empty.");
            return null;
        }

        Vector3 pos;
        Quaternion rot = Quaternion.identity;

        bool canUsePoints = useSpawnPointsIfAvailable && spawnPoints != null && spawnPoints.Length > 0;

        if (canUsePoints)
        {
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (sp == null)
            {
                Debug.LogWarning("[WaveEnemySpawner] spawnPoints contains NULL.");
                return null;
            }

            pos = sp.position;
            rot = sp.rotation;
        }
        else
        {
            float r = Random.Range(minRadius, maxRadius);
            Vector2 circle = Random.insideUnitCircle.normalized * r;
            pos = new Vector3(player.position.x + circle.x, player.position.y, player.position.z + circle.y);
        }

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        if (prefab == null)
        {
            Debug.LogWarning("[WaveEnemySpawner] enemyPrefabs contains NULL.");
            return null;
        }

        GameObject go = Instantiate(prefab, pos, rot);
        ApplyScaledStats(go, hp, dmg);

        return go;
    }

    void ApplyScaledStats(GameObject go, float hp, float dmg)
    {
        if (go == null)
            return;

        EnemyHealth health = go.GetComponentInChildren<EnemyHealth>();
        if (health != null)
        {
            health.health = hp;
            health.currentHealth = hp;
        }

        EnemyMeleeDamage melee = go.GetComponentInChildren<EnemyMeleeDamage>();
        if (melee != null)
            melee.damagePerHit = dmg;

        EnemyRangedAI ranged = go.GetComponentInChildren<EnemyRangedAI>();
        if (ranged != null)
            ranged.projectileDamage = dmg;
    }

    bool IsDeadOrInvalid(GameObject go)
    {
        if (go == null)
            return true;

        EnemyHealth health = go.GetComponentInChildren<EnemyHealth>();
        return health != null && health.currentHealth <= 0f;
    }

    void CleanupAlive()
    {
        for (int i = alive.Count - 1; i >= 0; i--)
        {
            if (IsDeadOrInvalid(alive[i]))
                alive.RemoveAt(i);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (player == null)
            return;

        if (drawSpawnRings)
        {
            Vector3 p = player.position + Vector3.up * gizmoY;

            Gizmos.color = new Color(1f, 1f, 0f, 0.9f);
            Gizmos.DrawWireSphere(p, minRadius);

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
            Gizmos.DrawWireSphere(p, maxRadius);
        }

        if (drawSpawnPoints && spawnPoints != null)
        {
            Gizmos.color = new Color(0.2f, 1f, 1f, 0.9f);

            foreach (Transform sp in spawnPoints)
            {
                if (sp == null)
                    continue;

                Gizmos.DrawSphere(sp.position, 0.35f);
                Gizmos.DrawLine(sp.position, sp.position + sp.forward * 1.5f);
            }
        }
    }
}

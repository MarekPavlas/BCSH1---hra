using System.Collections.Generic;
using UnityEngine;

public class OrbitingBladesWeapon : MonoBehaviour, IWeaponLevelApplier
{
    [Header("Refs")]
    public PlayerStats stats;

    [Header("Prefab")]
    public GameObject bladePrefab;

    [Header("Base stats")]
    public int bladeCount = 3;
    public float radius = 2.0f;
    public float rotationSpeed = 180f;
    public float bladeDamage = 10f;

    [Header("Orbit Offset")]
    public Vector3 centerOffset = new Vector3(0f, 0.3f, 0f);

    [Header("Debug")]
    public bool debugLogs = false;

    readonly List<Transform> blades = new();
    float angleOffsetRad;

    int currentBladeCount;
    float currentRadius;
    float currentRotationSpeed;
    float currentBladeDamage;

    void Awake()
    {
        if (stats == null)
            stats = GetComponent<PlayerStats>();

        currentBladeCount = bladeCount;
        currentRadius = radius;
        currentRotationSpeed = rotationSpeed;
        currentBladeDamage = bladeDamage;
    }

    void OnEnable()
    {
        Respawn();
    }

    void OnDisable()
    {
        ClearBlades();
    }

    void OnDestroy()
    {
        ClearBlades();
    }

    public void ApplyWeaponLevel(WeaponLevelTuning tuning, int level)
    {
        currentBladeDamage = Mathf.Max(0f, tuning.damage);
        currentRadius = Mathf.Max(0.1f, tuning.range);
        currentBladeCount = Mathf.Max(1, tuning.projectileCount);

        if (debugLogs)
        {
            Debug.Log(
                $"[OrbitingBlades] Apply lvl={level} dmg={currentBladeDamage:0.0} " +
                $"radius={currentRadius:0.0} count={currentBladeCount}"
            );
        }

        Respawn();
    }

    void Update()
    {
        if (blades.Count == 0)
            return;

        float atkSpd = stats ? Mathf.Max(0.05f, stats.attackSpeed.Value) : 1f;
        angleOffsetRad += currentRotationSpeed * atkSpd * Mathf.Deg2Rad * Time.deltaTime;

        float rangeMult = stats ? Mathf.Max(0.1f, stats.range.Value) : 1f;
        float effectiveRadius = currentRadius * rangeMult;

        Vector3 center = transform.position + centerOffset;

        for (int i = 0; i < blades.Count; i++)
        {
            if (blades[i] == null)
                continue;

            float a = angleOffsetRad + i * (Mathf.PI * 2f / blades.Count);
            Vector3 pos = center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * effectiveRadius;
            blades[i].position = pos;

            Vector3 dir = blades[i].position - center;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
                blades[i].rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }
    }

    public float GetCurrentDamage()
    {
        float dmgMult = stats ? Mathf.Max(0f, stats.damage.Value) : 1f;
        return currentBladeDamage * dmgMult;
    }

    void SpawnBlades()
    {
        if (bladePrefab == null)
        {
            if (debugLogs)
                Debug.LogWarning("[OrbitingBlades] bladePrefab is NULL");
            return;
        }

        ClearBlades();

        Vector3 center = transform.position + centerOffset;

        for (int i = 0; i < currentBladeCount; i++)
        {
            float a = i * (Mathf.PI * 2f / Mathf.Max(1, currentBladeCount));
            Vector3 pos = center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * currentRadius;

            GameObject go = Instantiate(bladePrefab, pos, Quaternion.identity);
            go.name = "Blade_" + i;
            go.transform.SetParent(transform, true);

            OrbitingBladeHit hit = go.GetComponent<OrbitingBladeHit>();
            if (hit != null)
            {
                hit.weapon = this;
                hit.damage = currentBladeDamage;
            }

            blades.Add(go.transform);
        }

        if (debugLogs)
            Debug.Log($"[OrbitingBlades] Spawned {currentBladeCount} blades");
    }

    void ClearBlades()
    {
        for (int i = blades.Count - 1; i >= 0; i--)
        {
            if (blades[i] != null)
                Destroy(blades[i].gameObject);
        }

        blades.Clear();
    }

    [ContextMenu("Respawn Blades")]
    public void Respawn()
    {
        SpawnBlades();
    }
}

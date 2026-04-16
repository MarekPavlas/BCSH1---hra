using System.Collections.Generic;
using UnityEngine;

public class OrbitingBladesWeapon : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject bladePrefab;

    [Header("Blades")]
    public int bladeCount = 3;
    public float radius = 2.0f;
    public float rotationSpeed = 180f; 
    public float bladeDamage = 10f;

    [Header("Orbit Offset")]
    public Vector3 centerOffset = new Vector3(0f, 0.3f, 0f); 

    [Header("Debug")]
    public bool debugLogs = false;

    private readonly List<Transform> blades = new List<Transform>();
    private float angleOffsetRad;

    void Start()
    {
        SpawnBlades();
    }

    void Update()
    {
        if (blades.Count == 0) return;

        angleOffsetRad += rotationSpeed * Mathf.Deg2Rad * Time.deltaTime;

        Vector3 center = transform.position + centerOffset;

        for (int i = 0; i < blades.Count; i++)
        {
            float a = angleOffsetRad + i * (Mathf.PI * 2f / blades.Count);

            Vector3 pos = center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius;
            blades[i].position = pos;

            Vector3 dir = (blades[i].position - center);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                blades[i].rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }
    }

    void SpawnBlades()
    {
        if (bladePrefab == null)
        {
            if (debugLogs) Debug.LogWarning("[OrbitingBlades] bladePrefab is NULL");
            return;
        }

        for (int i = blades.Count - 1; i >= 0; i--)
        {
            if (blades[i] != null) Destroy(blades[i].gameObject);
        }
        blades.Clear();

        Vector3 center = transform.position + centerOffset;

        for (int i = 0; i < bladeCount; i++)
        {
            float a = i * (Mathf.PI * 2f / Mathf.Max(1, bladeCount));
            Vector3 pos = center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius;

            GameObject go = Instantiate(bladePrefab, pos, Quaternion.identity);
            go.name = "Blade_" + i;

            var hit = go.GetComponent<OrbitingBladeHit>();
            if (hit != null) hit.damage = bladeDamage;

            blades.Add(go.transform);
        }

        if (debugLogs) Debug.Log($"[OrbitingBlades] Spawned {bladeCount} blades | radius={radius} | offset={centerOffset}");
    }

    [ContextMenu("Respawn Blades")]
    public void Respawn()
    {
        SpawnBlades();
    }
}
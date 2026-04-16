using System.Collections.Generic;
using UnityEngine;

public class AOESlowZone : MonoBehaviour
{
    [Header("Follow")]
    public Transform player;
    public float footOffset = 0.10f;

    [Header("Shape (hits only near feet)")]
    public float radius = 3.0f;
    public float capsuleHeight = 0.6f;
    public LayerMask enemyMask = ~0;
    public bool includeTriggerColliders = false;

    [Header("Slow")]
    [Range(0f, 0.95f)] public float slowPercent = 0.35f;

    public float slowDuration = 0.35f;

    public float tickInterval = 0.15f;

    [Header("Height filter (IMPORTANT)")]
    public float feetHeightTolerance = 0.25f;

    [Header("Visual")]
    public Transform visual;
    public bool showVisual = true;
    public bool scaleVisualToRadius = true;
    public float visualYOffest = 0.02f;

    [Header("Debug")]
    public bool debugLogs = false;

    float nextTick;
    readonly Collider[] hits = new Collider[128];

    void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        Vector3 center = GetFeetCenterXZ();

        if (visual != null)
        {
            if (visual.gameObject.activeSelf != showVisual)
                visual.gameObject.SetActive(showVisual);

            if (showVisual)
            {
                visual.position = new Vector3(center.x, center.y + visualYOffest, center.z);

                if (scaleVisualToRadius)
                    visual.localScale = new Vector3(radius * 2f, visual.localScale.y, radius * 2f);
            }
        }

        if (Time.time < nextTick) return;
        nextTick = Time.time + Mathf.Max(0.02f, tickInterval);

        DoSlowTick(center);
    }

    void DoSlowTick(Vector3 center)
    {
        float half = Mathf.Max(0.01f, capsuleHeight * 0.5f);

        Vector3 p1 = center + Vector3.up * half;
        Vector3 p2 = center - Vector3.up * half;

        var qti = includeTriggerColliders ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

        int count = Physics.OverlapCapsuleNonAlloc(p1, p2, radius, hits, enemyMask, qti);
        if (count <= 0) return;

        float feetY = center.y;
        float slowMul = 1f - slowPercent;
        slowMul = Mathf.Clamp(slowMul, 0.05f, 1f);

        HashSet<EnemyStatusController> unique = new HashSet<EnemyStatusController>();

        for (int i = 0; i < count; i++)
        {
            var c = hits[i];
            if (c == null) continue;

            var esc = c.GetComponentInParent<EnemyStatusController>();
            if (esc == null) continue;

            if (!unique.Add(esc)) continue;

            if (!IsEnemyFeetNearPlayerFeet(esc.transform, feetY, feetHeightTolerance))
                continue;

            esc.ApplySlowMultiplier(slowMul, slowDuration);

            if (debugLogs)
                Debug.Log($"[AOE SLOW] {esc.name} slowMul={slowMul:0.00} dur={slowDuration:0.00}");
        }
    }

    Vector3 GetFeetCenterXZ()
    {
        Vector3 pos = player.position;
        float feetY = player.position.y;

        if (player.TryGetComponent<CharacterController>(out var cc))
            feetY = cc.bounds.min.y;
        else if (player.TryGetComponent<Collider>(out var col))
            feetY = col.bounds.min.y;

        return new Vector3(pos.x, feetY + footOffset, pos.z);
    }

    bool IsEnemyFeetNearPlayerFeet(Transform enemyRoot, float playerFeetY, float tolerance)
    {
        Collider col = enemyRoot.GetComponentInChildren<Collider>();
        if (col == null) return true;

        float enemyFeetY = col.bounds.min.y;
        return Mathf.Abs(enemyFeetY - playerFeetY) <= Mathf.Max(0.01f, tolerance);
    }
}

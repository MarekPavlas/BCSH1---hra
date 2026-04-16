using UnityEngine;

public class EnemyMeleeDamage : MonoBehaviour
{
    [Header("Target")]
    public string playerTag = "Player";
    public LayerMask playerMask = ~0; 

    [Header("Damage")]
    public float damagePerHit = 10f;
    public float hitInterval = 0.8f;     
    public float hitRadius = 1.3f;       

    [Header("Optional LOS")]
    public bool requireLineOfSight = false;
    public LayerMask lineOfSightMask = ~0;
    public float aimHeightOffset = 1.0f;

    [Header("Debug")]
    public bool debugLogs = false;

    float nextHitTime;

    void Update()
    {
        if (Time.time < nextHitTime) return;

        var hits = Physics.OverlapSphere(transform.position, hitRadius, playerMask, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0) return;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;

            if (!c.CompareTag(playerTag) && (c.transform.root == null || !c.transform.root.CompareTag(playerTag)))
                continue;

            Transform playerTf = c.transform.root != null ? c.transform.root : c.transform;

            if (requireLineOfSight && !HasLineOfSight(playerTf))
                continue;

            var stats = c.GetComponentInParent<PlayerStats>();
            if (stats == null) stats = playerTf.GetComponentInChildren<PlayerStats>();
            if (stats == null) continue;

            stats.TakeDamage(damagePerHit);
            nextHitTime = Time.time + hitInterval;

            if (debugLogs)
                Debug.Log($"[EnemyMeleeDamage] {name} hit player for {damagePerHit}");

            return;
        }
    }

    bool HasLineOfSight(Transform playerTf)
    {
        Vector3 origin = transform.position + Vector3.up * aimHeightOffset;
        Vector3 target = playerTf.position + Vector3.up * aimHeightOffset;
        Vector3 dir = target - origin;
        float dist = dir.magnitude;

        if (dist <= 0.01f) return true;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, lineOfSightMask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == playerTf || hit.transform.IsChildOf(playerTf);
        }

        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
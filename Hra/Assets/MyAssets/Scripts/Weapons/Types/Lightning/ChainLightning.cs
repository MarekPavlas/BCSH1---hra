using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ChainLightning : MonoBehaviour
{
    [Header("Visual")]
    public float duration = 0.12f;
    public float width = 0.08f;

    [Range(2, 30)] public int pointsPerLink = 8;

    public float jaggedness = 0.6f;

    LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 0;
        line.startWidth = width;
        line.endWidth = width;
    }

    public void Play(
        Vector3 skyStart,
        EnemyHealth firstTarget,
        Vector3 firstHitPos,
        int maxChains,
        float chainRange,
        float damage,
        float damageFalloffPerJump,
        LayerMask enemyMask,
        string enemyTag,
        float targetHeightOffset,
        bool debugLogs
    )
    {
        StartCoroutine(RunChain(
            skyStart, firstTarget, firstHitPos,
            maxChains, chainRange,
            damage, damageFalloffPerJump,
            enemyMask, enemyTag, targetHeightOffset,
            debugLogs
        ));
    }

    IEnumerator RunChain(
        Vector3 skyStart,
        EnemyHealth firstTarget,
        Vector3 firstHitPos,
        int maxChains,
        float chainRange,
        float damage,
        float damageFalloffPerJump,
        LayerMask enemyMask,
        string enemyTag,
        float targetHeightOffset,
        bool debugLogs
    )
    {
        if (firstTarget == null)
        {
            Destroy(gameObject);
            yield break;
        }

        List<EnemyHealth> chainTargets = new List<EnemyHealth>(1 + maxChains);
        HashSet<EnemyHealth> used = new HashSet<EnemyHealth>();

        EnemyHealth current = firstTarget;
        chainTargets.Add(current);
        used.Add(current);

        for (int i = 0; i < maxChains; i++)
        {
            EnemyHealth next = FindNextTarget(current.transform.position, chainRange, enemyMask, enemyTag, used);
            if (next == null) break;

            chainTargets.Add(next);
            used.Add(next);
            current = next;
        }

        List<Vector3> points = new List<Vector3>();

        Vector3 start = skyStart;
        Vector3 end = chainTargets[0].transform.position + Vector3.up * targetHeightOffset;

        AddJaggedLink(points, start, end);

        ApplyDamage(chainTargets[0], damage, debugLogs);

        float curDamage = damage;
        for (int i = 1; i < chainTargets.Count; i++)
        {
            curDamage *= Mathf.Clamp01(damageFalloffPerJump);

            Vector3 a = chainTargets[i - 1].transform.position + Vector3.up * targetHeightOffset;
            Vector3 b = chainTargets[i].transform.position + Vector3.up * targetHeightOffset;

            AddJaggedLink(points, a, b);
            ApplyDamage(chainTargets[i], curDamage, debugLogs);
        }

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());

        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    void ApplyDamage(EnemyHealth eh, float dmg, bool debugLogs)
    {
        if (eh == null) return;
        if (eh.currentHealth <= 0f) return;

        eh.TakeDamage(dmg);

        if (debugLogs)
            Debug.Log($"[ChainLightning] HIT {eh.name} dmg={dmg:0.0}");
    }

    EnemyHealth FindNextTarget(
        Vector3 fromPos,
        float range,
        LayerMask enemyMask,
        string enemyTag,
        HashSet<EnemyHealth> used
    )
    {
        Collider[] cols = Physics.OverlapSphere(fromPos, range, enemyMask, QueryTriggerInteraction.Ignore);
        EnemyHealth best = null;
        float bestDist = float.PositiveInfinity;

        foreach (var c in cols)
        {
            if (c == null) continue;
            Transform root = c.transform.root;
            if (root == null) continue;
            if (!root.CompareTag(enemyTag)) continue;

            var eh = c.GetComponentInParent<EnemyHealth>();
            if (eh == null) continue;
            if (eh.currentHealth <= 0f) continue;
            if (used.Contains(eh)) continue;

            float d = Vector3.Distance(fromPos, root.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = eh;
            }
        }

        return best;
    }

    void AddJaggedLink(List<Vector3> points, Vector3 a, Vector3 b)
    {
        if (points.Count == 0) points.Add(a);

        Vector3 dir = (b - a);
        float len = dir.magnitude;
        if (len < 0.0001f)
        {
            points.Add(b);
            return;
        }

        Vector3 n = dir / len;

        for (int i = 1; i < pointsPerLink; i++)
        {
            float t = i / (float)pointsPerLink;
            Vector3 p = Vector3.Lerp(a, b, t);

            Vector3 side = Vector3.Cross(n, Vector3.up);
            if (side.sqrMagnitude < 0.001f) side = Vector3.right;
            side.Normalize();

            float amp = jaggedness * len * 0.05f;
            p += side * Random.Range(-amp, amp);
            p += Vector3.up * Random.Range(-amp, amp) * 0.25f;

            points.Add(p);
        }

        points.Add(b);
    }
}
using UnityEngine;

public class LightningWeapon : MonoBehaviour
{
    public enum FirstTargetMode
    {
        ClosestToPlayer,
        RandomAnywhere
    }

    [Header("Refs")]
    public PlayerStats stats;              
    public GameObject lightningPrefab;       
    public Transform player;                

    [Header("Fire")]
    public float fireInterval = 2f;
    public float baseDamage = 10f;

    [Header("Targeting")]
    public FirstTargetMode firstTargetMode = FirstTargetMode.ClosestToPlayer;

    public float firstStrikeRange = 25f;

    public string enemyTag = "Enemy";

    public LayerMask enemyMask = ~0;

    public float targetHeightOffset = 1f;

    [Header("Chain")]
    [Min(0)] public int maxChains = 3;

    [Min(0.1f)] public float chainRange = 8f;

    [Range(0.1f, 1f)] public float damageFalloffPerJump = 1f;

    [Header("From Sky")]
    public bool strikeFromSky = true;
    public float skyHeight = 25f;

    [Header("Debug")]
    public bool debugLogs = false;

    float nextFireTime;

    void Awake()
    {
        if (player == null) player = transform;
        if (stats == null) stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (lightningPrefab == null) return;
        if (Time.time < nextFireTime) return;

        EnemyHealth first = FindFirstTarget();
        if (first == null) return;

        nextFireTime = Time.time + Mathf.Max(0.05f, fireInterval);

        float dmgMult = stats ? stats.damage.Value : 1f;
        float damage = baseDamage * dmgMult;

        SpawnLightning(first, damage);
    }

    EnemyHealth FindFirstTarget()
    {
        if (firstTargetMode == FirstTargetMode.RandomAnywhere)
        {
            var all = GameObject.FindGameObjectsWithTag(enemyTag);
            if (all == null || all.Length == 0) return null;

            for (int i = 0; i < 10; i++)
            {
                var go = all[Random.Range(0, all.Length)];
                if (go == null) continue;
                var eh = go.GetComponentInParent<EnemyHealth>();
                if (eh != null && eh.currentHealth > 0f) return eh;
            }
            return null;
        }
        else
        {
            Collider[] hits = Physics.OverlapSphere(player.position, firstStrikeRange, enemyMask, QueryTriggerInteraction.Ignore);
            EnemyHealth best = null;
            float bestDist = float.PositiveInfinity;

            foreach (var c in hits)
            {
                if (c == null) continue;

                Transform root = c.transform.root;
                if (root == null) continue;
                if (!root.CompareTag(enemyTag)) continue;

                var eh = c.GetComponentInParent<EnemyHealth>();
                if (eh == null || eh.currentHealth <= 0f) continue;

                float d = Vector3.Distance(player.position, root.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = eh;
                }
            }
            return best;
        }
    }

    void SpawnLightning(EnemyHealth firstTarget, float damage)
    {
        var go = Instantiate(lightningPrefab);
        var chain = go.GetComponent<ChainLightning>();

        if (chain == null)
        {
            Debug.LogWarning("[LightningWeapon] lightningPrefab nemá komponentu ChainLightning.");
            Destroy(go);
            return;
        }

        Vector3 skyStart = firstTarget.transform.position + Vector3.up * Mathf.Max(0f, skyHeight);
        Vector3 firstHitPos = firstTarget.transform.position + Vector3.up * targetHeightOffset;

        chain.Play(
            skyStart: strikeFromSky ? skyStart : (player.position + Vector3.up * targetHeightOffset),
            firstTarget: firstTarget,
            firstHitPos: firstHitPos,
            maxChains: maxChains,
            chainRange: chainRange,
            damage: damage,
            damageFalloffPerJump: damageFalloffPerJump,
            enemyMask: enemyMask,
            enemyTag: enemyTag,
            targetHeightOffset: targetHeightOffset,
            debugLogs: debugLogs
        );
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, firstStrikeRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, chainRange);
    }
}
using UnityEngine;

public class LightningWeapon : MonoBehaviour, IWeaponLevelApplier
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

    float currentFireInterval;
    float currentDamage;
    float currentFirstStrikeRange;

    void Awake()
    {
        if (player == null) player = transform;
        if (stats == null) stats = GetComponent<PlayerStats>();

        currentFireInterval = fireInterval;
        currentDamage = baseDamage;
        currentFirstStrikeRange = firstStrikeRange;
    }

    public void ApplyWeaponLevel(WeaponLevelTuning tuning, int level)
    {
        currentFireInterval = Mathf.Max(0.05f, tuning.fireInterval);
        currentDamage = Mathf.Max(0f, tuning.damage);
        currentFirstStrikeRange = Mathf.Max(0.1f, tuning.range);

        if (debugLogs)
        {
            Debug.Log(
                $"[LightningWeapon] Apply lvl={level} " +
                $"dmg={currentDamage:0.0} interval={currentFireInterval:0.00} range={currentFirstStrikeRange:0.0}"
            );
        }
    }

    void Update()
    {
        if (lightningPrefab == null)
            return;

        float atkSpd = stats ? Mathf.Max(0.05f, stats.attackSpeed.Value) : 1f;
        float interval = Mathf.Clamp(currentFireInterval / atkSpd, 0.05f, 10f);

        if (Time.time < nextFireTime)
            return;

        float rangeMult = stats ? Mathf.Max(0.1f, stats.range.Value) : 1f;
        float effectiveFirstStrikeRange = currentFirstStrikeRange * rangeMult;
        float effectiveChainRange = chainRange * rangeMult;

        EnemyHealth first = FindFirstTarget(effectiveFirstStrikeRange);
        if (first == null)
            return;

        nextFireTime = Time.time + interval;

        float dmgMult = stats ? Mathf.Max(0f, stats.damage.Value) : 1f;
        float damage = currentDamage * dmgMult;

        SpawnLightning(first, damage, effectiveChainRange);
    }

    EnemyHealth FindFirstTarget(float effectiveFirstStrikeRange)
    {
        if (firstTargetMode == FirstTargetMode.RandomAnywhere)
        {
            GameObject[] all = GameObject.FindGameObjectsWithTag(enemyTag);
            if (all == null || all.Length == 0)
                return null;

            for (int i = 0; i < 10; i++)
            {
                GameObject go = all[Random.Range(0, all.Length)];
                if (go == null)
                    continue;

                EnemyHealth eh = go.GetComponentInParent<EnemyHealth>();
                if (eh != null && eh.currentHealth > 0f)
                    return eh;
            }

            return null;
        }

        Collider[] hits = Physics.OverlapSphere(
            player.position,
            effectiveFirstStrikeRange,
            enemyMask,
            QueryTriggerInteraction.Ignore
        );

        EnemyHealth best = null;
        float bestDist = float.PositiveInfinity;

        foreach (Collider c in hits)
        {
            if (c == null)
                continue;

            Transform root = c.transform.root;
            if (root == null || !root.CompareTag(enemyTag))
                continue;

            EnemyHealth eh = c.GetComponentInParent<EnemyHealth>();
            if (eh == null || eh.currentHealth <= 0f)
                continue;

            float d = Vector3.Distance(player.position, root.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = eh;
            }
        }

        return best;
    }

    void SpawnLightning(EnemyHealth firstTarget, float damage, float effectiveChainRange)
    {
        GameObject go = Instantiate(lightningPrefab);
        ChainLightning chain = go.GetComponent<ChainLightning>();

        if (chain == null)
        {
            Debug.LogWarning("[LightningWeapon] lightningPrefab nema komponentu ChainLightning.");
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
            chainRange: effectiveChainRange,
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
        if (player == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, firstStrikeRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.position, chainRange);
    }
}

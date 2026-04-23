using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    [Header("Money")]
    public GameObject moneyPrefab;
    [Range(0f, 1f)] public float moneyDropChance = 0.25f;
    public int minAmount = 1;
    public int maxAmount = 3;

    [Header("Health")]
    public GameObject healthPrefab;
    [Range(0f, 1f)] public float healthDropChance = 0.1f;

    [Header("Spawn")]
    public float spawnYOffset = 0.2f;
    public float rayStartHeight = 5f;
    public float rayDistance = 20f;
    public float moneyScatterRadius = 0.15f;
    public float healthScatterRadius = 0.25f;
    public LayerMask groundMask = ~0;

    [Header("Debug")]
    public bool debugLogs = false;

    bool suppressNextDrop = false;

    public void SuppressNextDrop()
    {
        suppressNextDrop = true;
    }

    public void Drop(bool killedByStackOfMoney, float bonusChance, float bonusExtraPercent)
    {
        if (suppressNextDrop)
        {
            if (debugLogs)
                Debug.Log($"[EnemyDrop] DROP SUPPRESSED on {name}");

            suppressNextDrop = false;
            return;
        }

        TryDropMoney(killedByStackOfMoney, bonusChance, bonusExtraPercent);
        TryDropHealth();
    }

    void TryDropMoney(bool killedByStackOfMoney, float bonusChance, float bonusExtraPercent)
    {
        if (moneyPrefab == null)
        {
            if (debugLogs)
                Debug.LogWarning($"[EnemyDrop] moneyPrefab is NULL on {name}");
            return;
        }

        if (moneyDropChance <= 0f)
            return;

        if (Random.value > moneyDropChance)
            return;

        int amount = Random.Range(minAmount, maxAmount + 1);

        if (killedByStackOfMoney && bonusChance > 0f && Random.value <= Mathf.Clamp01(bonusChance))
        {
            float mult = 1f + Mathf.Max(0f, bonusExtraPercent);
            int boosted = Mathf.Max(amount + 1, Mathf.RoundToInt(amount * mult));

            if (debugLogs)
                Debug.Log($"[EnemyDrop] BONUS! amount {amount} -> {boosted}");

            amount = boosted;
        }

        Vector3 spawnPos = GetGroundSpawnPosition(moneyScatterRadius);
        GameObject go = Instantiate(moneyPrefab, spawnPos, Quaternion.identity);

        MoneyPickup mp = go.GetComponent<MoneyPickup>();
        if (mp == null)
            mp = go.GetComponentInChildren<MoneyPickup>();

        if (mp != null)
            mp.amount = amount;

        if (debugLogs)
            Debug.Log($"[EnemyDrop] Dropped money amount={amount} from {name}");
    }

    void TryDropHealth()
    {
        if (healthPrefab == null)
            return;

        if (healthDropChance <= 0f)
            return;

        if (Random.value > healthDropChance)
            return;

        Vector3 spawnPos = GetGroundSpawnPosition(healthScatterRadius);
        Instantiate(healthPrefab, spawnPos, Quaternion.identity);

        if (debugLogs)
            Debug.Log($"[EnemyDrop] Dropped health from {name}");
    }

    Vector3 GetGroundSpawnPosition(float scatterRadius)
    {
        Vector2 random2D = Random.insideUnitCircle * scatterRadius;
        Vector3 approxPos = transform.position + new Vector3(random2D.x, 0f, random2D.y);
        Vector3 rayOrigin = approxPos + Vector3.up * rayStartHeight;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, groundMask, QueryTriggerInteraction.Ignore))
            return hit.point + Vector3.up * spawnYOffset;

        return approxPos + Vector3.up * spawnYOffset;
    }
}

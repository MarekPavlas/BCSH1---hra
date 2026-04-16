using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    [Header("Money")]
    public GameObject moneyPrefab;
    [Range(0f, 1f)] public float dropChance = 0.25f;
    public int minAmount = 1;
    public int maxAmount = 3;

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
            if (debugLogs) Debug.Log($"[EnemyDrop] DROP SUPPRESSED on {name}");
            suppressNextDrop = false;
            return;
        }

        if (moneyPrefab == null)
        {
            if (debugLogs) Debug.LogWarning($"[EnemyDrop] moneyPrefab is NULL on {name}");
            return;
        }

        if (dropChance <= 0f) return;
        if (Random.value > dropChance) return;

        int amount = Random.Range(minAmount, maxAmount + 1);

        if (killedByStackOfMoney && bonusChance > 0f && Random.value <= Mathf.Clamp01(bonusChance))
        {
            float mult = 1f + Mathf.Max(0f, bonusExtraPercent);
            int boosted = Mathf.Max(amount + 1, Mathf.RoundToInt(amount * mult));
            if (debugLogs) Debug.Log($"[EnemyDrop] BONUS! amount {amount} -> {boosted} (chance={bonusChance:P0}, +{bonusExtraPercent:P0})");
            amount = boosted;
        }

        var go = Instantiate(moneyPrefab, transform.position, Quaternion.identity);
        var mp = go.GetComponent<MoneyPickup>();
        if (mp != null) mp.amount = amount;

        if (debugLogs) Debug.Log($"[EnemyDrop] Dropped money amount={amount} from {name}");
    }
}
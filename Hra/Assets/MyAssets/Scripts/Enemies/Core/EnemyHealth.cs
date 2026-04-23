using UnityEngine;

public enum DamageSource
{
    Unknown,
    StackOfMoney
}

public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    public float health = 50f;
    public float currentHealth;

    [Header("Damage Modifiers")]
    public float damageMultiplier = 1f;

    [Header("Debug")]
    public bool debugLogs = false;

    DamageSource lastSource = DamageSource.Unknown;
    float lastBonusChance = 0f;
    float lastBonusExtraPercent = 0f;
    bool isDead = false;

    void Awake()
    {
        if (currentHealth <= 0f)
            currentHealth = health;
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, DamageSource.Unknown, 0f, 0f);
    }

    public void TakeDamage(float damage, DamageSource source, float bonusChance, float bonusExtraPercent)
    {
        if (isDead)
            return;

        lastSource = source;

        if (source == DamageSource.StackOfMoney)
        {
            lastBonusChance = Mathf.Clamp01(bonusChance);
            lastBonusExtraPercent = Mathf.Max(0f, bonusExtraPercent);
        }
        else
        {
            lastBonusChance = 0f;
            lastBonusExtraPercent = 0f;
        }

        float mult = Mathf.Max(0f, damageMultiplier);
        float finalDamage = damage * mult;

        float before = currentHealth;
        currentHealth -= finalDamage;

        if (debugLogs)
            Debug.Log($"[EnemyHealth] {name} dmg={finalDamage:0.00} HP {before:0.00}->{currentHealth:0.00} source={source}");

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        EnemyDrop drop = GetComponent<EnemyDrop>();
        if (drop == null)
            drop = GetComponentInChildren<EnemyDrop>();

        if (drop != null)
        {
            bool killedByStack = lastSource == DamageSource.StackOfMoney;
            drop.Drop(killedByStack, lastBonusChance, lastBonusExtraPercent);
        }

        Destroy(gameObject);
    }

    public void SetMaxHealth(float newMax, bool healToFull)
    {
        health = Mathf.Max(1f, newMax);

        if (healToFull)
            currentHealth = health;
        else
            currentHealth = Mathf.Min(currentHealth, health);
    }
}

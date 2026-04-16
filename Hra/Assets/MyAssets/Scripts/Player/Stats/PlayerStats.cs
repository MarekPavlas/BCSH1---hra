using System;
using UnityEngine;

public enum PlayerStatType
{
    MaxHp,
    RegenPerSec,
    MoveSpeed,
    Damage,
    AttackSpeed,
    Range,
    ProjectileSpeed,
    ProjectileCount,
    PickupRange,
    MoneyGain,
    Luck,
    DamageTaken,
    CritChance,
    CritDamage,
    Dodge,
    ItemPrice
}

[Serializable]
public class StatBlock
{
    [Header("Base")]
    public float baseValue = 0f;

    [Header("Bonuses (runtime)")]
    public float flatAdd = 0f;
    public float percentAdd = 0f;

    [Header("Multipliers (runtime)")]
    public float multiplier = 1f;

    public float Value
    {
        get
        {
            float v = baseValue + flatAdd;
            v *= (1f + percentAdd);
            v *= multiplier;
            return v;
        }
    }

    public void ResetRuntime()
    {
        flatAdd = 0f;
        percentAdd = 0f;
        multiplier = 1f;
    }
}

public class PlayerStats : MonoBehaviour
{
    [Header("HP")]
    public StatBlock maxHp = new StatBlock { baseValue = 100f };
    public float currentHP = 100f;
    public bool setMaxHpFromStartingHp = true;

    [Header("Regen")]
    public StatBlock regenPerSec = new StatBlock { baseValue = 0f };

    [Header("Movement")]
    public StatBlock moveSpeed = new StatBlock { baseValue = 12f };

    [Header("Combat")]
    public StatBlock damage = new StatBlock { baseValue = 1f };
    public StatBlock attackSpeed = new StatBlock { baseValue = 1f };
    public StatBlock range = new StatBlock { baseValue = 1f };
    public StatBlock projectileSpeed = new StatBlock { baseValue = 1f };
    public StatBlock projectileCount = new StatBlock { baseValue = 0f };

    [Header("Utility")]
    public StatBlock pickupRange = new StatBlock { baseValue = 2.5f };
    public StatBlock moneyGain = new StatBlock { baseValue = 1f };
    public StatBlock luck = new StatBlock { baseValue = 0f };
    public StatBlock itemPrice = new StatBlock { baseValue = 1f };

    [Header("Defense")]
    public StatBlock damageTaken = new StatBlock { baseValue = 1f };
    public StatBlock dodge = new StatBlock { baseValue = 0f };

    [Header("Crit")]
    public StatBlock critChance = new StatBlock { baseValue = 0f };
    public StatBlock critDamage = new StatBlock { baseValue = 1f };

    [Header("Progression")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 5;
    public int totalCrystals = 0;

    public event Action OnLevelUp;

    [Header("Debug")]
    public bool debugLogs = false;
    public float debugPrintEvery = 5f;

    float regenAccum;
    float debugNext;

    void Awake()
    {
        if (setMaxHpFromStartingHp)
        {
            float start = currentHP > 0f ? currentHP : maxHp.baseValue;
            start = Mathf.Max(1f, start);

            maxHp.baseValue = start;
            currentHP = start;
        }
        else
        {
            if (currentHP <= 0f)
                currentHP = maxHp.Value;
        }

        currentHP = Mathf.Clamp(currentHP, 0f, maxHp.Value);
    }

    void Update()
    {
        float r = regenPerSec.Value;
        if (r > 0f && currentHP > 0f)
        {
            regenAccum += r * Time.deltaTime;
            if (regenAccum >= 0.1f)
            {
                Heal(regenAccum);
                regenAccum = 0f;
            }
        }

        if (debugLogs && Time.time >= debugNext)
        {
            debugNext = Time.time + debugPrintEvery;
            Debug.Log(
                $"[PlayerStats] HP {currentHP:0.0}/{maxHp.Value:0.0} | " +
                $"Move {moveSpeed.Value:0.00} | Dmg x{damage.Value:0.00} | " +
                $"AS x{attackSpeed.Value:0.00} | Range x{range.Value:0.00} | " +
                $"ProjSpeed x{projectileSpeed.Value:0.00} | Proj+{GetProjectileBonus()} | " +
                $"Money x{moneyGain.Value:0.00} | Luck {luck.Value:0.00} | " +
                $"Crit {critChance.Value:0.00} | CritDmg x{critDamage.Value:0.00} | " +
                $"Lvl {currentLevel} XP {currentXP}/{xpToNextLevel} | Crystals {totalCrystals}"
            );
        }
    }

    public void AddCrystal(int amount = 1)
    {
        if (amount <= 0) return;
        totalCrystals += amount;
        AddXP(amount);
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }
    }

    void LevelUp()
    {
        currentLevel++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f);

        if (debugLogs)
            Debug.Log($"[PlayerStats] LEVEL UP -> {currentLevel} | next XP: {xpToNextLevel}");

        OnLevelUp?.Invoke();
    }

    public float Get(PlayerStatType type) => GetBlock(type).Value;

    public int GetProjectileBonus() => Mathf.RoundToInt(projectileCount.Value);

    public float TakeDamage(float amount)
    {
        if (amount <= 0f)
            return currentHP;

        float final = amount * damageTaken.Value;
        currentHP = Mathf.Max(0f, currentHP - final);

        if (debugLogs)
            Debug.Log($"[PlayerStats] Took {final:0.0} dmg -> HP {currentHP:0.0}/{maxHp.Value:0.0}");

        return currentHP;
    }

    public float Heal(float amount)
    {
        if (amount <= 0f)
            return currentHP;

        currentHP = Mathf.Min(maxHp.Value, currentHP + amount);

        if (debugLogs)
            Debug.Log($"[PlayerStats] Healed {amount:0.0} -> HP {currentHP:0.0}/{maxHp.Value:0.0}");

        return currentHP;
    }

    public void SetCurrentHPToMax()
    {
        currentHP = maxHp.Value;
    }

    public void AddFlat(PlayerStatType type, float add)
    {
        GetBlock(type).flatAdd += add;
        PostChangeClampHP(type);
    }

    public void AddPercent(PlayerStatType type, float addPercent)
    {
        GetBlock(type).percentAdd += addPercent;
        PostChangeClampHP(type);
    }

    public void Multiply(PlayerStatType type, float mul)
    {
        GetBlock(type).multiplier *= mul;
        PostChangeClampHP(type);
    }

    public void ResetAllRuntimeBonuses()
    {
        maxHp.ResetRuntime();
        regenPerSec.ResetRuntime();
        moveSpeed.ResetRuntime();

        damage.ResetRuntime();
        attackSpeed.ResetRuntime();
        range.ResetRuntime();
        projectileSpeed.ResetRuntime();
        projectileCount.ResetRuntime();

        pickupRange.ResetRuntime();
        moneyGain.ResetRuntime();
        luck.ResetRuntime();
        itemPrice.ResetRuntime();

        damageTaken.ResetRuntime();
        dodge.ResetRuntime();

        critChance.ResetRuntime();
        critDamage.ResetRuntime();

        currentHP = Mathf.Clamp(currentHP, 0f, maxHp.Value);
    }

    StatBlock GetBlock(PlayerStatType type)
    {
        return type switch
        {
            PlayerStatType.MaxHp => maxHp,
            PlayerStatType.RegenPerSec => regenPerSec,
            PlayerStatType.MoveSpeed => moveSpeed,
            PlayerStatType.Damage => damage,
            PlayerStatType.AttackSpeed => attackSpeed,
            PlayerStatType.Range => range,
            PlayerStatType.ProjectileSpeed => projectileSpeed,
            PlayerStatType.ProjectileCount => projectileCount,
            PlayerStatType.PickupRange => pickupRange,
            PlayerStatType.MoneyGain => moneyGain,
            PlayerStatType.Luck => luck,
            PlayerStatType.DamageTaken => damageTaken,
            PlayerStatType.CritChance => critChance,
            PlayerStatType.CritDamage => critDamage,
            PlayerStatType.Dodge => dodge,
            PlayerStatType.ItemPrice => itemPrice,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    void PostChangeClampHP(PlayerStatType changed)
    {
        if (changed == PlayerStatType.MaxHp)
            currentHP = Mathf.Clamp(currentHP, 0f, maxHp.Value);
    }
}

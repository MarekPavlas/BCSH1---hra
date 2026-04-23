using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatModMode
{
    FlatAdd,
    PercentAdd,
    PercentPointsAdd,
    Multiply
}

public enum PassiveSpecialEffectType
{
    None,
    HealOnAcquire,
    NextShotBonusAfterDodge,
    LowHpStatBonus,
    OnHitBonusDamageProc
}

[Serializable]
public class PassiveStatMod
{
    public PlayerStatType stat;
    public StatModMode mode = StatModMode.FlatAdd;
    public float value = 0f;
}

[Serializable]
public class PassiveSpecialEffect
{
    public PassiveSpecialEffectType type = PassiveSpecialEffectType.None;

    public float value = 0f;

    public float secondaryValue = 0f;

    public float duration = 0f;

    public PlayerStatType targetStat = PlayerStatType.Damage;

    public StatModMode targetMode = StatModMode.PercentAdd;
}

[CreateAssetMenu(menuName = "Game/Passive Item")]
public class PassiveItemDefinition : ScriptableObject
{
    [Header("Identity")]
    public string internalId;

    [Header("UI")]
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Rarity")]
    public ItemRarity rarity = ItemRarity.COMMON;

    [Header("Shop")]
    public int shopPrice = 10;
    public int maxStacks = 1;

    [Header("Stat Mods")]
    public List<PassiveStatMod> mods = new();

    [Header("Special Effects")]
    public List<PassiveSpecialEffect> specialEffects = new();
}

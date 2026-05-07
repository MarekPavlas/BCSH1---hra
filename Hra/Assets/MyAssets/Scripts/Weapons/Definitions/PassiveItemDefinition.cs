using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatModMode
{
    FlatAdd,
    PercentAdd,
}

[Serializable]
public class PassiveStatMod
{
    public PlayerStatType stat;
    public StatModMode mode = StatModMode.FlatAdd;
    public float value = 0f;
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
}

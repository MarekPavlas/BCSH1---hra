using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Weapons/Weapon Upgrade Config")]
public class WeaponUpgradeConfig : ScriptableObject
{
    public WeaponId id;
    public string displayName;
    public Sprite icon;

    [Min(1)] public int maxLevel = 3;

    [Header("Stats per level")]
    public WeaponLevelTuning level1;
    public WeaponLevelTuning level2;
    public WeaponLevelTuning level3;

    [Header("Upgrade to Level 2")]
    public WeaponUpgradeStep upgradeToLevel2 = new WeaponUpgradeStep
    {
        price = 100,
        successChance = 10f,
        failBonus = 10f
    };

    [Header("Upgrade to Level 3")]
    public WeaponUpgradeStep upgradeToLevel3 = new WeaponUpgradeStep
    {
        price = 250,
        successChance = 10f,
        failBonus = 10f
    };

    public WeaponLevelTuning GetTuningForLevel(int level)
    {
        if (level <= 1) return level1;
        if (level == 2) return level2;
        return level3;
    }

    public WeaponUpgradeStep GetUpgradeStepForCurrentLevel(int currentLevel)
    {
        if (currentLevel <= 1) return upgradeToLevel2;
        if (currentLevel == 2) return upgradeToLevel3;
        return default;
    }
}

[Serializable]
public struct WeaponLevelTuning
{
    public float damage;
    public float fireInterval;
    public float range;
    public float projectileSpeed;
    public float aoeRadius;
    public int projectileCount;
    public int maxSimultaneous;
}

[Serializable]
public struct WeaponUpgradeStep
{
    public int price;
    [Range(0f, 100f)] public float successChance;      
    [Range(0f, 100f)] public float failBonus;           
}
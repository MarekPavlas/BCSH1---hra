using UnityEngine;

public enum WeaponId
{
    Bullet,
    Lightning,
    Boomerang,
    AOE,
    Rubber,
    Piercing,
    Laser,
    OrbitingBlades,
    RocketNapalm,
    Shotgun,
    AK47,
    MoneyStack
}

[CreateAssetMenu(menuName = "Game/Weapon Definition")]
public class WeaponDefinition : ScriptableObject
{
    [Header("Identity")]
    public WeaponId id;

    [Header("UI")]
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Rarity")]
    public ItemRarity rarity = ItemRarity.COMMON;

    [Header("Progress")]
    [Min(1)] public int maxLevel = 8;

    [Header("Shop")]
    [Min(0)] public int shopPrice = 10;
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeaponInventoryUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerWeapons playerWeapons;
    public WaveShopUI waveShopUI;

    [Header("UI")]
    public Transform container;
    public InventoryItemUI itemPrefab;
    public TextMeshProUGUI headerText;
    public string headerFormat = "Tvuj arzenal ({0})";

    readonly List<InventoryItemUI> activeItems = new();
    readonly Dictionary<WeaponId, WeaponDefinition> definitionsById = new();

    void Awake() => ResolveRefs();

    void OnEnable()
    {
        ResolveRefs();
        Subscribe();
        Refresh();
    }

    void OnDisable() => Unsubscribe();

    void ResolveRefs()
    {
        if (waveShopUI == null)
            waveShopUI = GetComponentInParent<WaveShopUI>(true);

        if (playerWeapons == null && waveShopUI != null)
            playerWeapons = waveShopUI.playerWeapons;

        if (playerWeapons == null)
            playerWeapons = FindFirstObjectByType<PlayerWeapons>();
    }

    void Subscribe()
    {
        if (playerWeapons == null) return;
        playerWeapons.OnWeaponLevelChanged -= HandleWeaponLevelChanged;
        playerWeapons.OnWeaponLevelChanged += HandleWeaponLevelChanged;
    }

    void Unsubscribe()
    {
        if (playerWeapons == null) return;
        playerWeapons.OnWeaponLevelChanged -= HandleWeaponLevelChanged;
    }

    void HandleWeaponLevelChanged(WeaponId id, int level)
    {
        if (gameObject.activeInHierarchy)
            Refresh();
    }

    void RebuildDefinitionCache()
    {
        definitionsById.Clear();

        if (playerWeapons != null && playerWeapons.weaponDefinitions != null)
        {
            foreach (var def in playerWeapons.weaponDefinitions)
                AddDefinition(def);
        }

        if (waveShopUI != null && waveShopUI.allWeapons != null)
        {
            foreach (var def in waveShopUI.allWeapons)
                AddDefinition(def);
        }
    }

    void AddDefinition(WeaponDefinition def)
    {
        if (def == null) return;
        definitionsById[def.id] = def;
    }

    WeaponDefinition GetDefinition(OwnedWeaponData weapon)
    {
        if (weapon.definition != null)
            return weapon.definition;

        definitionsById.TryGetValue(weapon.id, out var def);
        return def;
    }

    public void Refresh()
    {
        ClearItems();

        if (playerWeapons == null || container == null || itemPrefab == null)
        {
            if (headerText != null)
                headerText.text = string.Format(headerFormat, 0);
            return;
        }

        RebuildDefinitionCache();

        List<OwnedWeaponData> owned = playerWeapons.GetOwnedWeapons();
        owned.Sort((a, b) => string.Compare(GetWeaponName(a), GetWeaponName(b),
            StringComparison.OrdinalIgnoreCase));

        if (headerText != null)
            headerText.text = string.Format(headerFormat, owned.Count);

        foreach (var weapon in owned)
        {
            var definition = GetDefinition(weapon);

            var item = Instantiate(itemPrefab, container);
            int maxLevel = 3;
            Sprite icon = definition != null ? definition.icon : null;

            item.Bind(weapon.id, GetWeaponName(weapon), weapon.currentLevel, maxLevel, icon);
            activeItems.Add(item);
        }
    }

    void ClearItems()
    {
        foreach (var item in activeItems)
            if (item != null) Destroy(item.gameObject);
        activeItems.Clear();
    }

    string GetWeaponName(OwnedWeaponData weapon)
    {
        var definition = GetDefinition(weapon);

        if (definition != null && !string.IsNullOrWhiteSpace(definition.displayName))
            return definition.displayName;

        return weapon.id.ToString();
    }
}

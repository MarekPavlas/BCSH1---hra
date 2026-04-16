using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OwnedWeaponData
{
    public WeaponId id;
    public int currentLevel;
    public WeaponDefinition definition;
}

public class PlayerWeapons : MonoBehaviour
{
    [Serializable]
    public class WeaponBinding
    {
        public WeaponId id;
        public MonoBehaviour script;
    }

    [Header("Weapon Definitions")]
    public List<WeaponDefinition> weaponDefinitions;
    Dictionary<WeaponId, WeaponDefinition> defById = new();

    [Header("Setup")]
    public List<WeaponBinding> bindings = new();
    public List<WeaponId> startUnlocked = new();
    public List<WeaponUpgradeConfig> upgradeConfigs = new();
    public int defaultMaxLevel = 3;

    [Header("Debug / Test Mode")]
    public bool testMode = true;

    public WeaponId testWeapon;

    [Range(1, 10)]
    public int testLevel = 1;

    private readonly Dictionary<WeaponId, MonoBehaviour> map = new();
    private readonly Dictionary<WeaponId, int> levels = new();
    private readonly Dictionary<WeaponId, WeaponUpgradeConfig> cfgById = new();
    private readonly HashSet<WeaponId> shopTouched = new();

    public event Action<WeaponId, int> OnWeaponLevelChanged;

    void Awake()
    {
        foreach (var def in weaponDefinitions)
        {
            if (def != null)
                defById[def.id] = def;
        }

        BuildMap();
        BuildConfigMap();
        DisableAllWeaponScripts();

        if (testMode)
        {
            InitTestMode();
        }
        else
        {
            foreach (var id in startUnlocked)
                Unlock(id, false);
        }
    }

    void InitTestMode()
    {
        levels.Clear();

        if (!cfgById.ContainsKey(testWeapon))
        {
            Debug.LogWarning($"[TEST MODE] Chybi config pro {testWeapon}");
            return;
        }

        int lvl = Mathf.Clamp(testLevel, 1, GetMaxLevel(testWeapon));
        levels[testWeapon] = lvl;

        if (map.TryGetValue(testWeapon, out var script) && script != null)
        {
            script.enabled = true;
            ApplyLevelToScript(testWeapon, lvl);
        }

        Debug.Log($"[TEST MODE] Aktivni zbran: {testWeapon} (Level {lvl})");
        OnWeaponLevelChanged?.Invoke(testWeapon, lvl);
    }

    public bool HasWeapon(WeaponId id) => levels.ContainsKey(id);

    public int GetLevel(WeaponId id) =>
        levels.TryGetValue(id, out var lvl) ? lvl : 0;

    public void Unlock(WeaponId id) => Unlock(id, false);

    public void Upgrade(WeaponId id, int maxLevel) => Upgrade(id, maxLevel, false);

    public void Unlock(WeaponId id, bool fromShop)
    {
        if (testMode) return;
        if (levels.ContainsKey(id)) return;

        levels[id] = 1;

        if (map.TryGetValue(id, out var script) && script != null)
        {
            script.enabled = true;
            ApplyLevelToScript(id, 1);
        }

        if (fromShop) shopTouched.Add(id);
        OnWeaponLevelChanged?.Invoke(id, 1);
    }

    public void Upgrade(WeaponId id, int maxLevel, bool fromShop)
    {
        if (testMode)
        {
            Debug.Log("[TEST MODE] Upgrade je vypnuty");
            return;
        }

        int realMax = Mathf.Min(GetMaxLevel(id), Mathf.Max(1, maxLevel));

        if (!levels.ContainsKey(id))
        {
            Unlock(id, fromShop);
            return;
        }

        int lvl = levels[id];
        if (lvl >= realMax) return;

        levels[id] = lvl + 1;

        ApplyLevelToScript(id, lvl + 1);

        if (fromShop) shopTouched.Add(id);
        OnWeaponLevelChanged?.Invoke(id, lvl + 1);
    }

    public void RemoveWeapon(WeaponId id)
    {
        if (!levels.ContainsKey(id)) return;

        levels.Remove(id);

        if (map.TryGetValue(id, out var script) && script != null)
            script.enabled = false;
    }

    public List<OwnedWeaponData> GetOwnedWeapons()
    {
        List<OwnedWeaponData> list = new();

        foreach (var kv in levels)
        {
            defById.TryGetValue(kv.Key, out var def);

            list.Add(new OwnedWeaponData
            {
                id = kv.Key,
                currentLevel = kv.Value,
                definition = def
            });
        }

        return list;
    }

    public int GetMaxLevel(WeaponId id) =>
        cfgById.TryGetValue(id, out var cfg) ? cfg.maxLevel : defaultMaxLevel;

    public WeaponLevelTuning GetTuning(WeaponId id, int level)
    {
        if (cfgById.TryGetValue(id, out var cfg))
            return cfg.GetTuningForLevel(level);

        Debug.LogWarning($"[PlayerWeapons] Chybi WeaponUpgradeConfig pro {id}");
        return default;
    }

    void BuildMap()
    {
        map.Clear();

        foreach (var b in bindings)
        {
            if (b == null || b.script == null) continue;
            if (map.ContainsKey(b.id)) continue;

            map.Add(b.id, b.script);
        }
    }

    void BuildConfigMap()
    {
        cfgById.Clear();

        foreach (var cfg in upgradeConfigs)
        {
            if (cfg == null) continue;
            cfgById[cfg.id] = cfg;
        }
    }

    void DisableAllWeaponScripts()
    {
        foreach (var kv in map)
        {
            if (kv.Value != null)
                kv.Value.enabled = false;
        }
    }

    void ApplyLevelToScript(WeaponId id, int level)
    {
        if (!map.TryGetValue(id, out var script) || script == null) return;
        if (!cfgById.ContainsKey(id)) return;

        var tuning = GetTuning(id, level);

        if (script is IWeaponLevelApplier applier)
            applier.ApplyWeaponLevel(tuning, level);
    }

    void Update()
    {
        if (!testMode) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetTestWeapon(WeaponId.Bullet);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetTestWeapon(WeaponId.Laser);
    }

    void SetTestWeapon(WeaponId id)
    {
        if (!cfgById.ContainsKey(id))
        {
            Debug.LogWarning($"[TEST MODE] Chybi config pro {id}");
            return;
        }

        DisableAllWeaponScripts();
        levels.Clear();

        int lvl = Mathf.Clamp(testLevel, 1, GetMaxLevel(id));
        levels[id] = lvl;

        if (map.TryGetValue(id, out var script) && script != null)
        {
            script.enabled = true;
            ApplyLevelToScript(id, lvl);
        }

        Debug.Log($"[TEST MODE] Prepnuto na {id} (Level {lvl})");
        OnWeaponLevelChanged?.Invoke(id, lvl);
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PassiveInventory : MonoBehaviour
{
    [Header("Refs")]
    public PlayerStats stats;

    [Header("Debug")]
    public bool debugLogs = true;
    public bool debugKeyPPrint = true; 

    private readonly Dictionary<PassiveItemDefinition, int> stacks = new();

    void Awake()
    {
        if (stats == null) stats = GetComponent<PlayerStats>();
        if (stats == null) Debug.LogError("[PassiveInventory] Chybí PlayerStats na hráči!");
    }

    void Update()
    {
        if (debugKeyPPrint && Input.GetKeyDown(KeyCode.P))
            DebugPrint();
    }

    public int GetStacks(PassiveItemDefinition def)
        => def != null && stacks.TryGetValue(def, out var s) ? s : 0;

    public bool CanAdd(PassiveItemDefinition def)
    {
        if (def == null) return false;
        int cur = GetStacks(def);
        return cur < Mathf.Max(1, def.maxStacks);
    }

    public bool TryAdd(PassiveItemDefinition def, int amount = 1)
    {
        if (def == null || stats == null) return false;
        if (amount <= 0) amount = 1;

        int cur = GetStacks(def);
        int max = Mathf.Max(1, def.maxStacks);
        if (cur >= max) return false;

        int add = Mathf.Min(amount, max - cur);
        stacks[def] = cur + add;

        RebuildStats();

        if (debugLogs)
            Debug.Log($"[PassiveInventory] +{add}x {def.displayName} (now {stacks[def]}/{max})");

        return true;
    }

    public void RebuildStats()
    {
        if (stats == null) return;

        stats.ResetAllRuntimeBonuses();

        foreach (var kv in stacks)
        {
            var def = kv.Key;
            int s = kv.Value;

            if (def == null || def.mods == null) continue;

            foreach (var m in def.mods)
            {
                ApplyMod(m, s);
            }
        }

        stats.SetCurrentHPToMax();
    }

    void ApplyMod(PassiveStatMod mod, int stacksCount)
    {
        if (mod == null) return;

        float v = mod.value;

        switch (mod.mode)
        {
            case StatModMode.FlatAdd:
                stats.AddFlat(mod.stat, v * stacksCount);
                break;

            case StatModMode.PercentAdd:
                stats.AddPercent(mod.stat, (mod.value / 100f) * stacksCount);
                break;

            case StatModMode.Multiply:
                float mul = Mathf.Pow(v, stacksCount);
                stats.Multiply(mod.stat, mul);
                break;
        }
    }

    public void DebugPrint()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== PASSIVES (owned) ===");

        if (stacks.Count == 0) sb.AppendLine("(none)");

        foreach (var kv in stacks.OrderBy(k => k.Key.displayName))
            sb.AppendLine($"- {kv.Key.displayName} x{kv.Value}");

        Debug.Log(sb.ToString());
    }
}
using UnityEngine;
using TMPro;
using System.Reflection;

public class HealthUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerStats stats;
    public TMP_Text label;

    [Header("Text")]
    public string prefix = "HP: ";
    public bool roundToInt = true;

    [Header("Death FX")]
    public Color aliveColor = Color.white;
    public Color deadColor = Color.red;
    public float deadScaleMul = 1.2f; 

    float _lastCur = -9999f;
    float _lastMax = -9999f;
    bool _lastDead = false;

    Vector3 _baseScale;

    void Awake()
    {
        if (stats == null) stats = FindFirstObjectByType<PlayerStats>();
        if (label == null) label = GetComponentInChildren<TMP_Text>();
        if (label != null) _baseScale = label.rectTransform.localScale;
    }

    void Update()
    {
        if (stats == null || label == null) return;

        float cur = ReadFloat(stats, "currentHP", "currentHp", "CurrentHP", "CurrentHp");
        float max = ReadStatBlockValue(stats, "MaxHp", "MaxHP", "maxHp", "maxHP");

        if (max <= 0f) max = Mathf.Max(cur, 1f);

        bool dead = cur <= 0.01f;

        if (!Mathf.Approximately(cur, _lastCur) || !Mathf.Approximately(max, _lastMax))
        {
            _lastCur = cur;
            _lastMax = max;

            if (roundToInt)
                label.text = $"{prefix}{Mathf.CeilToInt(cur)}/{Mathf.CeilToInt(max)}";
            else
                label.text = $"{prefix}{cur:0.0}/{max:0.0}";
        }

        if (dead != _lastDead)
        {
            _lastDead = dead;
            label.color = dead ? deadColor : aliveColor;
            label.rectTransform.localScale = dead ? _baseScale * deadScaleMul : _baseScale;
        }
    }

    static float ReadFloat(object obj, params string[] names)
    {
        var t = obj.GetType();
        foreach (var n in names)
        {
            var f = t.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(float))
                return (float)f.GetValue(obj);

            var p = t.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.PropertyType == typeof(float))
                return (float)p.GetValue(obj);
        }
        return 0f;
    }
        
    static float ReadStatBlockValue(object obj, params string[] names)
    {
        var t = obj.GetType();
        foreach (var n in names)
        {
            var f = t.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null)
            {
                object sb = f.GetValue(obj);
                float v = TryGetStatBlockValue(sb);
                if (v > 0f) return v;
            }

            var p = t.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null)
            {
                object sb = p.GetValue(obj);
                float v = TryGetStatBlockValue(sb);
                if (v > 0f) return v;
            }
        }
        return 0f;
    }

    static float TryGetStatBlockValue(object sb)
    {
        if (sb == null) return 0f;
        var sbType = sb.GetType();
        var valueProp = sbType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (valueProp != null && valueProp.PropertyType == typeof(float))
            return (float)valueProp.GetValue(sb);
        return 0f;
    }
}
using System.Reflection;
using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    [Header("Refs")]
    public CurrencyWallet wallet;
    public TMP_Text label;

    [Header("Format")]
    public string prefix = "Money: ";
    public string suffix = "";
    public float refreshRate = 0.1f; 

    int _lastValue = int.MinValue;
    float _nextRefresh;
    bool _warned;

    static readonly string[] MoneyPropertyNames =
    {
        "Money", "money", "CurrentMoney", "currentMoney", "Balance", "balance"
    };

    static readonly string[] MoneyMethodNames =
    {
        "GetMoney", "GetCurrentMoney", "GetBalance"
    };

    void Awake()
    {
        if (label == null) label = GetComponent<TMP_Text>();
        if (wallet == null) wallet = FindFirstObjectByType<CurrencyWallet>();
    }

    void Update()
    {
        if (Time.time < _nextRefresh) return;
        _nextRefresh = Time.time + refreshRate;

        if (wallet == null || label == null) return;

        int money = ReadMoney(wallet);

        if (money != _lastValue)
        {
            _lastValue = money;
            label.text = $"{prefix}{money}{suffix}";
        }
    }

    int ReadMoney(CurrencyWallet w)
    {
        var type = w.GetType();

        foreach (var name in MoneyPropertyNames)
        {
            var p = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (p != null && p.PropertyType == typeof(int))
                return (int)p.GetValue(w);
        }

        foreach (var name in MoneyPropertyNames)
        {
            var f = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(int))
                return (int)f.GetValue(w);
        }

        foreach (var name in MoneyMethodNames)
        {
            var m = type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
            if (m != null && m.ReturnType == typeof(int) && m.GetParameters().Length == 0)
                return (int)m.Invoke(w, null);
        }

        if (!_warned)
        {
            _warned = true;
            Debug.LogWarning("[MoneyUI] Nemůžu najít peníze v CurrencyWallet. Přidej např. public int Money => money; nebo public int GetMoney().");
        }

        return 0;
    }
}
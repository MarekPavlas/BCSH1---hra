using UnityEngine;
using System; 

public class CurrencyWallet : MonoBehaviour
{
    [SerializeField] private int money = 0;

    public event Action<int> OnCurrencyChanged;

    public int Money => money;

    public int GetCurrentAmount()
    {
        return money;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        money += amount;

        Debug.Log($"[Wallet] +{amount} money => {money}");
        OnCurrencyChanged?.Invoke(money);
    }

    public bool CanAfford(int cost)
    {
        return money >= cost;
    }

    public bool TrySpend(int cost)
    {
        if (cost <= 0) return true;
        if (money < cost) return false;

        money -= cost;

        Debug.Log($"[Wallet] -{cost} money => {money}");
        OnCurrencyChanged?.Invoke(money);

        return true;
    }
}
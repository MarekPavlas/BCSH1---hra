using System;
using UnityEngine;

[Serializable]
public class SaveStatsData
{
    public int deaths;
    public int wins;
    public int enemiesKilled;
    public int totalMoneyEarned;
}

public class SaveStatsManager : MonoBehaviour
{
    public static SaveStatsManager Instance { get; private set; }

    public SaveStatsData Data { get; private set; } = new SaveStatsData();

    const string SaveKey = "PLAYER_SAVE_STATS";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void AddDeath()
    {
        Data.deaths++;
        Save();
    }

    public void AddWin()
    {
        Data.wins++;
        Save();
    }

    public void AddEnemyKill()
    {
        Data.enemiesKilled++;
    }

    public void AddMoneyEarned(int amount)
    {
        if (amount > 0)
            Data.totalMoneyEarned += amount;
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(Data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log("[SAVE] Saved: " + json);
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Data = new SaveStatsData();
            Debug.Log("[SAVE] No save found. New data created.");
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        Data = JsonUtility.FromJson<SaveStatsData>(json);

        if (Data == null)
            Data = new SaveStatsData();

        Debug.Log("[SAVE] Loaded: " + json);
    }

    public void ResetSave()
    {
        Data = new SaveStatsData();
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        Debug.Log("[SAVE] Save reset.");
    }
}
using System;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Config")]
    [Min(1)] public int totalWaves = 20;
    [Min(1f)] public float waveDuration = 30f;
    [Min(0f)] public float intermissionDuration = 5f;

    [Header("Test Mode")]
    public bool testModeNoShop = false;
    public bool testSkipIntermission = false;

    [Header("End Wave Cleanup")]
    public bool killEnemiesOnWaveEnd = true;
    public float killDamage = 999999f;

    public bool suppressDropsOnWaveEndKill = true;

    public bool despawnMoneyOnWaveEnd = true;

    [Header("Debug")]
    public bool debugLogs = true;
    public float debugEverySeconds = 1f;

    public int CurrentWave { get; private set; } = 0;
    public float WaveTimeLeft { get; private set; } = 0f;
    public bool IsWaveRunning { get; private set; } = false;
    public bool IsIntermission { get; private set; } = false;

    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveEnded;
    public event Action<int> OnIntermissionStarted;
    public event Action OnAllWavesCompleted;

    float debugNextTime;

    void Start() => StartNextWave();

    void Update()
    {
        if (IsWaveRunning)
        {
            WaveTimeLeft -= Time.deltaTime;

            if (debugLogs && Time.time >= debugNextTime)
            {
                debugNextTime = Time.time + debugEverySeconds;
                float progress = 1f - Mathf.Clamp01(WaveTimeLeft / waveDuration);
                Debug.Log($"[WAVE] {CurrentWave}/{totalWaves} running | left={WaveTimeLeft:0.0}s | progress={(progress * 100f):0}%");
            }

            if (WaveTimeLeft <= 0f)
                EndWave();
        }
        else if (IsIntermission)
        {
            if (testModeNoShop && testSkipIntermission)
            {
                if (debugLogs) Debug.Log($"[WAVE] TEST MODE: skipping intermission -> starting wave {CurrentWave + 1}");
                StartNextWave();
                return;
            }

            WaveTimeLeft -= Time.deltaTime;

            if (debugLogs && Time.time >= debugNextTime)
            {
                debugNextTime = Time.time + debugEverySeconds;
                Debug.Log($"[WAVE] Intermission before wave {CurrentWave + 1} | left={WaveTimeLeft:0.0}s");
            }

            if (WaveTimeLeft <= 0f)
                StartNextWave();
        }
    }

    void StartNextWave()
    {
        IsIntermission = false;

        CurrentWave++;
        if (CurrentWave > totalWaves)
        {
            IsWaveRunning = false;
            if (debugLogs) Debug.Log("[WAVE] All waves completed!");
            OnAllWavesCompleted?.Invoke();
            return;
        }

        IsWaveRunning = true;
        WaveTimeLeft = waveDuration;
        debugNextTime = Time.time + debugEverySeconds;

        if (debugLogs) Debug.Log($"[WAVE] START wave {CurrentWave}/{totalWaves}");
        OnWaveStarted?.Invoke(CurrentWave);
    }

    void EndWave()
    {
        IsWaveRunning = false;

        if (debugLogs) Debug.Log($"[WAVE] END wave {CurrentWave}/{totalWaves}");

        if (killEnemiesOnWaveEnd)
        {
            int killed = KillAllEnemiesByEnemyHealth();
            if (debugLogs) Debug.Log($"[WAVE] Killed enemies = {killed}");
        }

        if (despawnMoneyOnWaveEnd)
        {
            int removed = DespawnAllMoneyPickups();
            if (debugLogs) Debug.Log($"[WAVE] Despawned MoneyPickup = {removed}");
        }

        OnWaveEnded?.Invoke(CurrentWave);

        if (CurrentWave >= totalWaves)
        {
            if (debugLogs) Debug.Log("[WAVE] All waves completed!");
            OnAllWavesCompleted?.Invoke();
            return;
        }

        IsIntermission = true;
        WaveTimeLeft = intermissionDuration;
        debugNextTime = Time.time + debugEverySeconds;

        OnIntermissionStarted?.Invoke(CurrentWave + 1);
    }

    public void StartNextWaveNow()
    {
        if (IsWaveRunning) return;
        if (!IsIntermission) return;

        IsIntermission = false;
        WaveTimeLeft = 0f;
        StartNextWave();
    }

    [ContextMenu("Force End Wave")]
    public void ForceEndWave()
    {
        if (IsWaveRunning) WaveTimeLeft = 0.01f;
    }

    int KillAllEnemiesByEnemyHealth()
    {
        var enemies = FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int killed = 0;

        foreach (var eh in enemies)
        {
            if (eh == null) continue;

            if (suppressDropsOnWaveEndKill)
            {
                var drop = eh.GetComponent<EnemyDrop>();
                if (drop != null) drop.SuppressNextDrop();
            }

            eh.TakeDamage(killDamage);
            killed++;
        }

        return killed;
    }

    int DespawnAllMoneyPickups()
    {
        var money = FindObjectsByType<MoneyPickup>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int removed = 0;

        foreach (var m in money)
        {
            if (m == null) continue;
            Destroy(m.gameObject);
            removed++;
        }

        return removed;
    }
}
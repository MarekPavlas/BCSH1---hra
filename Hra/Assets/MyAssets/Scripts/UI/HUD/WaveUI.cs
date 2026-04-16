using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [Header("Refs")]
    public WaveManager waveManager;
    public TMP_Text waveLabel;

    [Header("Settings")]
    public string wavePrefix = "Vlna: ";
    public string intermissionPrefix = "Další vlna za: ";

    void Start() 
    {
        if (waveManager == null) waveManager = Object.FindFirstObjectByType<WaveManager>();

        if (waveLabel == null) waveLabel = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (waveManager == null || waveLabel == null) return;

        if (waveManager.IsWaveRunning)
        {
            waveLabel.text = $"{wavePrefix}{waveManager.CurrentWave}/{waveManager.totalWaves} ({Mathf.CeilToInt(waveManager.WaveTimeLeft)}s)";
        }
        else if (waveManager.IsIntermission)
        {
            waveLabel.text = $"{intermissionPrefix}{Mathf.CeilToInt(waveManager.WaveTimeLeft)}s";
        }
        else if (waveManager.CurrentWave >= waveManager.totalWaves)
        {
            waveLabel.text = "HOTOVO!";
        }
    }
}
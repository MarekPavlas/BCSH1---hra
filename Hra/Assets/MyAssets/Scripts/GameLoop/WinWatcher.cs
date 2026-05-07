using UnityEngine;

public class WinWatcher : MonoBehaviour
{
    public WaveManager waveManager;
    public GameOverManager gameOverManager;

    bool won;

    void Awake()
    {
        if (waveManager == null)
            waveManager = FindFirstObjectByType<WaveManager>();

        if (gameOverManager == null)
            gameOverManager = FindFirstObjectByType<GameOverManager>();
    }

    void OnEnable()
    {
        if (waveManager != null)
            waveManager.OnAllWavesCompleted += HandleWin;
    }

    void OnDisable()
    {
        if (waveManager != null)
            waveManager.OnAllWavesCompleted -= HandleWin;
    }

    void HandleWin()
    {
        if (won) return;
        won = true;

        if (gameOverManager != null)
            gameOverManager.ShowWinScreen();
    }
}
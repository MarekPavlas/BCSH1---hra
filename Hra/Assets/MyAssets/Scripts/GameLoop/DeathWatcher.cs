using UnityEngine;

public class DeathWatcher : MonoBehaviour
{
    public PlayerStats playerStats;
    public GameOverManager gameOverManager;

    bool dead;

    void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (gameOverManager == null)
            gameOverManager = FindFirstObjectByType<GameOverManager>();
    }

    void Update()
    {
        if (dead) return;
        if (playerStats == null || gameOverManager == null) return;

        if (playerStats.currentHP <= 0f)
        {
            dead = true;
            gameOverManager.ShowDeathScreen();
        }
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject deathPanel;
    public GameObject winPanel;

    [Header("Texts")]
    public TMP_Text deathStatsText;
    public TMP_Text winStatsText;

    [Header("Camera")]
    public GameObject cinemachineCameraObject;

    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";

    bool gameEnded = false;

    void Start()
    {
        if (deathPanel != null)
            deathPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        if (cinemachineCameraObject == null)
            cinemachineCameraObject = GameObject.Find("CinemachineCamera");
    }

    public void ShowDeathScreen()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        EndGameCommon();

        if (SaveStatsManager.Instance != null)
        {
            SaveStatsManager.Instance.AddDeath();
            SaveStatsManager.Instance.Save();
        }

        if (deathStatsText != null)
            deathStatsText.text = BuildStatsText();

        if (deathPanel != null)
            deathPanel.SetActive(true);
    }

    public void ShowWinScreen()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        EndGameCommon();

        if (SaveStatsManager.Instance != null)
        {
            SaveStatsManager.Instance.AddWin();
            SaveStatsManager.Instance.Save();
        }

        if (winStatsText != null)
            winStatsText.text = BuildStatsText();

        if (winPanel != null)
            winPanel.SetActive(true);
    }

    void EndGameCommon()
    {
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cinemachineCameraObject != null)
            cinemachineCameraObject.SetActive(false);
    }

    public void RestartMap()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    string BuildStatsText()
    {
        if (SaveStatsManager.Instance == null)
            return "Stats not found.";

        SaveStatsData data = SaveStatsManager.Instance.Data;

        return
            $"Deaths: {data.deaths}\n" +
            $"Wins: {data.wins}\n" +
            $"Enemies killed: {data.enemiesKilled}\n" +
            $"Total money earned: {data.totalMoneyEarned}";
    }
}
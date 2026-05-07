using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject mapSelectPanel;
    public GameObject statsPanel;

    [Header("Stats UI")]
    public TMP_Text statsText;

    void Start()
    {
        if (mainPanel != null)
            mainPanel.SetActive(true);

        if (mapSelectPanel != null)
            mapSelectPanel.SetActive(false);

        if (statsPanel != null)
            statsPanel.SetActive(false);
    }

    public void OpenMapSelect()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);

        if (mapSelectPanel != null)
            mapSelectPanel.SetActive(true);

        if (statsPanel != null)
            statsPanel.SetActive(false);
    }

    public void BackToMainMenu()
    {
        if (mapSelectPanel != null)
            mapSelectPanel.SetActive(false);

        if (statsPanel != null)
            statsPanel.SetActive(false);

        if (mainPanel != null)
            mainPanel.SetActive(true);
    }

    public void OpenStats()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);

        if (mapSelectPanel != null)
            mapSelectPanel.SetActive(false);

        if (statsPanel != null)
            statsPanel.SetActive(true);

        RefreshStatsText();
    }

    public void CloseStats()
    {
        BackToMainMenu();
    }

    public void ResetStats()
    {
        if (SaveStatsManager.Instance != null)
        {
            SaveStatsManager.Instance.ResetSave();
            RefreshStatsText();
        }
    }

    void RefreshStatsText()
    {
        if (statsText == null)
            return;

        if (SaveStatsManager.Instance == null)
        {
            statsText.text = "SaveStatsManager not found!";
            return;
        }

        var data = SaveStatsManager.Instance.Data;

        statsText.text =
            $"Deaths: {data.deaths}\n" +
            $"Wins: {data.wins}\n" +
            $"Enemies killed: {data.enemiesKilled}\n" +
            $"Total money earned: {data.totalMoneyEarned}";
    }

    public void LoadForestMap()
    {
        SceneManager.LoadScene("ForestMap");
    }

    public void LoadMarsMap()
    {
        SceneManager.LoadScene("MarsMap");
    }

    public void LoadMountainMap()
    {
        SceneManager.LoadScene("MountainMap");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit game");
    }
}
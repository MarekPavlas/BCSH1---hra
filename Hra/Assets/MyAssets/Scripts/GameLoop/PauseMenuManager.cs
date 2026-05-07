using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;

    [Header("Camera")]
    public GameObject cinemachineCameraObject;

    [Header("Disable While Paused")]
    public MonoBehaviour[] disableWhilePaused;

    [Header("Blocked Panels")]
    public GameObject deathPanel;
    public GameObject winPanel;
    public GameObject shopPanel;

    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";

    bool isPaused;
    float previousTimeScale = 1f;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (cinemachineCameraObject == null)
            cinemachineCameraObject = GameObject.Find("CinemachineCamera");

        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsBlockedByOtherPanel())
                return;

            TogglePause();
        }
    }

    bool IsBlockedByOtherPanel()
    {
        if (deathPanel != null && deathPanel.activeSelf)
            return true;

        if (winPanel != null && winPanel.activeSelf)
            return true;

        if (shopPanel != null && shopPanel.activeSelf)
            return true;

        return false;
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (cinemachineCameraObject != null)
            cinemachineCameraObject.SetActive(false);

        foreach (MonoBehaviour script in disableWhilePaused)
        {
            if (script != null)
                script.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (cinemachineCameraObject != null)
            cinemachineCameraObject.SetActive(true);

        foreach (MonoBehaviour script in disableWhilePaused)
        {
            if (script != null)
                script.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartMap()
    {
        Time.timeScale = 1f;

        if (cinemachineCameraObject != null)
            cinemachineCameraObject.SetActive(true);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        if (cinemachineCameraObject != null)
            cinemachineCameraObject.SetActive(true);

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit game");
    }
}
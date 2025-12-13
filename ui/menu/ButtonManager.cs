using Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject MainMenuUI;
    private bool isPaused = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                EnablePauseMenu();
            }
        }
    }
    public void EnablePauseMenu()
    {
        pauseMenuUI.SetActive(true);
        isPaused = true;
        Time.timeScale = 0f;
    }
    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        GameManager.Instance.RestartRace();
    }
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        GameManager.Instance.QuitToMainMenu();
    }
    public void LaunchGame()
    {
        SceneManager.LoadScene("TrackLevel");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Used by the Play button on your Main Menu
    public void StartGame()
    {
        Time.timeScale = 1f; // Ensure time is running
        SceneManager.LoadScene("SampleScene");
    }

    // --- ADD THESE FOR YOUR VICTORY BUTTONS ---

    public void RestartGame()
    {
        Time.timeScale = 1f; // Reset time in case you were paused
        // This reloads whatever scene you are currently playing
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Make sure your menu scene is named this
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit!");
        Application.Quit();
    }
}
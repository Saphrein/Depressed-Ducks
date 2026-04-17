using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Required for the new Input System

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;    // Drag your Pause Panel here
    public PlayerController player;  // Drag your Player here
    private bool isPaused = false;

    // This method name must match the Action name in your Input Actions (e.g., "Pause")
    public void OnPause(InputValue value)
    {
        if (value.isPressed)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Unfreezes the game
        isPaused = false;

        if (player != null) player.canMove = true;
        AudioListener.pause = false; // Optional: Unpause BGM
    }

    public void Restart()
    {
        Time.timeScale = 1f; // Unfreeze the world
        AudioListener.pause = false; // Unpause the sound
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload level
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Freezes everything (physics, timers)
        isPaused = true;

        if (player != null) player.canMove = false;
        AudioListener.pause = true; // Optional: Pause BGM
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f; // ALWAYS reset time scale before changing scenes
        AudioListener.pause = false;
        SceneManager.LoadScene("MainMenu");
    }
}
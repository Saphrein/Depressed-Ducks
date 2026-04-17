using UnityEngine;
using TMPro; // Make sure you have TextMeshPro installed!

public class GameTimer : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;

    private float elapsedTime;
    private bool isTimerRunning = false;

    void Start()
    {
        // Start counting as soon as the level loads
        StartTimer();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        isTimerRunning = true;
    }

    public float StopTimer()
    {
        isTimerRunning = false;
        return elapsedTime; // Returns the final time for the leaderboard
    }

    void UpdateTimerDisplay()
    {
        // Logic to turn seconds into Minutes : Seconds . Milliseconds
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        int fraction = Mathf.FloorToInt((elapsedTime * 100) % 100);

        timerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, fraction);
    }
}
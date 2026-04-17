using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using TMPro;
using System.Threading.Tasks;

public class GlobalLeaderboard : MonoBehaviour
{
    [Header("UI Groups")]
    public GameObject leaderboardGroup;
    public GameObject buttonGroup;
    public TextMeshProUGUI display;

    [Header("Settings")]
    private const string LeaderboardId = "Enraged";
    private bool isInitialized = false;

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                // Set a default name if they don't have one
                string currentName = await AuthenticationService.Instance.GetPlayerNameAsync();
                if (string.IsNullOrEmpty(currentName))
                {
                    await AuthenticationService.Instance.UpdatePlayerNameAsync("Climber" + Random.Range(100, 999));
                }
            }

            isInitialized = true;
            Debug.Log("UGS Initialized!");
            LoadScores();
        }
        catch (System.Exception e)
        {
            Debug.LogError("UGS Initialization Failed: " + e.Message);
        }
    }

    public async void AddScore(float time)
    {
        if (!isInitialized) return;

        try
        {
            // 1. Submit the score
            await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, time);

            // 2. Refresh the display
            LoadScores();

            // 3. Start the timed sequence
            StartCoroutine(VictorySequence());
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to submit score: " + e.Message);
            // Show buttons anyway if the internet fails so the player isn't stuck
            buttonGroup.SetActive(true);
        }
    }

    IEnumerator VictorySequence()
    {
        // Ensure buttons are hidden and leaderboard is shown
        leaderboardGroup.SetActive(true);
        buttonGroup.SetActive(false);

        // Wait for 5 seconds for them to admire their rank
        yield return new WaitForSeconds(5f);

        // Show the Retry/Menu buttons
        buttonGroup.SetActive(true);
    }

    public async void LoadScores()
    {
        if (!isInitialized) return;

        try
        {
            var scores = await LeaderboardsService.Instance.GetScoresAsync(LeaderboardId);
            display.text = "ENRAGED LEADERS:\n";
            foreach (var entry in scores.Results)
            {
                string playerName = !string.IsNullOrEmpty(entry.PlayerName) ? entry.PlayerName : "Anonymous";
                display.text += $"{entry.Rank + 1}. {playerName} - {entry.Score:F2}s\n";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not load scores: " + e.Message);
        }
    }
}
using UnityEngine;

public class VictoryChest : MonoBehaviour
{
    [Header("UI & References")]
    public GameObject victoryPanel;
    public GameTimer gameTimer;

    [Header("Global Leaderboard")]
    // Drag your GlobalManager object (with the GlobalLeaderboard script) here
    public GlobalLeaderboard globalLeaderboard;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Check if the object touching the chest is the Player
        if (other.CompareTag("Player"))
        {
            Debug.Log("Victory! Player touched the chest.");

            float finalTime = 0;

            // 2. Stop the Timer and get the final time
            if (gameTimer != null)
            {
                finalTime = gameTimer.StopTimer();
            }

            // 3. Show the Victory UI
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }

            // 4. Submit the score to the Global Leaderboard
            if (globalLeaderboard != null)
            {
                globalLeaderboard.AddScore(finalTime);
            }

            // 5. Stop the Player from moving further
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.canMove = false;
            }

            // 6. Freeze the physics so they don't slide past the chest
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0;
                // Optional: makes the player ignore further gravity/force
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }
}
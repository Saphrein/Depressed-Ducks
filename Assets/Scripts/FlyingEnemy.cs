using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public float chaseSpeed = 4f;
    public float hoverAmplitude = 0.5f;
    public float hoverFrequency = 2f;

    [Header("Detection")]
    public float visionDistance = 5f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    private Vector2 anchorPos; // This moves horizontally
    private int direction = 1;
    private bool isChasing = false;
    private Transform playerTransform;

    void Start()
    {
        anchorPos = transform.position;
    }

    void Update()
    {
        DetectPlayer();

        if (isChasing && playerTransform != null)
        {
            ExecuteChase();
        }
        else
        {
            ExecutePatrol();
        }
    }

    void DetectPlayer()
    {
        // 1. Check if the player is within range using a circle
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, visionDistance, playerLayer);

        if (playerCollider != null)
        {
            // 2. We found a player! Now check if there is a wall in the way
            Vector2 directionToPlayer = (playerCollider.transform.position - transform.position).normalized;
            float distanceToPlayer = Vector2.Distance(transform.position, playerCollider.transform.position);

            // Raycast only looks for Obstacles (Walls/Floor)
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);

            if (hit.collider == null)
            {
                // No wall in between! Attack!
                isChasing = true;
                playerTransform = playerCollider.transform;
                return;
            }
        }

        // If we get here, no player was seen or a wall was in the way
        isChasing = false;
    }

    void ExecutePatrol()
    {
        // Move the anchor point
        anchorPos.x += direction * speed * Time.deltaTime;

        // Apply hover to the anchor
        float hoverOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        transform.position = new Vector3(anchorPos.x, anchorPos.y + hoverOffset, transform.position.z);
    }

    void ExecuteChase()
    {
        // Move directly toward player
        transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, chaseSpeed * Time.deltaTime);

        // Update anchorPos so it doesn't "snap" back when it loses the player
        anchorPos = transform.position;

        // Flip to face player
        float dirToPlayer = playerTransform.position.x - transform.position.x;
        if ((dirToPlayer > 0 && direction < 0) || (dirToPlayer < 0 && direction > 0))
        {
            Flip();
        }
    }

    public void ForceFlip() { Flip(); }

    private void Flip()
    {
        direction *= -1;
        transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision);
    }

    // Backup in case the enemy moves too fast and "skips" the entrance frame
    private void OnTriggerStay2D(Collider2D collision)
    {
        HandleCollision(collision);
    }

    private void HandleCollision(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            Flip();
            // Force the anchor to move away from the wall immediately
            anchorPos.x += direction * 0.2f;

            // Sync the current position to the anchor so it doesn't "pull" back into the wall
            transform.position = new Vector3(anchorPos.x, transform.position.y, transform.position.z);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionDistance);
    }
}
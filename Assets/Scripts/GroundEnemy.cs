using UnityEngine;

public class GroundEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 5f;

    [Header("Detection")]
    public float visionDistance = 6f;
    public LayerMask playerLayer;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private int moveDirection = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    private void FixedUpdate()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, visionDistance, playerLayer);

        if (hit != null)
        {
            Vector2 directionToPlayer = (hit.transform.position - transform.position).normalized;

            // --- NEW: Check if the player is in front of the mushroom ---
            // moveDirection is 1 (right) or -1 (left). 
            // We check if the player is within a 60-degree cone in front.
            float angle = Vector2.Angle(new Vector2(moveDirection, 0), directionToPlayer);

            if (angle < 60f) // Adjust this number to make the vision wider or narrower
            {
                float distanceToPlayer = Vector2.Distance(transform.position, hit.transform.position);
                RaycastHit2D sightBlocker = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, playerLayer | groundLayer);

                if (sightBlocker.collider != null && ((1 << sightBlocker.collider.gameObject.layer) & playerLayer) != 0)
                {
                    DamageSystem ds = hit.GetComponent<DamageSystem>();
                    if (ds != null && !ds.IsGhosted && !ds.IsInBerserkState)
                    {
                        ExecuteChase(hit.transform.position);
                        return;
                    }
                }
            }
        }

        ExecutePatrol();
    }

    private void ExecuteChase(Vector3 target)
    {
        float dir = Mathf.Sign(target.x - transform.position.x);
        if (Mathf.Sign(moveDirection) != dir) Flip();
        rb.linearVelocity = new Vector2(chaseSpeed * moveDirection, rb.linearVelocity.y);
        CheckObstacles();
    }

    private void ExecutePatrol()
    {
        rb.linearVelocity = new Vector2(patrolSpeed * moveDirection, rb.linearVelocity.y);
        CheckObstacles();
    }

    private void CheckObstacles()
    {
        Vector2 wallOrigin = (Vector2)transform.position + new Vector2(col.bounds.extents.x * moveDirection, 0);
        bool hitWall = Physics2D.Raycast(wallOrigin, Vector2.right * moveDirection, 0.2f, groundLayer);

        Vector2 ledgeOrigin = (Vector2)transform.position + new Vector2((col.bounds.extents.x + 0.1f) * moveDirection, 0);
        bool isAtLedge = !Physics2D.Raycast(ledgeOrigin, Vector2.down, col.bounds.extents.y + 0.5f, groundLayer);

        if (hitWall || isAtLedge) Flip();
    }

    public void ForceFlip() { Flip(); }

    private void Flip()
    {
        moveDirection *= -1;
        transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * moveDirection, transform.localScale.y, transform.localScale.z);
    }
}
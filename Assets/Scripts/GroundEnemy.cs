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
        DamageSystem ds = hit != null ? hit.GetComponent<DamageSystem>() : null;

        // If player is in a special state, just ignore them and patrol
        if (ds != null && !ds.IsGhosted && !ds.IsInBerserkState)
        {
            ExecuteChase(hit.transform.position);
        }
        else
        {
            ExecutePatrol();
        }
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
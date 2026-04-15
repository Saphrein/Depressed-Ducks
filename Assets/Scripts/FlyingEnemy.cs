using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 5f;

    [Header("Territory")]
    public float visionRadius = 8f;
    public float patrolRange = 5f;
    public LayerMask playerLayer;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private int moveDirection = 1;
    private Vector2 homePosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        homePosition = transform.position;
    }

    private void FixedUpdate()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, visionRadius, playerLayer);
        DamageSystem ds = hit != null ? hit.GetComponent<DamageSystem>() : null;

        // Only chase if player is visible and NOT scary/invisible
        if (ds != null && !ds.IsGhosted && !ds.IsInBerserkState && HasLineOfSight(hit.transform.position))
        {
            ExecuteChase(hit.transform.position);
        }
        else
        {
            ExecuteNormalPatrol();
        }
    }

    private void ExecuteNormalPatrol()
    {
        float xDist = transform.position.x - homePosition.x;
        float yDist = transform.position.y - homePosition.y;

        if (xDist > patrolRange && moveDirection == 1) Flip();
        else if (xDist < -patrolRange && moveDirection == -1) Flip();

        // Magnet effect to pull bird back to the green line height
        float verticalSnap = -yDist * 4f;
        rb.linearVelocity = new Vector2(patrolSpeed * moveDirection, verticalSnap);
    }

    private void ExecuteChase(Vector3 target)
    {
        Vector2 dir = ((Vector2)target - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * chaseSpeed;
        if (dir.x > 0.1f && moveDirection == -1) Flip();
        else if (dir.x < -0.1f && moveDirection == 1) Flip();
    }

    private bool HasLineOfSight(Vector3 target)
    {
        Vector2 dir = (target - transform.position).normalized;
        float dist = Vector2.Distance(transform.position, target);
        return Physics2D.Raycast(transform.position, dir, dist, groundLayer).collider == null;
    }

    public void ForceFlip() { Flip(); }

    private void Flip()
    {
        moveDirection *= -1;
        transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * moveDirection, transform.localScale.y, transform.localScale.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 home = Application.isPlaying ? (Vector3)homePosition : transform.position;
        Gizmos.DrawLine(home + Vector3.left * patrolRange, home + Vector3.right * patrolRange);
    }
}
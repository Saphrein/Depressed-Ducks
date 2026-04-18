using UnityEngine;

public class GroundEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 5f;

    [Header("Detection")]
    public float visionDistance = 6f;
    public LayerMask playerLayer;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private int moveDirection = 1;

    private float flipCooldown = 0f;
    private const float FLIP_COOLDOWN = 1.0f;

    // Stuck detection
    private float lastPositionX;
    private float stuckTimer = 0f;
    private const float STUCK_TIME = 0.15f;
    private const float STUCK_DIST = 0.005f;

    // The platform the enemy is currently standing on
    private Collider2D currentPlatform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;
    }

    private void Start()
    {
        lastPositionX = transform.position.x;
        ApplyVisual();
        FindCurrentPlatform();
    }

    private void FixedUpdate()
    {
        if (flipCooldown > 0f) flipCooldown -= Time.fixedDeltaTime;

        Collider2D player = Physics2D.OverlapCircle(transform.position, visionDistance, playerLayer);
        if (player != null)
            ExecuteChase(player.transform.position);
        else
            ExecutePatrol();

        // Stuck = wall hit
        if (flipCooldown <= 0f)
        {
            float moved = Mathf.Abs(transform.position.x - lastPositionX);
            stuckTimer = moved < STUCK_DIST ? stuckTimer + Time.fixedDeltaTime : 0f;
            if (stuckTimer >= STUCK_TIME) Flip();
        }

        lastPositionX = transform.position.x;
    }

    private void ExecutePatrol()
    {
        rb.linearVelocity = new Vector2(patrolSpeed * moveDirection, rb.linearVelocity.y);
        if (flipCooldown <= 0f) CheckPlatformEdge();
    }

    private void ExecuteChase(Vector3 target)
    {
        float dir = Mathf.Sign(target.x - transform.position.x);
        if (flipCooldown <= 0f && Mathf.Sign(moveDirection) != dir) Flip();
        rb.linearVelocity = new Vector2(chaseSpeed * moveDirection, rb.linearVelocity.y);
    }

    // Find what platform we're standing on by checking what's directly below
    private void FindCurrentPlatform()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 5f);
        if (hit.collider != null)
            currentPlatform = hit.collider;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Update current platform whenever we land on something
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f) // hit from above = floor
            {
                currentPlatform = collision.collider;
                return;
            }
        }
    }

    private void CheckPlatformEdge()
    {
        if (currentPlatform == null)
        {
            FindCurrentPlatform();
            return;
        }

        // Get the bounds of the platform we're standing on
        Bounds b = currentPlatform.bounds;
        float enemyFrontX = transform.position.x + (col.size.x * 0.5f) * moveDirection;

        // If our front foot is past the platform edge, flip
        bool pastRightEdge = moveDirection == 1 && enemyFrontX >= b.max.x;
        bool pastLeftEdge = moveDirection == -1 && enemyFrontX <= b.min.x;

        if (pastRightEdge || pastLeftEdge)
            Flip();
    }

    private void Flip()
    {
        moveDirection *= -1;
        ApplyVisual();
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.position += new Vector2(moveDirection * 0.5f, 0f);
        flipCooldown = FLIP_COOLDOWN;
        stuckTimer = 0f;
    }

    private void ApplyVisual()
    {
        transform.localScale = new Vector3(-moveDirection, transform.localScale.y, transform.localScale.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, visionDistance);
    }
}
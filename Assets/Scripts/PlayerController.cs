using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    [SerializeField] private float groundDecel = 20f;
    [SerializeField] private float airDecel = 5f;

    [Header("Wall Movement")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(10f, 18f);
    [SerializeField] private LayerMask wallLayer;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 18f;
    [SerializeField] private float fallMultiplier = 3.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Dash")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.8f;

    [Header("Drop Down")]
    [SerializeField] private float dropDownTime = 0.6f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.08f;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;

    [HideInInspector] public Vector2 MoveInput;
    private bool jumpHeld;
    private bool dashPressed;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private float dashCooldownCounter;
    private bool isGrounded;
    private bool isDashing;
    private bool isWallSliding;
    private bool isTouchingWall;
    private bool canDashInAir = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        CheckGrounded();
        CheckWallTouch();
        HandleCoyoteTime();
        HandleJumpBuffer();
        HandleDashCooldown();
    }

    // At the top of PlayerController
    public bool canMove = true;

    private void FixedUpdate()
    {
        HandleWallSlide();
        // Only allow normal movement if NOT berserk/stunned
        if (!isDashing && !isWallSliding && canMove)
        {
            HandleMovement();
            HandleGravity();
        }
        HandleJump();
        HandleDash();
    }

    // --- Public Triggers (Called by ControlRandomizer) ---
    public void TriggerJump() { jumpBufferCounter = jumpBufferTime; jumpHeld = true; }
    public void TriggerStopJump() { jumpHeld = false; }
    public void TriggerDash() { dashPressed = true; }
    public void TriggerDropDown() { StartCoroutine(DropDownRoutine()); }

    private void HandleMovement()
    {
        float targetVelX = MoveInput.x * moveSpeed;
        float decel = isGrounded ? groundDecel : airDecel;
        float newVelX = Mathf.MoveTowards(rb.linearVelocity.x, targetVelX, decel * Time.fixedDeltaTime * 10f);
        rb.linearVelocity = new Vector2(newVelX, rb.linearVelocity.y);
    }

    private void HandleJump()
    {
        if (jumpBufferCounter > 0f)
        {
            if (coyoteCounter > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                FinishJump();
            }
            else if (isWallSliding)
            {
                float checkDist = col.size.x * 0.6f;
                bool wallOnRight = Physics2D.Raycast(transform.position, Vector2.right, checkDist, wallLayer);
                float jumpDir = wallOnRight ? -1f : 1f;
                rb.linearVelocity = new Vector2(wallJumpForce.x * jumpDir, wallJumpForce.y);
                FinishJump();
            }
        }
    }

    private void FinishJump() { jumpBufferCounter = 0f; coyoteCounter = 0f; canDashInAir = true; }

    private void HandleGravity()
    {
        if (rb.linearVelocity.y < 0f)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
    }

    private void HandleWallSlide()
    {
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
        else isWallSliding = false;
    }

    private void CheckWallTouch()
    {
        float checkDist = col.size.x * 0.5f + 0.1f;
        bool hitRight = Physics2D.Raycast(transform.position, Vector2.right, checkDist, wallLayer);
        bool hitLeft = Physics2D.Raycast(transform.position, Vector2.left, checkDist, wallLayer);
        isTouchingWall = hitRight || hitLeft;
    }

    private void CheckGrounded()
    {
        Vector2 origin = (Vector2)transform.position + Vector2.down * (col.size.y * 0.5f);
        RaycastHit2D hit = Physics2D.BoxCast(origin, new Vector2(col.size.x * 0.9f, 0.05f), 0f, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
        if (isGrounded) canDashInAir = true;
    }

    private IEnumerator DropDownRoutine()
    {
        Vector2 origin = (Vector2)transform.position + Vector2.down * (col.size.y * 0.5f);
        RaycastHit2D hit = Physics2D.BoxCast(origin, new Vector2(col.size.x * 0.9f, 0.1f), 0f, Vector2.down, 0.2f, LayerMask.GetMask("Platform"));
        if (hit.collider != null)
        {
            Collider2D pCol = hit.collider;
            Physics2D.IgnoreCollision(col, pCol, true);
            yield return new WaitForSeconds(dropDownTime);
            Physics2D.IgnoreCollision(col, pCol, false);
        }
    }

    private void HandleDash()
    {
        if (dashPressed && dashCooldownCounter <= 0f && canDashInAir)
        {
            StartCoroutine(DashRoutine());
        }
        dashPressed = false;
    }

    public void ApplyKnockback(Vector2 damageSourcePosition, float force)
    {
        float direction = transform.position.x < damageSourcePosition.x ? -1f : 1f;

        // Force the velocity to zero first so old momentum doesn't fight the knockback
        rb.linearVelocity = Vector2.zero;

        // Use a slightly higher force if you feel the "stuck" behavior persists
        rb.AddForce(new Vector2(direction * force, force), ForceMode2D.Impulse);

        // Start a tiny "Lockout" so player input doesn't immediately cancel the knockback
        StartCoroutine(KnockbackLockout());
    }

    private IEnumerator KnockbackLockout()
    {
        // Briefly disable move input so the player "reels" from the hit
        float originalSpeed = moveSpeed;
        moveSpeed = 0;
        yield return new WaitForSeconds(0.2f);
        moveSpeed = originalSpeed;
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        canDashInAir = false;
        dashCooldownCounter = dashCooldown;
        float dir = MoveInput.x != 0 ? Mathf.Sign(MoveInput.x) : (transform.localScale.x > 0 ? 1f : -1f);
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dir * dashForce, 0f);
        yield return new WaitForSeconds(dashDuration);
        rb.gravityScale = 1f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.4f, 0f);
        isDashing = false;
    }

    private void HandleCoyoteTime() { if (isGrounded) coyoteCounter = coyoteTime; else coyoteCounter -= Time.deltaTime; }
    private void HandleJumpBuffer() { if (jumpBufferCounter > 0f) jumpBufferCounter -= Time.deltaTime; }
    private void HandleDashCooldown() { if (dashCooldownCounter > 0f) dashCooldownCounter -= Time.deltaTime; }
}
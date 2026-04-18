using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
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

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.08f;

    [Header("Audio Clips")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip wallSlideSound;
    [SerializeField] private AudioClip runSound;
    [SerializeField] private AudioClip enrageSound; // New Enrage Sound
    [SerializeField] private float runVolume = 0.3f;
    [SerializeField] private float enrageVolume = 0.5f;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private Animator anim;
    private DamageSystem ds;

    [HideInInspector] public Vector2 MoveInput;
    [HideInInspector] public bool canMove = true;

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
        anim = GetComponent<Animator>();
        ds = GetComponent<DamageSystem>();
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (!canMove)
        {
            MoveInput = Vector2.zero;
            if (loopSource.isPlaying) loopSource.Stop();
            return;
        }

        CheckGrounded();
        CheckWallTouch();
        HandleCoyoteTime();
        HandleJumpBuffer();
        HandleDashCooldown();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (!canMove) return;

        HandleWallSlide();

        bool isGhosted = ds != null && ds.IsGhosted;
        bool isBerserk = ds != null && ds.IsInBerserkState;

        if (isBerserk)
        {
            jumpBufferCounter = 0;
            dashPressed = false;
        }

        if (!isDashing && !isWallSliding && canMove && !isBerserk)
        {
            HandleMovement();
            HandleGravity();
        }

        if (!isGhosted && !isBerserk)
        {
            HandleJump();
        }

        HandleDash();
    }

    private void UpdateAnimations()
    {
        if (anim == null) return;

        bool isBerserk = ds != null && ds.IsInBerserkState;

        anim.SetFloat("Speed", Mathf.Abs(MoveInput.x));
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWallSliding", isWallSliding);

        float yVel = rb.linearVelocity.y;
        if (Mathf.Abs(yVel) < 0.1f) yVel = 0;
        anim.SetFloat("yVelocity", yVel);

        if (Mathf.Abs(MoveInput.x) > 0.1f)
            transform.localScale = new Vector3(Mathf.Sign(MoveInput.x), 1, 1);
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            transform.localScale = new Vector3(Mathf.Sign(rb.linearVelocity.x), 1, 1);

        // --- AUDIO LOOP LOGIC (ENRAGE PRIORITY) ---
        if (isBerserk)
        {
            if (loopSource.clip != enrageSound || !loopSource.isPlaying)
            {
                loopSource.clip = enrageSound;
                loopSource.loop = true;
                loopSource.volume = enrageVolume;
                loopSource.Play();
            }
        }
        else if (isWallSliding)
        {
            if (loopSource.clip != wallSlideSound || !loopSource.isPlaying)
            {
                loopSource.clip = wallSlideSound;
                loopSource.loop = true;
                loopSource.volume = 0.4f;
                loopSource.Play();
            }
        }
        else if (Mathf.Abs(MoveInput.x) > 0.1f && isGrounded && !isDashing)
        {
            if (loopSource.clip != runSound || !loopSource.isPlaying)
            {
                loopSource.clip = runSound;
                loopSource.loop = true;
                loopSource.volume = runVolume;
                loopSource.Play();
            }
            loopSource.pitch = Mathf.Lerp(0.8f, 1.2f, Mathf.Abs(rb.linearVelocity.x) / moveSpeed);
        }
        else
        {
            // Stop looping sounds if none of the above conditions are met
            if (loopSource.isPlaying)
            {
                loopSource.Stop();
                loopSource.clip = null;
                loopSource.pitch = 1f;
            }
        }
    }

    private void PlayOneShotSFX(AudioClip clip, float vol = 0.7f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, vol);
    }

    public void TriggerJump() { if (canMove) { jumpBufferCounter = jumpBufferTime; jumpHeld = true; } }
    public void TriggerStopJump() { jumpHeld = false; }
    public void TriggerDash() { if (canMove) dashPressed = true; }
    public void TriggerDropDown() { if (canMove) StartCoroutine(DropDownRoutine()); }

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
            if (coyoteCounter > 0f || isWallSliding)
            {
                if (loopSource.clip == runSound) loopSource.Stop();

                if (isWallSliding)
                {
                    float checkDist = col.size.x * 0.6f;
                    bool wallOnRight = Physics2D.Raycast(transform.position, Vector2.right, checkDist, wallLayer);
                    float jumpDir = wallOnRight ? -1f : 1f;
                    rb.linearVelocity = new Vector2(wallJumpForce.x * jumpDir, wallJumpForce.y);
                }
                else
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                }
                PlayOneShotSFX(jumpSound, 1f);
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
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < -0.1f)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(0, -wallSlideSpeed);
        }
        else
        {
            isWallSliding = false;
        }
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
        Vector2 origin = (Vector2)transform.position + Vector2.down * (col.size.y * 0.45f);
        RaycastHit2D hit = Physics2D.BoxCast(origin, new Vector2(col.size.x * 0.8f, 0.05f), 0f, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
        if (isGrounded)
        {
            canDashInAir = true;
            coyoteCounter = coyoteTime;
        }
    }

    private IEnumerator DropDownRoutine()
    {
        Vector2 origin = (Vector2)transform.position + Vector2.down * (col.size.y * 0.5f);
        RaycastHit2D hit = Physics2D.BoxCast(origin, new Vector2(col.size.x * 0.9f, 0.1f), 0f, Vector2.down, 0.2f, LayerMask.GetMask("Platform"));
        if (hit.collider != null)
        {
            Collider2D pCol = hit.collider;
            Physics2D.IgnoreCollision(col, pCol, true);
            yield return new WaitForSeconds(0.6f);
            Physics2D.IgnoreCollision(col, pCol, false);
        }
    }

    private void HandleDash()
    {
        if (dashPressed && dashCooldownCounter <= 0f && canDashInAir) StartCoroutine(DashRoutine());
        dashPressed = false;
    }

    public void ApplyKnockback(Vector2 source, float force)
    {
        rb.linearVelocity = Vector2.zero;
        float dir = transform.position.x < source.x ? -1f : 1f;
        rb.AddForce(new Vector2(dir * force, force), ForceMode2D.Impulse);
        StartCoroutine(KnockbackLockout());
    }

    private IEnumerator KnockbackLockout()
    {
        float speed = moveSpeed; moveSpeed = 0;
        yield return new WaitForSeconds(0.2f);
        moveSpeed = speed;
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true; if (anim) anim.SetBool("isDashing", true);
        PlayOneShotSFX(dashSound, 0.8f);
        canDashInAir = false; dashCooldownCounter = dashCooldown;
        float dir = MoveInput.x != 0 ? Mathf.Sign(MoveInput.x) : (transform.localScale.x > 0 ? 1f : -1f);
        rb.gravityScale = 0f; rb.linearVelocity = new Vector2(dir * dashForce, 0f);
        yield return new WaitForSeconds(dashDuration);
        rb.gravityScale = 1f; rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.4f, 0f);
        isDashing = false; if (anim) anim.SetBool("isDashing", false);
    }

    private void HandleCoyoteTime() { if (isGrounded) coyoteCounter = coyoteTime; else coyoteCounter -= Time.deltaTime; }
    private void HandleJumpBuffer() { if (jumpBufferCounter > 0f) jumpBufferCounter -= Time.deltaTime; }
    private void HandleDashCooldown() { if (dashCooldownCounter > 0f) dashCooldownCounter -= Time.deltaTime; }
}
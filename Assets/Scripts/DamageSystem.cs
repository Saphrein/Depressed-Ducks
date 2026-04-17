using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageSystem : MonoBehaviour
{
    private ControlRandomizer randomizer;
    private PlayerController player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private AudioSource bgm;

    [Header("Settings")]
    public float scrambleDuration = 5f;
    public float knockbackForce = 12f;

    [Header("Rage System")]
    public RageController rageUI; // <--- LINK YOUR NEW UI HERE
    public Slider rageSlider;     // Optional: Keep for debugging
    public float ragePerHit = 20f; // Set to 20 so 5 hits = 100%
    public float stunDuration = 2.5f;
    public float forcedMoveSpeed = 7f;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip hurtSound;

    private float currentRage = 0f;
    private bool isScrambled = false;
    private bool isBerserk = false;
    private bool isBerserkCooldown = false;

    public bool IsInBerserkState => isBerserk;
    public bool IsGhosted => isScrambled || isBerserkCooldown;

    private void Awake()
    {
        randomizer = GetComponent<ControlRandomizer>();
        player = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        bgm = Camera.main.GetComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Hazard")) HandleDamage(collision.transform);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hazard")) HandleDamage(other.transform);
    }

    private void HandleDamage(Transform enemy)
    {
        if (isScrambled || isBerserk || isBerserkCooldown) return;

        if (sfxSource && hurtSound) sfxSource.PlayOneShot(hurtSound, 1.0f);

        currentRage += ragePerHit;

        // Update the new Pixel Art UI
        if (rageUI != null)
        {
            // Divide by 20 to get levels 0, 1, 2, 3, 4, 5
            rageUI.currentRageLevel = Mathf.FloorToInt(currentRage / 20f);
        }

        if (rageSlider) rageSlider.value = currentRage;

        if (currentRage >= 100f)
            StartCoroutine(BerserkRoutine(enemy));
        else
            StartCoroutine(ScrambleRoutine(enemy));
    }

    private IEnumerator BerserkRoutine(Transform target)
    {
        isBerserk = true;
        if (bgm) bgm.pitch = 1.3f;

        if (anim) anim.SetBool("isEnraged", true);

        rb.linearVelocity = Vector2.zero;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        SetHazardCollision(false);
        sr.color = Color.red;

        float timer = 0f;
        while (timer < stunDuration)
        {
            if (target != null)
            {
                Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
                rb.linearVelocity = dir * forcedMoveSpeed;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        EndBerserk(originalGravity);
    }

    private void EndBerserk(float originalGravity)
    {
        isBerserk = false;
        currentRage = 0f;

        // Reset the UI
        if (rageUI != null) rageUI.currentRageLevel = 0;
        if (rageSlider) rageSlider.value = 0;

        if (bgm) bgm.pitch = 1f;
        if (anim) anim.SetBool("isEnraged", false);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;
        StartCoroutine(BerserkCooldownRoutine());
    }

    private IEnumerator BerserkCooldownRoutine()
    {
        isBerserkCooldown = true;
        float safetyTimer = 3.0f;
        while (safetyTimer > 0)
        {
            safetyTimer -= Time.deltaTime;
            float alpha = (safetyTimer < 1f) ? (Mathf.Sin(Time.time * 20f) + 1.5f) / 2.5f : 0.5f;
            sr.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        isBerserkCooldown = false;
        SetHazardCollision(true);
        sr.color = Color.white;
    }

    private IEnumerator ScrambleRoutine(Transform enemy)
    {
        isScrambled = true;
        if (randomizer) randomizer.ShuffleControls();
        if (player) player.ApplyKnockback(enemy.position, knockbackForce);

        SetHazardCollision(false);
        sr.color = new Color(1, 0, 1, 0.5f);
        yield return new WaitForSeconds(scrambleDuration);

        if (randomizer) randomizer.ResetControls();
        yield return new WaitForSeconds(1.0f);
        isScrambled = false;
        if (!isBerserkCooldown) SetHazardCollision(true);
        sr.color = Color.white;
    }

    private void SetHazardCollision(bool canCollide)
    {
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("HazardLayer"), !canCollide);
    }
}
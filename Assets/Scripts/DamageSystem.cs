using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageSystem : MonoBehaviour
{
    private ControlRandomizer randomizer;
    private PlayerController player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    [Header("Settings")]
    public float scrambleDuration = 5f;
    public float knockbackForce = 12f;

    [Header("Rage System")]
    public Slider rageSlider;
    public float ragePerHit = 25f;
    public float stunDuration = 2.5f;
    public float forcedMoveSpeed = 7f;

    private float currentRage = 0f;
    private bool isScrambled = false;
    private bool isBerserk = false;
    private bool isBerserkCooldown = false;

    // Enemies check this to see if they should ignore the player
    public bool IsInBerserkState => isBerserk;
    public bool IsGhosted => isScrambled || isBerserkCooldown;

    private void Awake()
    {
        randomizer = GetComponent<ControlRandomizer>();
        player = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (rageSlider) rageSlider.value = 0;
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

        currentRage += ragePerHit;
        if (rageSlider) rageSlider.value = currentRage;

        if (currentRage >= 100f)
            StartCoroutine(BerserkRoutine(enemy));
        else
            StartCoroutine(ScrambleRoutine(enemy));
    }

    private IEnumerator BerserkRoutine(Transform target)
    {
        isBerserk = true;
        currentRage = 0f;
        if (rageSlider) rageSlider.value = 0;

        // 1. Disable Gravity and Collisions
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        SetHazardCollision(false);
        sr.color = Color.red;

        float timer = 0f;
        while (timer < stunDuration)
        {
            if (target != null)
            {
                // Move in 2D space (X and Y) toward the target
                Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
                rb.linearVelocity = direction * forcedMoveSpeed;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // 2. Start Cooldown Phase
        isBerserk = false;
        isBerserkCooldown = true;
        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;

        // 3. 3-Second Ghost Window
        float safetyTimer = 3.0f;
        while (safetyTimer > 0)
        {
            safetyTimer -= Time.deltaTime;
            // Flashing effect for the last second
            float alpha = (safetyTimer < 1f) ? (Mathf.Sin(Time.time * 20f) + 1.5f) / 2.5f : 0.5f;
            sr.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        // 4. Reset to Normal
        isBerserkCooldown = false;
        SetHazardCollision(true);
        sr.color = Color.white;
    }

    private IEnumerator ScrambleRoutine(Transform enemy)
    {
        isScrambled = true;
        randomizer.ShuffleControls();
        player.ApplyKnockback(enemy.position, knockbackForce);

        if (enemy.TryGetComponent<GroundEnemy>(out GroundEnemy g)) g.ForceFlip();
        if (enemy.TryGetComponent<FlyingEnemy>(out FlyingEnemy f)) f.ForceFlip();

        SetHazardCollision(false);
        sr.color = new Color(1, 0, 1, 0.5f);

        yield return new WaitForSeconds(scrambleDuration);

        randomizer.ResetControls();
        yield return new WaitForSeconds(1.0f); // Small safety buffer

        isScrambled = false;
        if (!isBerserkCooldown) SetHazardCollision(true);
        sr.color = Color.white;
    }

    private void SetHazardCollision(bool canCollide)
    {
        int pLayer = gameObject.layer;
        int hLayer = LayerMask.NameToLayer("HazardLayer");
        Physics2D.IgnoreLayerCollision(pLayer, hLayer, !canCollide);
    }
}
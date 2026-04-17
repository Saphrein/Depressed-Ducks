using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public float chaseSpeed = 4f;
    public float hoverAmplitude = 0.5f;
    public float hoverFrequency = 2f;

    [Header("Detection")]
    public float visionDistance = 8f; // Increased for better feel
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    private Vector2 anchorPos;
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
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, visionDistance, playerLayer);

        if (playerCollider != null)
        {
            var ds = playerCollider.GetComponent<DamageSystem>();

            // Check the specific properties in your DamageSystem
            if (ds != null && !ds.IsGhosted && !ds.IsInBerserkState)
            {
                Vector2 directionToPlayer = (playerCollider.transform.position - transform.position).normalized;
                float distanceToPlayer = Vector2.Distance(transform.position, playerCollider.transform.position);

                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);

                if (hit.collider == null)
                {
                    isChasing = true;
                    playerTransform = playerCollider.transform;
                    return;
                }
            }
        }
        isChasing = false;
    }

    void ExecutePatrol()
    {
        anchorPos.x += direction * speed * Time.deltaTime;
        float hoverOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        transform.position = new Vector3(anchorPos.x, anchorPos.y + hoverOffset, transform.position.z);
    }

    void ExecuteChase()
    {
        transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, chaseSpeed * Time.deltaTime);
        anchorPos = transform.position;

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

    private void OnTriggerEnter2D(Collider2D collision) { HandleCollision(collision); }
    private void OnTriggerStay2D(Collider2D collision) { HandleCollision(collision); }

    private void HandleCollision(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            Flip();
            anchorPos.x += direction * 0.2f;
            transform.position = new Vector3(anchorPos.x, transform.position.y, transform.position.z);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionDistance);
    }
}
using UnityEngine;

public class WallCrawler : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3f;
    public float patrolDistance = 4f; // How far up/down it goes from start

    private float minY;
    private float maxY;
    private int direction = 1;
    private float startX;

    void Start()
    {
        // Lock the X position so it stays on the side of the wall
        startX = transform.position.x;

        // Set the boundaries based on where you place it in the scene
        minY = transform.position.y - patrolDistance;
        maxY = transform.position.y + patrolDistance;
    }

    void Update()
    {
        // Move only on the Y axis
        transform.Translate(Vector2.up * direction * speed * Time.deltaTime);

        // Check if it hit the top or bottom limits
        if (transform.position.y >= maxY)
        {
            direction = -1;
            transform.position = new Vector3(startX, maxY, transform.position.z);
        }
        else if (transform.position.y <= minY)
        {
            direction = 1;
            transform.position = new Vector3(startX, minY, transform.position.z);
        }
    }

    // Keep X perfectly locked even if physics bumps it
    void LateUpdate()
    {
        transform.position = new Vector3(startX, transform.position.y, transform.position.z);
    }
}
using UnityEngine;

public class WallCrawler : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3f;
    [Tooltip("How many units UP the enemy will travel from its start point")]
    public float distanceUp = 2f;
    [Tooltip("How many units DOWN the enemy will travel from its start point")]
    public float distanceDown = 2f;

    private float topBoundary;
    private float bottomBoundary;
    private int direction = 1;
    private float startX;

    void Start()
    {
        startX = transform.position.x;

        // Calculate boundaries based on the initial placement in the editor
        topBoundary = transform.position.y + distanceUp;
        bottomBoundary = transform.position.y - distanceDown;
    }

    void Update()
    {
        // 1. Calculate movement
        float nextY = transform.position.y + (direction * speed * Time.deltaTime);

        // 2. Check Boundaries
        if (direction == 1 && nextY >= topBoundary)
        {
            direction = -1;
            nextY = topBoundary;
            FlipVisual();
        }
        else if (direction == -1 && nextY <= bottomBoundary)
        {
            direction = 1;
            nextY = bottomBoundary;
            FlipVisual();
        }

        // 3. Apply position
        transform.position = new Vector3(startX, nextY, transform.position.z);
    }

    private void FlipVisual()
    {
        transform.localScale = new Vector3(transform.localScale.x, direction, transform.localScale.z);
    }

    // This lets you see the patrol range for EVERY enemy in the Scene View
    private void OnDrawGizmosSelected()
    {
        // Only draws when you click on the enemy in the editor
        float gizmoTop = Application.isPlaying ? topBoundary : transform.position.y + distanceUp;
        float gizmoBottom = Application.isPlaying ? bottomBoundary : transform.position.y - distanceDown;

        Gizmos.color = Color.green;
        Vector3 startLine = new Vector3(transform.position.x, gizmoTop, transform.position.z);
        Vector3 endLine = new Vector3(transform.position.x, gizmoBottom, transform.position.z);

        Gizmos.DrawLine(startLine, endLine);
        Gizmos.DrawCube(startLine, new Vector3(0.5f, 0.05f, 0.1f));
        Gizmos.DrawCube(endLine, new Vector3(0.5f, 0.05f, 0.1f));
    }
}
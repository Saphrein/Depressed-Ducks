using UnityEngine;

public class VerticalLooper : MonoBehaviour
{
    public Transform cam;
    [Header("Settings")]
    public int numberOfSets = 3;
    public float zGap = 0.1f; // Forces the layers apart to stop flashing

    private Transform[] sets;
    private float backgroundHeight;

    void Start()
    {
        if (cam == null) cam = Camera.main.transform;

        int childCount = transform.childCount;
        sets = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            sets[i] = transform.GetChild(i);

            // This line FORCES the Z-position so you don't have to do it manually
            // It puts Set1 at 0, Set2 at 0.1, Set3 at 0.2
            sets[i].localPosition = new Vector3(sets[i].localPosition.x, sets[i].localPosition.y, i * zGap);
        }

        // Calculate height from the first child found
        SpriteRenderer firstSprite = GetComponentInChildren<SpriteRenderer>();
        if (firstSprite != null)
        {
            backgroundHeight = firstSprite.bounds.size.y;
        }
    }

    void LateUpdate()
    {
        if (cam == null || sets == null) return;

        // Follow camera X
        transform.position = new Vector3(cam.position.x, transform.position.y, transform.position.z);

        foreach (Transform t in sets)
        {
            // Use the center of the set relative to camera
            float cameraDistY = cam.position.y - t.position.y;

            // If the set is entirely below the camera view, move it to the top
            if (cameraDistY > backgroundHeight)
            {
                t.position += new Vector3(0, backgroundHeight * numberOfSets, 0);
            }
            // If the set is entirely above (falling down)
            else if (cameraDistY < -backgroundHeight * (numberOfSets - 1))
            {
                t.position -= new Vector3(0, backgroundHeight * numberOfSets, 0);
            }
        }
    }
}
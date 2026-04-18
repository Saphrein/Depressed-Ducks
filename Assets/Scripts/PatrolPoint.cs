using UnityEngine;

public class PatrolPoint : MonoBehaviour
{
    // This just helps you see the points in the Scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
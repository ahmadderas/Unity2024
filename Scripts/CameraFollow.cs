using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;      // Reference to the player's Transform
    public Vector3 offset;        // Offset distance between player and camera

    void LateUpdate()
    {
        if (player != null)
        {
            // Update camera position
            transform.position = player.position + offset;
        }
    }
}

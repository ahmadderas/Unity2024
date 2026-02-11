using UnityEngine;

public class ArrowCollision : MonoBehaviour
{
    public int arrowDamage = 10;  // Amount of damage the arrow deals
    private GameObject targetEnemy; // The targeted enemy

    // Set the target enemy when the arrow is instantiated
    public void SetTargetEnemy(GameObject enemy)
    {
        targetEnemy = enemy;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only apply damage if the arrow hits the targeted enemy
        if (other.gameObject == targetEnemy)
        {
            // Apply damage to the enemy
            MinionController minionController = other.GetComponent<MinionController>();
            if (minionController != null)
            {
                minionController.TakeDamage(arrowDamage);
            }

            // Destroy the arrow after it hits the target
            Destroy(gameObject);
        }
    }
}

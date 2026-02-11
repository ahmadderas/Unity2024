using UnityEngine;

public class FireballCollision : MonoBehaviour
{
    public int fireballDamage = 5;  // Amount of damage the fireball deals
    private GameObject targetEnemy;  // Reference to the targeted enemy

    private void Start()
    {
        // Ensure the fireball is only able to collide with the target enemy
        Collider fireballCollider = GetComponent<Collider>();
        if (fireballCollider != null)
        {
            fireballCollider.isTrigger = true; // Make it a trigger to avoid physical collisions
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only apply damage if the fireball hits the targeted enemy
        if (other.gameObject == targetEnemy && other.gameObject.CompareTag("Enemy"))
        {
            // Handle what happens when the fireball hits the enemy
            Debug.Log("Fireball hit the target enemy!");

            // Call the TakeDamage method on the enemy and pass the damage value
            other.gameObject.GetComponent<MinionController>().TakeDamage(fireballDamage);

            // Destroy the fireball after impact
            Destroy(gameObject);
        }
    }

    // Assign the target enemy when the fireball is spawned
    public void SetTargetEnemy(GameObject enemy)
    {
        targetEnemy = enemy;
    }
}

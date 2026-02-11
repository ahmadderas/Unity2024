using UnityEngine;

public class InfernoTrigger : MonoBehaviour
{
    public int initialDamage = 10;      // Damage dealt immediately upon creation
    public int damagePerSecond = 2;    // Damage dealt per second
    public float duration = 5f;        // Duration the Inferno lasts
    private CapsuleCollider capsuleCollider;

    private float damageInterval = 1f; // Interval in seconds for applying continuous damage
    private float nextDamageTime = 0f; // Tracks the next time continuous damage can be applied
    private bool hasAppliedInitialDamage = false; // Tracks if initial damage has been applied
    private bool hasStartedContinuousDamage = false; // Tracks if continuous damage has started

    public float initialDamageDelay = 1f; // Delay before continuous damage starts

    private void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();

        // Ensure the collider is set as a trigger
        if (!capsuleCollider.isTrigger)
        {
            Debug.LogWarning("InfernoTrigger: CapsuleCollider should be set as a trigger.");
            capsuleCollider.isTrigger = true;
        }

        // Destroy the Inferno after its duration
        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Skip if the other object is the Sorcerer
        if (other.CompareTag("Wanderer"))
        {
            return; // Ignore the Sorcerer
        }

        // Check if the object is an enemy
        if (other.CompareTag("Enemy"))
        {
            MinionController enemy = other.GetComponent<MinionController>();
            if (enemy != null)
            {
                // Apply initial damage only the first time the enemy enters the Inferno
                if (!hasAppliedInitialDamage)
                {
                    enemy.TakeDamage(initialDamage);
                    hasAppliedInitialDamage = true; // Ensure initial damage is applied only once

                    // Start the delay for continuous damage after the initial damage
                    Invoke(nameof(StartContinuousDamage), initialDamageDelay);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Skip if the other object is the Sorcerer
        if (other.CompareTag("Wanderer"))
        {
            return; // Ignore the Sorcerer
        }

        // Check if the object is an enemy
        if (other.CompareTag("Enemy"))
        {
            MinionController enemy = other.GetComponent<MinionController>();
            if (enemy != null && hasStartedContinuousDamage)
            {
                // Apply continuous damage after the initial delay
                if (Time.time >= nextDamageTime)
                {
                    enemy.TakeDamage(damagePerSecond);
                    nextDamageTime = Time.time + damageInterval; // Set the next damage time
                }
            }
        }
    }

    // Starts the continuous damage after the initial damage delay
    private void StartContinuousDamage()
    {
        hasStartedContinuousDamage = true;
    }
}

using UnityEngine;
using UnityEngine.AI;

public class DemonAi : MonoBehaviour
{
    public NavMeshAgent agent;
    private Vector3 campCenter; // Patrol center
    public float patrolRadius = 5f; // Radius within which demons patrol
    public float collisionAvoidanceRadius = 2f; // Radius to avoid other enemies

    public float health = 50f;

    private Vector3 walkPoint;
    private bool walkPointSet;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Set the camp center dynamically to the parent's position
        if (transform.parent != null)
        {
            campCenter = transform.parent.position;
        }
        else
        {
            Debug.LogError("DemonAi: No parent found. Patrol center defaulting to spawn location.");
            campCenter = transform.position;
        }
    }

    private void Update()
    {
        Patrol();
    }

    private void Patrol()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);

            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            // Reached walk point
            if (distanceToWalkPoint.magnitude < 1f)
                walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        for (int attempt = 0; attempt < 10; attempt++) // Try 10 times to find a valid point
        {
            float randomZ = Random.Range(-patrolRadius, patrolRadius);
            float randomX = Random.Range(-patrolRadius, patrolRadius);

            Vector3 potentialPoint = new Vector3(
                campCenter.x + randomX,
                campCenter.y,
                campCenter.z + randomZ
            );

            // Validate the point is on the NavMesh and not overlapping other units
            if (NavMesh.SamplePosition(potentialPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas) && IsPositionClear(hit.position))
            {
                walkPoint = hit.position;
                walkPointSet = true;
                return; // Exit the loop once a valid point is found
            }
        }
        walkPointSet = false; // No valid point found
    }

    private bool IsPositionClear(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, collisionAvoidanceRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Demon") || collider.CompareTag("Minion"))
            {
                return false; // Position is not clear
            }
        }
        return true; // Position is clear
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Patrol radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(campCenter, patrolRadius);

        // Collision avoidance radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collisionAvoidanceRadius);
    }
}

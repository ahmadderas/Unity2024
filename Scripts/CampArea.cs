using UnityEngine;
using System.Collections.Generic;

public class CampSpawner : MonoBehaviour
{
    public GameObject minionPrefab;  // The prefab for minions
    public GameObject demonPrefab;   // The prefab for demons

    private const float initialMinSpawnDistance = 2f; // Starting distance between enemies
    private const float distanceIncreaseStep = 0.5f;  // Distance increment if spawn attempts fail
    private const int maxSpawnAttempts = 100;         // Maximum attempts to find a spawn position

    private bool isAlerted = false; // Track if minions should be alerted

    // Spawns the specified number of minions and demons
    public (int, int) SpawnEnemies(int minions, int demons)
    {
        if (!ValidatePrefabs())
            return (0, 0);

        Collider campCollider = GetComponent<Collider>();
        if (campCollider == null)
        {
            Debug.LogError("[CampSpawner] A Collider is required to define the spawn area.");
            return (0, 0);
        }

        List<Vector3> occupiedPositions = new List<Vector3>();
        float minSpawnDistance = initialMinSpawnDistance;

        Vector3 boundsMin = campCollider.bounds.min;
        Vector3 boundsMax = campCollider.bounds.max;

        int spawnedMinions = SpawnEntity(minionPrefab, minions, occupiedPositions, minSpawnDistance, boundsMin, boundsMax);
        int spawnedDemons = SpawnEntity(demonPrefab, demons, occupiedPositions, minSpawnDistance, boundsMin, boundsMax);

        return (spawnedMinions, spawnedDemons);
    }

    // Spawns a specified number of entities of a given type
    private int SpawnEntity(GameObject prefab, int count, List<Vector3> occupiedPositions, float minSpawnDistance, Vector3 boundsMin, Vector3 boundsMax)
    {
        int spawnedCount = 0;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition(occupiedPositions, ref minSpawnDistance, boundsMin, boundsMax);
            if (spawnPosition == Vector3.zero)
            {
                Debug.LogWarning($"[CampSpawner] Failed to find a valid position for {prefab.name}.");
                continue;
            }

            GameObject spawnedEntity = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
            occupiedPositions.Add(spawnPosition);
            spawnedCount++;

            // If minions are spawned, alert them
            if (prefab == minionPrefab)
            {
                MinionAI minionAI = spawnedEntity.GetComponent<MinionAI>();
                if (minionAI != null && isAlerted)
                {
                    minionAI.Alert();  // Alert minion to start moving toward the Wanderer
                }
            }
        }

        return spawnedCount;
    }

    // Finds a valid spawn position within the camp bounds
    private Vector3 GetValidSpawnPosition(List<Vector3> occupiedPositions, ref float minSpawnDistance, Vector3 boundsMin, Vector3 boundsMax)
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(boundsMin.x, boundsMax.x),
                transform.position.y,  // Use the current y position of the CampSpawner
                Random.Range(boundsMin.z, boundsMax.z)
            );

            if (IsPositionValid(randomPosition, occupiedPositions, minSpawnDistance))
                return randomPosition;  // Valid position found
        }

        // Increase spawn distance and log a warning after repeated failures
        minSpawnDistance += distanceIncreaseStep;
        Debug.LogWarning("[CampSpawner] Increasing min spawn distance to avoid overlap.");
        return Vector3.zero;  // Return zero vector if no valid position is found
    }

    // Validates whether a position is suitable for spawning
    private bool IsPositionValid(Vector3 position, List<Vector3> occupiedPositions, float minSpawnDistance)
    {
        foreach (Vector3 pos in occupiedPositions)
        {
            if (Vector3.Distance(position, pos) < minSpawnDistance)
                return false;  // Too close to another object
        }
        return true;
    }

    // Validates that required prefabs are assigned
    private bool ValidatePrefabs()
    {
        if (minionPrefab == null)
        {
            Debug.LogError("[CampSpawner] Minion prefab is not assigned.");
            return false;
        }

        if (demonPrefab == null)
        {
            Debug.LogError("[CampSpawner] Demon prefab is not assigned.");
            return false;
        }

        return true;
    }

    // Called when a trigger event happens (e.g., when Wanderer enters the camp)
    private void OnTriggerEnter(Collider other)
    {
        // Check if the Wanderer has entered the camp's trigger area
        if (other.CompareTag("Wanderer"))
        {
            isAlerted = true; // Alert minions when Wanderer enters the camp
            AlertMinions();  // Call to alert all minions
        }
    }

    // Alert all minions in the camp to start moving toward the Wanderer
    private void AlertMinions()
    {
        // Get all Minions within the camp (assuming they are children of the camp)
        MinionAI[] minions = GetComponentsInChildren<MinionAI>();

        foreach (MinionAI minion in minions)
        {
            minion.Alert(); // Alert each minion to start moving toward the Wanderer
        }
    }
}

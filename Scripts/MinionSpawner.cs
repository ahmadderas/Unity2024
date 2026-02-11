using UnityEngine;

public class MinionSpawner : MonoBehaviour
{
    public GameObject minionPrefab;  // Reference to the Minion prefab
    public int minionCount = 10;     // Number of Minions to spawn
    public float spawnRadius = 5f;   // Radius within which Minions will be spawned

    void Start()
    {
        SpawnMinions();
    }

    void SpawnMinions()
    {
        for (int i = 0; i < minionCount; i++)
        {
            // Randomize spawn position within a sphere of a given radius
            Vector3 randomPosition = transform.position + Random.insideUnitSphere * spawnRadius;

            // Ensure the Minion is on the ground by setting y to 0
            randomPosition.y = 0;

            // Instantiate the Minion prefab
            Instantiate(minionPrefab, randomPosition, Quaternion.identity);
        }
    }
}

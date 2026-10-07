using System.Collections.Generic;
using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("The enemy prefab you want to spawn.")]
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("List of transforms representing the 5 spawn points in your arena.")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("Timing & Limits")]
    [Tooltip("Time in seconds between each spawn attempt.")]
    [SerializeField] private float spawnInterval = 3.0f;

    [Tooltip("Maximum number of active enemies allowed in the arena at once.")]
    [SerializeField] private int maxEnemies = 10;

    private float spawnTimer;
    private readonly List<GameObject> activeEnemies = new List<GameObject>();

    private void Update()
    {
        // Clean up any destroyed enemies from our tracking list first
        activeEnemies.RemoveAll(enemy => enemy == null);

        // Check if we have reached the maximum enemy limit
        if (activeEnemies.Count >= maxEnemies)
        {
            return;
        }

        // Handle the spawn timer countdown
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f; // Reset timer
        }
    }

    private void SpawnEnemy()
    {
        // Validate that we have a prefab and spawn points assigned
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy Prefab is not assigned on the Spawner!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points assigned to the Spawner!");
            return;
        }

        // Pick a random spawn point from our list
        int randomIndex = Random.Range(0, spawnPoints.Count);
        Transform selectedPoint = spawnPoints[randomIndex];

        // Instantiate the enemy at the chosen spawn point's position and rotation
        GameObject newEnemy = Instantiate(enemyPrefab, selectedPoint.position, selectedPoint.rotation);

        // Track the spawned enemy
        activeEnemies.Add(newEnemy);
    }
}
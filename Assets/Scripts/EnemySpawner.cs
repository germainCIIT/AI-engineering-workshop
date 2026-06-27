using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Endlessly spawns enemies at random points across a rectangular field, up to a
/// maximum living at once. Place this on an empty GameObject; the spawn area is
/// centered on its position. Draws the area as a gizmo for easy tuning.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Tooltip("Enemy prefabs to spawn (e.g. Triangle and Square). One is picked at random each spawn.")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Tooltip("Size of the rectangular spawn field (width x height), centered on this object.")]
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(16f, 9f);

    [Tooltip("Seconds between spawn attempts.")]
    [SerializeField] private float spawnInterval = 1.5f;

    [Tooltip("Maximum number of enemies alive at once.")]
    [SerializeField] private int maxEnemies = 20;

    [Tooltip("Don't spawn closer than this to the player. Set 0 to disable.")]
    [SerializeField] private float minDistanceFromPlayer = 3f;

    [Tooltip("Tag used to find the player (for the spawn-distance check).")]
    [SerializeField] private string playerTag = "Player";

    private float spawnTimer = 0f;
    private Transform player;

    // Tracks living spawns so we can respect the cap (destroyed enemies become null).
    private readonly List<GameObject> alive = new List<GameObject>();

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
            player = playerObject.transform;
    }

    private void Update()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return;

        PruneDead();

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            spawnTimer = spawnInterval;

            if (alive.Count < maxEnemies)
                SpawnOne();
        }
    }

    private void SpawnOne()
    {
        Vector2 position;
        const int maxAttempts = 10;
        int attempt = 0;

        // Try a few times to find a point far enough from the player.
        do
        {
            position = GetRandomPointInArea();
            attempt++;
        }
        while (player != null
               && minDistanceFromPlayer > 0f
               && Vector2.Distance(position, player.position) < minDistanceFromPlayer
               && attempt < maxAttempts);

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        if (prefab == null)
            return;

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
        alive.Add(enemy);
    }

    private Vector2 GetRandomPointInArea()
    {
        float x = Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f);
        float y = Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f);
        return (Vector2)transform.position + new Vector2(x, y);
    }

    private void PruneDead()
    {
        for (int i = alive.Count - 1; i >= 0; i--)
        {
            if (alive[i] == null)
                alive.RemoveAt(i);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0f));
    }
}

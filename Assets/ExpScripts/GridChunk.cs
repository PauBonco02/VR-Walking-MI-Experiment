using UnityEngine;
using System.Collections.Generic;

public class ChunkGenerator : MonoBehaviour
{
    [Header("Player & Chunk Setup")]
    public Transform player;          // Player
    public GameObject[] chunkPrefabs; // Array of chunk prefabs
    public float radius = 20f;        // Radius around player in which chunks stay alive
    public int chunkSize = 5;         // Width and length of each chunk in world units

    // Dictionary to store which chunk GameObject corresponds to each grid coordinate
    private Dictionary<Vector2Int, GameObject> spawnedChunks = new Dictionary<Vector2Int, GameObject>();

    void Update()
    {
        // Find player's chunk coordinate
        Vector3 playerPos = player.position;
        int playerChunkX = Mathf.FloorToInt(playerPos.x / chunkSize);
        int playerChunkZ = Mathf.FloorToInt(playerPos.z / chunkSize);

        // Convert the float radius to a "chunk radius"
        int chunkRadius = Mathf.CeilToInt(radius / chunkSize);

        // Spawn new chunks in the region around player
        for (int x = -chunkRadius; x <= chunkRadius; x++)
        {
            for (int z = -chunkRadius; z <= chunkRadius; z++)
            {
                Vector2Int chunkCoord = new Vector2Int(playerChunkX + x, playerChunkZ + z);

                // If we haven't spawned a chunk at this coordinate yet, spawn it
                if (!spawnedChunks.ContainsKey(chunkCoord))
                {
                    SpawnChunkAt(chunkCoord);
                }
            }
        }

        // Collect and remove chunks that are too far from the player
        List<Vector2Int> coordsToRemove = new List<Vector2Int>();

        foreach (var kvp in spawnedChunks)
        {
            Vector2Int chunkCoord = kvp.Key;
            GameObject chunkObj = kvp.Value;

            // Distance in chunk-coordinates
            int distX = Mathf.Abs(chunkCoord.x - playerChunkX);
            int distZ = Mathf.Abs(chunkCoord.y - playerChunkZ);

            // If this chunk is outside the chunkRadius, schedule it for removal
            if (distX > chunkRadius || distZ > chunkRadius)
            {
                coordsToRemove.Add(chunkCoord);
            }
        }

        // Remove them
        foreach (Vector2Int coord in coordsToRemove)
        {
            Destroy(spawnedChunks[coord]);
            spawnedChunks.Remove(coord);
        }
    }

    void SpawnChunkAt(Vector2Int coord)
    {
        // Pick a random prefab
        int randIndex = Random.Range(0, chunkPrefabs.Length);
        GameObject chunkPrefab = chunkPrefabs[randIndex];

        // Calculate the world position of chunk center
        Vector3 worldPos = new Vector3(coord.x * chunkSize + chunkSize * 0.5f, 0f, coord.y * chunkSize + chunkSize * 0.5f);

        // Random rotation (for extra variability in the world) = 0, 90, 180, or 270 degrees about Y axis
        int rotations = Random.Range(0, 4);
        float rotationY = 90f * rotations;
        Quaternion randomRot = Quaternion.Euler(0f, rotationY, 0f);

        // Instantiate
        GameObject chunkObj = Instantiate(chunkPrefab, worldPos, randomRot);
        chunkObj.name = $"Chunk ({coord.x}, {coord.y})";

        // Store it in the dictionary
        spawnedChunks.Add(coord, chunkObj);
    }
}

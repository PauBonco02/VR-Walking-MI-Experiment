using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpManager : MonoBehaviour
{
    [Header("Prefabs & Player")]
    public GameObject chunkLeftPrefab;
    public GameObject chunkRightPrefab;
    public Transform player;

    [Header("Chunk Settings")]
    public int totalChunks = 10;
    public float chunkLength = 30f;

    [Header("Movement & Turning")]
    [HideInInspector] public float moveDistance;  // We won't show this in Inspector
    public float standDuration = 2f; // Time standing
    public float moveDuration = 2f;   // Time walking
    public float turnDuration = 1f;   // Time to turn
    public float pauseTime = 1f;   // Pause between steps

    private List<GameObject> activeChunks = new List<GameObject>();
    private List<char> turnSequence;
    private int turnIndex = 0;

    // We'll track where the last chunk ended, though the main logic now
    // is in the coroutine that moves the player to each chunk's ExitPoint.
    private Vector3 lastChunkEnd = Vector3.zero;
    private Quaternion lastChunkRotation = Quaternion.identity;

    void Start()
    {
        moveDistance = chunkLength * 0.5f;

        // Generate an L/R sequence up front
        turnSequence = GenerateTurnSequence(totalChunks);

        // 1) Spawn the chunks
        GenerateNextChunk();
        GenerateNextChunk();
        GenerateNextChunk();
        GenerateNextChunk();

        // Begin a step-by-step spawn + move + turn sequence
        StartCoroutine(SpawnAndMoveRoutine());
    }

    // ------------------------------------------------------------------------
    // The main coroutine that orchestrates chunk spawning, movement, turns, etc.
    // ------------------------------------------------------------------------
    private IEnumerator SpawnAndMoveRoutine()
    {
        // For however many chunks you want:
        for (int i = 0; i < totalChunks; i++)
        {
            // 1) Spawn next chunk
            GenerateNextChunk();  // chooses Left/Right prefab, updates lastChunkEnd/Rotation

            // Our "activeChunks" list’s newest entry is the chunk we just spawned.
            GameObject currentChunk = activeChunks[activeChunks.Count - 1];

            // 2) Move player to this chunk’s ExitPoint
            Transform exitPoint = currentChunk.transform.Find("ExitPoint");
            if (exitPoint == null)
            {
                Debug.LogError("Chunk prefab missing ExitPoint child!");
                yield break; // Abort if something is wrong
            }

            // 1) Move forward 15m
            yield return StartCoroutine(MoveForward(moveDistance, moveDuration));
            yield return new WaitForSeconds(pauseTime);

            // 2) Turn 90 degrees
            if (turnSequence[i]=='L')
            {
                yield return StartCoroutine(TurnLeft(90f, turnDuration));
            }
            else
            {
                yield return StartCoroutine(TurnLeft(-90f, turnDuration));
            }
            yield return new WaitForSeconds(pauseTime);

            // 3) Move forward another 15m
            yield return StartCoroutine(MoveForward(moveDistance, moveDuration));
            yield return new WaitForSeconds(pauseTime);

            // 3) Stand still watching the event
            yield return new WaitForSeconds(standDuration);
            yield return new WaitForSeconds(pauseTime);
        }

        // After all chunks are done, you could do any “end of sequence” logic here
        Debug.Log("All chunks spawned and player has moved/turned through them.");
    }

    // ------------------------------------------------------------------------
    // Spawns one chunk, left or right, updates lastChunkEnd & lastChunkRotation
    // ------------------------------------------------------------------------
    void GenerateNextChunk()
    {
        // Safeguard in case we surpass the generated turns
        if (turnIndex >= turnSequence.Count)
        {
            Debug.LogWarning("No more turns in sequence.");
            return;
        }

        char direction = turnSequence[turnIndex];
        turnIndex++;

        // Pick left vs right prefab
        GameObject prefab = (direction == 'L') ? chunkLeftPrefab : chunkRightPrefab;

        // Instantiate it at last chunk's end position/rotation
        GameObject newChunk = Instantiate(prefab, lastChunkEnd, lastChunkRotation);
        activeChunks.Add(newChunk);

        // Update the "lastChunkEnd" and "lastChunkRotation" by reading
        // the newly spawned chunk’s ExitPoint
        Transform exitPoint = newChunk.transform.Find("ExitPoint");
        if (exitPoint != null)
        {
            lastChunkEnd = exitPoint.position;
            lastChunkRotation = exitPoint.rotation;
        }

        // 4) Destroy the oldest chunk if we have too many
        //    For example, keep only the 6 newest chunks:
        if (activeChunks.Count > 6)
        {
            // Destroy the oldest chunk (at index 0)
            Destroy(activeChunks[0]);
            // Remove it from the list
            activeChunks.RemoveAt(0);
        }
    }

    // ------------------------------------------------------------------------
    // Generates a list of L/R ensuring half are L, half are R,
    // and no more than 2 of the same in a row (based on your original code).
    // ------------------------------------------------------------------------
    List<char> GenerateTurnSequence(int count)
    {
        int half = count / 2;
        int leftCount = 0;
        int rightCount = 0;

        List<char> sequence = new List<char>();

        for (int i = 0; i < count; i++)
        {
            List<char> options = new List<char>();

            // Limit number of 'L', and avoid triple 'L'
            if (leftCount < half && !(i >= 2 && sequence[i - 1] == 'L' && sequence[i - 2] == 'L'))
                options.Add('L');

            // Limit number of 'R', and avoid triple 'R'
            if (rightCount < half && !(i >= 2 && sequence[i - 1] == 'R' && sequence[i - 2] == 'R'))
                options.Add('R');

            // If no options left, retry
            if (options.Count == 0)
                return GenerateTurnSequence(count); // naive approach to re-generate

            // Randomly choose L or R from available options
            char chosen = options[Random.Range(0, options.Count)];
            sequence.Add(chosen);

            if (chosen == 'L') leftCount++;
            else rightCount++;
        }

        return sequence;
    }

    private IEnumerator MoveForward(float distance, float duration)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + transform.forward * distance;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Force final position
        transform.position = endPos;
    }

    private IEnumerator TurnLeft(float angle, float duration)
    {
        Quaternion startRot = transform.rotation;
        // Positive angle = turn around Y axis, but if you want “left”
        // you might do a negative angle or positive, depending on orientation. 
        // Commonly, turning left by +90 requires rotating -90 around the Y axis in many coordinate conventions.
        // Adjust the sign if you turn the wrong way.
        Quaternion endRot = startRot * Quaternion.Euler(0, -angle, 0);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Force final rotation
        transform.rotation = endRot;
    }

}

using System.Collections.Generic;
using UnityEngine;

public class ChunkTurnGenerator : MonoBehaviour
{
    public int chunkCount = 10;

    void Start()
    {
        List<char> turnSequence = GenerateTurnSequence(chunkCount);
        Debug.Log("Generated sequence: " + new string(turnSequence.ToArray()));
    }

    List<char> GenerateTurnSequence(int totalChunks)
    {
        int half = totalChunks / 2;
        int leftCount = 0;
        int rightCount = 0;

        List<char> sequence = new List<char>();

        for (int i = 0; i < totalChunks; i++)
        {
            List<char> options = new List<char>();

            // Check if we can still add left
            if (leftCount < half && !(i >= 2 && sequence[i - 1] == 'L' && sequence[i - 2] == 'L'))
            {
                options.Add('L');
            }

            // Check if we can still add right
            if (rightCount < half && !(i >= 2 && sequence[i - 1] == 'R' && sequence[i - 2] == 'R'))
            {
                options.Add('R');
            }

            // If no options are available, the logic failed
            if (options.Count == 0)
            {
                Debug.LogWarning("Couldn't generate a valid sequence with current rules. Retrying...");
                return GenerateTurnSequence(totalChunks); // retry
            }

            // Pick one randomly
            char chosen = options[Random.Range(0, options.Count)];
            sequence.Add(chosen);

            // Update counters
            if (chosen == 'L') leftCount++;
            else rightCount++;
        }

        return sequence;
    }
}

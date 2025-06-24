using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.PoseDetection;
using UnityEngine.Audio;


public class GridMovement : MonoBehaviour
{
    [Header("Yaw Pivot")]
    public Transform yawPivot;

    [Header("Movement Settings")]
    [HideInInspector] public float moveDistance = 5f; 
    public float moveDuration = 3f;   
    public float turnDuration = 3f;
    public float waitDuration = 3f;
    public float instrDuration = 1f;
    public float MIDuration = 0.5f;

    [HideInInspector] public float stopDuration = 1f;

    [Header("Samples")]
    public int nSamples = 10; // samples for each command 

    [Header("Audios")]
    public AudioSource audioSource;
    public AudioSource Beep;
    public AudioSource BeepEnd;
    public AudioClip audioMove;
    public AudioClip audioLeft;
    public AudioClip audioRight;
    public AudioClip audio25;
    public AudioClip audio50;
    public AudioClip audio75;
    public AudioClip audiocomplete;

    private List<char> sequence;

    [HideInInspector] public string state = "";

    private void Start()
    {
        moveDistance = 10f;

        // Generate an L/R sequence up front
        sequence = GenerateSequence(nSamples);

        // Begin a step-by-step spawn + move + turn sequence
        StartCoroutine(MoveTurnSequence());
    }

    private IEnumerator MoveTurnSequence()
    {
        // For however many chunks you want:
        yield return new WaitForSeconds(waitDuration);

        int perc25 = nSamples * 3 * 1/4;
        int perc50 = nSamples * 3 * 1/2;
        int perc75 = nSamples * 3 * 3/4;

        for (int i = 0; i < sequence.Count; i++)
        {

            state = "S";
            if (LSLOutlet.Instance != null) { LSLOutlet.Instance.PushMarker(state); }
            yield return new WaitForSeconds(waitDuration);

            // 1) Move forward
            if (sequence[i] == 'F')
            {
                audioSource.clip = audioMove;
                audioSource.Play();
                yield return new WaitForSeconds(instrDuration);
                Beep.Play();
                state = "F";
                if (LSLOutlet.Instance != null) { LSLOutlet.Instance.PushMarker(state); }
                yield return new WaitForSeconds(MIDuration);
                yield return StartCoroutine(MoveForward(moveDistance, moveDuration));
            }

            // 3) Turn left
            if (sequence[i] == 'L')
            {
                audioSource.clip = audioLeft;
                audioSource.Play();
                yield return new WaitForSeconds(instrDuration);
                Beep.Play();
                state = "L";
                if (LSLOutlet.Instance != null) { LSLOutlet.Instance.PushMarker(state); }
                yield return new WaitForSeconds(MIDuration);
                yield return StartCoroutine(TurnRight(-90f, turnDuration));
            }

            // 3) Turn right
            if (sequence[i] == 'R')
            {
                audioSource.clip = audioRight;
                audioSource.Play();
                yield return new WaitForSeconds(instrDuration);
                Beep.Play();
                state = "R";
                if (LSLOutlet.Instance != null) { LSLOutlet.Instance.PushMarker(state); }
                yield return new WaitForSeconds(MIDuration);
                yield return StartCoroutine(TurnRight(90f, turnDuration));
            }

            BeepEnd.Play();

            if (i == perc25-1)
            {
                yield return new WaitForSeconds(0.5f);
                audioSource.clip = audio25;
                audioSource.Play();
                yield return new WaitForSeconds(waitDuration);
            }

            if (i == perc50 - 1)
            {
                yield return new WaitForSeconds(0.5f);
                audioSource.clip = audio50;
                audioSource.Play();
                yield return new WaitForSeconds(waitDuration);
            }

            if (i == perc75 - 1)
            {
                yield return new WaitForSeconds(0.5f);
                audioSource.clip = audio75;
                audioSource.Play();
                yield return new WaitForSeconds(waitDuration);
            }
        }

        yield return new WaitForSeconds(0.5f);
        audioSource.clip = audiocomplete;
        audioSource.Play();
        Debug.Log("Session Complete");
    }

    private IEnumerator MoveForward(float distance, float duration)
    {
        // Record initial and target positions
        Vector3 startPos = yawPivot.position;
        //Vector3 endPos = transform.position + transform.forward * distance;

        // Combine yawJoint rotation and camera yaw
        Vector3 moveDir = Quaternion.Euler(0, yawPivot.eulerAngles.y, 0) * Vector3.forward;
        Vector3 endPos = yawPivot.position + moveDir.normalized * distance;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            // Lerp over time
            float t = elapsedTime / duration;
            yawPivot.position = Vector3.Lerp(startPos, endPos, t);

            elapsedTime += Time.deltaTime;
            yield return null; // Continue next frame
        }

        // Force final position in case of any rounding
        yawPivot.position = endPos;
    }

    private IEnumerator TurnRight(float angle, float duration)
    {
        Quaternion startRot = yawPivot.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, angle, 0);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            yawPivot.rotation = Quaternion.Slerp(startRot, endRot, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Snap yaw to the nearest 90°
        Vector3 finalEuler = yawPivot.eulerAngles;
        finalEuler.y = Mathf.Round(finalEuler.y / 90f) * 90f;
        yawPivot.eulerAngles = finalEuler; 

    }

    private List<char> GenerateSequence(int count)
    {
        // We'll generate a sequence of length 4*count,
        // containing exactly `count` 'S', `count` 'F', `count` 'L' and `count` 'R',
        // ensuring no more than 2 identical letters in a row.

        // Keep track of how many remain of each
        Dictionary<char, int> counts = new Dictionary<char, int>
        {
            { 'F', count },
            { 'L', count },
            { 'R', count },
        };

        List<char> sequence = new List<char>();
        int totalNeeded = 3 * count;

        // We'll do a quick shuffle with a single Random
        System.Random rng = new System.Random();

        bool Backtrack()
        {
            // If we've placed everything, return success
            if (sequence.Count == totalNeeded)
            {
                return true;
            }

            // Gather valid candidates: must have a remaining count > 0
            // AND not form 3 in a row
            List<char> candidates = new List<char>();
            foreach (var kvp in counts)
            {
                char letter = kvp.Key;
                int remain = kvp.Value;
                if (remain > 0)
                {
                    int seqLen = sequence.Count;
                    if (seqLen >= 2 &&
                        sequence[seqLen - 1] == letter &&
                        sequence[seqLen - 2] == letter)
                    {
                        // Would form triple in a row, skip
                        continue;
                    }
                    candidates.Add(letter);
                }
            }

            if (candidates.Count == 0) return false;

            // Shuffle the candidates to get a random ordering
            Shuffle(candidates, rng);

            // Try each candidate
            foreach (char letter in candidates)
            {
                counts[letter]--;
                sequence.Add(letter);

                if (Backtrack()) return true;

                // Backtrack
                sequence.RemoveAt(sequence.Count - 1);
                counts[letter]++;
            }

            // No success with any candidate
            return false;
        }

        bool success = Backtrack();
        return success ? sequence : null;
    }

    // Fisher-Yates shuffle for a List<T>
    private void Shuffle<T>(List<T> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = rng.Next(i + 1);
            T temp = list[i];
            list[i] = list[swapIndex];
            list[swapIndex] = temp;
        }
    }

}

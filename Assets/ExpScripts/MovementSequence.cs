using UnityEngine;
using System.Collections;


public class MovementSequence : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 5f;      // X meters forward
    public float moveDuration = 2f;      // S seconds to move

    [Header("Rotation Settings")]
    public float turnAngle = 90f;        // Degrees to turn
    public float turnDuration = 2f;      // Seconds to take turning

    [Header("Stops")]
    public float stopDuration = 1f;      // 1 second stops

    private void Start()
    {
        // Start the coroutine when the script starts
        StartCoroutine(MoveTurnSequence());
    }

    private IEnumerator MoveTurnSequence()
    {
        // 1) Move forward
        yield return StartCoroutine(MoveForward(moveDistance, moveDuration));

        // 2) Wait 1 second
        yield return new WaitForSeconds(stopDuration);

        // 3) Turn right
        yield return StartCoroutine(TurnRight(turnAngle, turnDuration));

        // 4) Wait 1 second
        yield return new WaitForSeconds(stopDuration);

        // 5) Move forward again
        yield return StartCoroutine(MoveForward(moveDistance, moveDuration));

        // Done! (Feel free to extend with more steps.)
    }

    private IEnumerator MoveForward(float distance, float duration)
    {
        // Record initial and target positions
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + transform.forward * distance;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            // Lerp over time
            float t = elapsedTime / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);

            elapsedTime += Time.deltaTime;
            yield return null; // Continue next frame
        }

        // Force final position in case of any rounding
        transform.position = endPos;
    }

    private IEnumerator TurnRight(float angle, float duration)
    {
        // Current rotation to target rotation
        Quaternion startRot = transform.rotation;
        Quaternion endRot = transform.rotation * Quaternion.Euler(0, angle, 0);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            elapsedTime += Time.deltaTime;
            yield return null; // Continue next frame
        }

        // Force final rotation
        transform.rotation = endRot;
    }
}


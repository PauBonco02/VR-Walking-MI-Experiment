using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    public Transform yawJoint; // Assign this in the Inspector (parent or directional pivot)

    float yaw = 0f;
    float pitch = 0f;

    void Update()
    {
        // Mouse look
        yaw += lookSpeed * Input.GetAxis("Mouse X");
        pitch -= lookSpeed * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.localEulerAngles = new Vector3(pitch, yaw, 0f);

        // --- Combine yawJoint Y rotation + local yaw ---
        float totalYaw = yawJoint.eulerAngles.y + yaw;
        Vector3 flatForward = Quaternion.Euler(0, totalYaw, 0) * Vector3.forward;
        Vector3 flatRight = Quaternion.Euler(0, totalYaw, 0) * Vector3.right;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float y = 0;

        if (Input.GetKey(KeyCode.E)) y += 1;
        if (Input.GetKey(KeyCode.Q)) y -= 1;

        Vector3 move = flatRight * x + flatForward * z + Vector3.up * y;
        yawJoint.position += move * moveSpeed * Time.deltaTime; // move the parent
    }
}

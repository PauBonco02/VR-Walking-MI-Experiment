using UnityEngine;

public class FollowCameraXZNoRotation : MonoBehaviour
{
    public Transform cameraTransform;

    // We only care about the XZ offset to the camera.
    private Vector2 xzOffset;

    // We lock this object's own Y position (taken from the scene at Start).
    private float lockedY;

    // We'll also preserve this object's original rotation.
    private Quaternion originalRotation;

    void Start()
    {
        // 1) Record the initial XZ offset:
        //    objectPos.xz - cameraPos.xz
        xzOffset = new Vector2(
            transform.position.x - cameraTransform.position.x,
            transform.position.z - cameraTransform.position.z
        );

        // 2) Remember this object's Y so we never change it.
        lockedY = transform.position.y;

        // 3) Preserve the rotation it has at startup.
        originalRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // 4) Recompute the XZ position by adding the original XZ offset
        //    to the camera's current XZ. The Y stays locked.
        float newX = cameraTransform.position.x + xzOffset.x;
        float newZ = cameraTransform.position.z + xzOffset.y;

        transform.position = new Vector3(newX, lockedY, newZ);

        // 5) Keep the original rotation (ignore camera rotation)
        transform.rotation = originalRotation;
    }
}

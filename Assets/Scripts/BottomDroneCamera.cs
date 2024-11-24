using UnityEngine;

public class BottomDroneCamera : MonoBehaviour
{
    public Transform droneTransform; // Reference to the drone's transform
    public float rotationSpeed = 100f; // Speed of camera rotation
    public float maxUpAngle = 80f; // Maximum angle the camera can tilt upward
    public float minDownAngle = -80f; // Maximum angle the camera can tilt downward

    private float pitch = 0f; // Current pitch (rotation around the X-axis)
    private float yaw = 0f; // Current yaw (rotation around the Y-axis)
    private bool isRotating = false; // Tracks whether the camera is being rotated

    void Start()
    {
        if (droneTransform == null)
        {
            Debug.LogError("Drone Transform is not assigned to the BottomDroneCamera script.");
        }

        // Align the camera with the drone's bottom at start
        AlignCameraWithDroneBottom();
    }

    void Update()
    {
        // Check if the right mouse button is held down
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true; // Start rotating
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isRotating = false; // Stop rotating
        }

        // Perform rotation only while RMB is pressed
        if (isRotating)
        {
            // Get mouse movement
            float mouseX = Input.GetAxis("Mouse X"); // Horizontal mouse movement
            float mouseY = Input.GetAxis("Mouse Y"); // Vertical mouse movement

            // Update yaw (left-right rotation around the Y-axis)
            yaw += mouseX * rotationSpeed * Time.deltaTime;

            // Update pitch (up-down rotation around the X-axis)
            pitch -= mouseY * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minDownAngle, maxUpAngle); // Clamp the pitch
        }

        // Apply the rotation
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.rotation = rotation;

        // Keep the camera attached to the bottom of the drone
        transform.position = droneTransform.position - droneTransform.up * 0.5f; // Adjust 0.5f as needed for distance from the drone
    }

    /// <summary>
    /// Aligns the camera to face downward at the start.
    /// </summary>
    private void AlignCameraWithDroneBottom()
    {
        // Position the camera slightly below the drone
        transform.position = droneTransform.position - droneTransform.up * 0.5f;

        // Face directly downward
        pitch = 45f;
        yaw = 0f;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}

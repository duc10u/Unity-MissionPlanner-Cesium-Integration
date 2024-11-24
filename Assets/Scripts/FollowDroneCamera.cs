using UnityEngine;

public class FollowDroneCamera : MonoBehaviour
{
    public Transform droneTransform; // Reference to the drone (CesiumGlobeAnchor GameObject)
    public UpdateCesiumAnchor droneAnchor; // Reference to the UpdateCesiumAnchor script
    public float followDistance = 10f; // Distance behind the drone
    public float followHeight = 5f; // Height above the drone

    private bool isFollowing = false; // Tracks whether the camera should start following

    void Start()
    {
        if (droneTransform == null || droneAnchor == null)
        {
            Debug.LogError("Drone Transform or UpdateCesiumAnchor reference is missing!");
        }
    }

    void LateUpdate()
    {
        if (droneAnchor == null || droneTransform == null) return;

        // Wait for the drone's SetInitialPosition() to complete
        if (!isFollowing)
        {
            if (droneAnchor.isInitialized)
            {
                // Set the initial position behind and above the drone
                SetInitialCameraPosition();
                isFollowing = true;
            }
            return; // Skip following until initialized
        }

        // Directly follow the drone's position
        Vector3 desiredPosition = droneTransform.position
            - droneTransform.forward * followDistance
            + Vector3.up * followHeight;

        transform.position = desiredPosition;

        // Directly rotate to look at the drone
        transform.rotation = Quaternion.LookRotation(droneTransform.position - transform.position);
    }

    /// <summary>
    /// Set the initial camera position behind and above the drone.
    /// </summary>
    private void SetInitialCameraPosition()
    {
        Vector3 initialPosition = droneTransform.position
            - droneTransform.forward * followDistance
            + Vector3.up * followHeight;

        transform.position = initialPosition;

        // Look at the drone
        Quaternion initialRotation = Quaternion.LookRotation(droneTransform.position - transform.position);
        transform.rotation = initialRotation;

        Debug.Log("Camera initial position and rotation set.");
    }
}

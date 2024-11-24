using UnityEngine;

public class DroneBladeController : MonoBehaviour
{
    public float maxSpinSpeed = 1000f; // Maximum spin speed (RPM or arbitrary unit)
    public float spinAcceleration = 50f; // Acceleration rate (units per second)
    public float spinDeceleration = 50f; // Deceleration rate (units per second)

    private float currentSpinSpeed = 0f; // Current spin speed
    private bool isSpinningUp = false; // True if the blade is spinning up
    private bool isSpinningDown = false; // True if the blade is spinning down

    public UDPReceiverDisplay udpReceiver; // Reference to the UDPReceiverDisplay script

    void Start()
    {
        if (udpReceiver == null)
        {
            Debug.LogError("UDPReceiverDisplay not found in the scene. Please ensure it is added to the scene.");
        }
    }

    void Update()
    {
        // Check the StatusMessage from the UDPReceiverDisplay script
        if (udpReceiver != null)
        {
            string statusMessage = udpReceiver.StatusMessage;

            // Only proceed if the status message is not null or empty
            if (!string.IsNullOrEmpty(statusMessage))
            {
                // React to "Arming" and "Disarming" keywords
                if (statusMessage.Contains("Arming"))
                {
                    StartSpinning();
                }
                else if (statusMessage.Contains("Disarming"))
                {
                    StopSpinning();
                }
            }
        }

        // Control the spin speed
        if (isSpinningUp)
        {
            currentSpinSpeed = Mathf.Min(currentSpinSpeed + spinAcceleration * Time.deltaTime, maxSpinSpeed);
        }
        else if (isSpinningDown)
        {
            currentSpinSpeed = Mathf.Max(currentSpinSpeed - spinDeceleration * Time.deltaTime, 0f);
        }

        // Apply the rotation to the blade
        transform.Rotate(Vector3.up, currentSpinSpeed * Time.deltaTime, Space.Self);
    }

    /// <summary>
    /// Start spinning the blades.
    /// </summary>
    private void StartSpinning()
    {
        isSpinningUp = true;
        isSpinningDown = false;
    }

    /// <summary>
    /// Stop spinning the blades.
    /// </summary>
    private void StopSpinning()
    {
        isSpinningUp = false;
        isSpinningDown = true;
    }
}

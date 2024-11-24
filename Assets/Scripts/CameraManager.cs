using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera[] cameras; // Array of cameras to switch between
    private int currentCameraIndex = 0; // Tracks the currently active camera

    void Start()
    {
        if (cameras == null || cameras.Length == 0)
        {
            Debug.LogError("No cameras assigned to the CameraManager.");
            return;
        }

        // Initialize the first camera as the active camera
        SetActiveCamera(currentCameraIndex);
    }

    void Update()
    {
        // Listen for the spacebar press to switch the active camera
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwitchToNextCamera();
        }
    }

    /// <summary>
    /// Switches to the next camera in the array.
    /// </summary>
    private void SwitchToNextCamera()
    {
        // Move to the next camera, looping back to the first if necessary
        currentCameraIndex = (currentCameraIndex + 1) % cameras.Length;

        // Update the active camera
        SetActiveCamera(currentCameraIndex);
    }

    /// <summary>
    /// Sets the active camera by enabling its Camera component and tagging it as MainCamera.
    /// </summary>
    /// <param name="index">The index of the camera to activate.</param>
    private void SetActiveCamera(int index)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (i == index)
            {
                // Enable the active camera and set its tag to MainCamera
                cameras[i].enabled = true;
                cameras[i].tag = "MainCamera";
                Debug.Log($"Switched to camera: {cameras[i].name}");
            }
            else
            {
                // Disable the other cameras and remove their MainCamera tag
                cameras[i].enabled = false;
                cameras[i].tag = "Untagged";
            }
        }
    }
}

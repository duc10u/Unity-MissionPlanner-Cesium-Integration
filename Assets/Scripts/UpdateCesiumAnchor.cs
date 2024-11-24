using UnityEngine;
using Unity.Mathematics;
using CesiumForUnity;
using System.Collections;
using System.Threading.Tasks;

public class UpdateCesiumAnchor : MonoBehaviour
{
    public UDPReceiverDisplay udpReceiver; // Reference to the UDPReceiverDisplay script
    private CesiumGlobeAnchor globeAnchor; // The CesiumGlobeAnchor component on this GameObject
    public Cesium3DTileset terrainTileset; // Cesium World Terrain
    public InitializeCesiumOrigin initializeCesiumOrigin; // Reference to the InitializeCesiumOrigin script

    private double initialLatitude;
    private double initialLongitude;
    public double sampledHeight;
    public double modelOffset;

    private double3 currentPosition; // Current smoothed position
    private quaternion currentRotation; // Current smoothed rotation

    private double3 targetPosition; // Target position
    private quaternion targetRotation; // Target rotation

    public float positionSmoothingFactor = 0.1f; // Controls the blend speed for position (lower = smoother)
    public float rotationSmoothingFactor = 0.1f; // Controls the blend speed for rotation (lower = smoother)

    public bool isInitialized = false; // Tracks whether the initial position has been set

    void Start()
    {
        globeAnchor = GetComponent<CesiumGlobeAnchor>();

        if (globeAnchor == null)
        {
            Debug.LogError("CesiumGlobeAnchor component is missing!");
            return;
        }

        if (udpReceiver == null)
        {
            Debug.LogError("UDPReceiverDisplay reference is missing!");
            return;
        }

        if (terrainTileset == null)
        {
            Debug.LogError("Cesium3DTileset reference is missing!");
            return;
        }

        if (initializeCesiumOrigin == null)
        {
            Debug.LogError("InitializeCesiumOrigin reference is missing!");
            return;
        }

        StartCoroutine(WaitForOriginAndTileset());
    }

    IEnumerator WaitForOriginAndTileset()
    {
        while (!initializeCesiumOrigin.IsInitialized)
        {
            Debug.Log("Waiting for Cesium Origin to be initialized...");
            yield return null;
        }

        initialLatitude = udpReceiver.LatitudeValue;
        initialLongitude = udpReceiver.LongitudeValue;

        if (initialLatitude == 0 || initialLongitude == 0)
        {
            Debug.LogWarning("Initial latitude or longitude is invalid. Skipping terrain height sampling.");
            yield break;
        }

        while (terrainTileset.ComputeLoadProgress() < 99.0f)
        {
            yield return null;
        }

        Debug.Log("Cesium3DTileset is ready, sampling terrain height...");
        yield return StartCoroutine(SampleInitialTerrainHeight());

        SetInitialPosition();
    }

    public IEnumerator SampleInitialTerrainHeight()
    {
        if (terrainTileset != null)
        {
            double3[] positions = new double3[] { new double3(initialLongitude, initialLatitude, 3500.0) };
            Task<CesiumSampleHeightResult> terrainTask = terrainTileset.SampleHeightMostDetailed(positions);
            yield return new WaitForTask(terrainTask);

            Debug.Log("Completed Terrain Sampling");
            CesiumSampleHeightResult result = terrainTask.Result;

            if (result != null)
            {
                for (int i = 0; i < result.sampleSuccess.Length; i++)
                {
                    if (result.sampleSuccess[i])
                    {
                        sampledHeight = result.longitudeLatitudeHeightPositions[i].z;
                        Debug.Log($"Sampled Terrain Height: {sampledHeight}");
                    }
                    else
                    {
                        Debug.LogWarning("Could not sample terrain at the given position.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Terrain height sampling failed.");
            }
        }
    }

    private void SetInitialPosition()
    {
        double altitude = udpReceiver.AltitudeValue;
        double3 initialPosition = new double3(
            initialLongitude,
            initialLatitude,
            altitude + sampledHeight + modelOffset
        );

        currentPosition = initialPosition;
        targetPosition = initialPosition;
        globeAnchor.longitudeLatitudeHeight = initialPosition;

        float roll = udpReceiver.RollValue;
        float pitch = udpReceiver.PitchValue;
        float yaw = udpReceiver.YawValue;

        Quaternion rollRotation = Quaternion.AngleAxis(roll * Mathf.Rad2Deg, Vector3.forward);
        Quaternion pitchRotation = Quaternion.AngleAxis(-pitch * Mathf.Rad2Deg, Vector3.right);
        Quaternion yawRotation = Quaternion.AngleAxis(-yaw * Mathf.Rad2Deg, Vector3.up);

        currentRotation = yawRotation * pitchRotation * rollRotation;
        targetRotation = currentRotation;
        globeAnchor.rotationEastUpNorth = currentRotation;

        Debug.Log($"Initial position set: Lat={initialLatitude}, Lon={initialLongitude}, Alt={altitude + sampledHeight}");
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        // Get new target data from UDPReceiver
        double3 receivedPosition = new double3(
            udpReceiver.LongitudeValue,
            udpReceiver.LatitudeValue,
            udpReceiver.AltitudeValue + sampledHeight + modelOffset
        );

        Quaternion rollRotation = Quaternion.AngleAxis(udpReceiver.RollValue * Mathf.Rad2Deg, Vector3.forward);
        Quaternion pitchRotation = Quaternion.AngleAxis(-udpReceiver.PitchValue * Mathf.Rad2Deg, Vector3.right);
        Quaternion yawRotation = Quaternion.AngleAxis(-udpReceiver.YawValue * Mathf.Rad2Deg, Vector3.up);
        quaternion receivedRotation = yawRotation * pitchRotation * rollRotation;

        // Update target position and rotation
        targetPosition = receivedPosition;
        targetRotation = receivedRotation;

        // Exponential smoothing for position
        currentPosition = math.lerp(currentPosition, targetPosition, positionSmoothingFactor);

        // Exponential smoothing for rotation
        currentRotation = math.slerp(currentRotation, targetRotation, rotationSmoothingFactor);

        // Update CesiumGlobeAnchor
        globeAnchor.longitudeLatitudeHeight = currentPosition;
        globeAnchor.rotationEastUpNorth = currentRotation;
    }
}

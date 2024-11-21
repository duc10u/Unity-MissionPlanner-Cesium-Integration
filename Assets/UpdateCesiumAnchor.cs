using UnityEngine;
using Unity.Mathematics;
using CesiumForUnity;
using System.Collections;

public class UpdateCesiumAnchor : MonoBehaviour
{
    public UDPReceiverDisplay udpReceiver; // Reference to the UDPReceiverDisplay script
    private CesiumGlobeAnchor globeAnchor; // The CesiumGlobeAnchor component on this GameObject
    public Cesium3DTileset terrainTileset; // Cesium World Terrain
    public InitializeCesiumOrigin initializeCesiumOrigin; // Reference to the InitializeCesiumOrigin script

    private double initialLatitude;
    private double initialLongitude;
    private double initTerrainHeight;

    private double altitude;

    void Start()
    {
        // Get the CesiumGlobeAnchor component on this GameObject
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

        // Start the coroutine to wait for the InitializeCesiumOrigin script and tileset readiness
        StartCoroutine(WaitForOriginAndTileset());
    }

    IEnumerator WaitForOriginAndTileset()
    {
        // Wait until InitializeCesiumOrigin has completed its initialization
        while (!initializeCesiumOrigin.IsInitialized)
        {
            Debug.Log("Waiting for Cesium Origin to be initialized...");
            yield return null; // Wait for the next frame
        }

        // Once the origin is set, retrieve the latitude and longitude from the UDPReceiverDisplay
        initialLatitude = udpReceiver.LatitudeValue;
        initialLongitude = udpReceiver.LongitudeValue;

        if (initialLatitude == 0 || initialLongitude == 0)
        {
            Debug.LogWarning("Initial latitude or longitude is invalid. Skipping terrain height sampling.");
            yield break;
        }

        // Wait for the Cesium tileset to load (progress > 99%)
        while (terrainTileset.ComputeLoadProgress() < 99.0f)
        {
            yield return null; // Wait for the next frame
        }

        Debug.Log("Cesium3DTileset is ready, sampling terrain height...");

        // Now sample the terrain height
        SampleInitialTerrainHeight();
    }

    async void SampleInitialTerrainHeight()
    {
        try
        {
            // Prepare the position as a double3 (longitude, latitude, height)
            double3 position = new double3(initialLongitude, initialLatitude, 0.0);

            Debug.Log($"Sampling height at Lat: {initialLatitude}, Lon: {initialLongitude}");

            // Use SampleHeightMostDetailed to get the terrain height
            CesiumSampleHeightResult result = await terrainTileset.SampleHeightMostDetailed(position);

            if (result != null)
            {
               Debug.Log(result);
            }
            else
            {
                Debug.LogWarning("Terrain height sampling failed.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error sampling terrain height: {ex.Message}");
        }
    }

    void Update()
    {
        // Check if both components are available
        if (globeAnchor != null && udpReceiver != null)
        {
            // Get the current latitude, longitude, and altitude from UDPReceiverDisplay
            double latitude = udpReceiver.LatitudeValue;
            double longitude = udpReceiver.LongitudeValue;
            double altitude = udpReceiver.AltitudeValue;

            // Update the CesiumGlobeAnchor position
            globeAnchor.longitudeLatitudeHeight = new double3(longitude, latitude, altitude);

            // Optional: Log the updated position for debugging
            // Debug.Log($"CesiumGlobeAnchor updated to Lat: {latitude}, Lon: {longitude}, Alt: {altitude} meters");
        }
    }
}

using UnityEngine;
using CesiumForUnity;
using Unity.Mathematics;
using System.Collections;

public class InitializeCesiumOrigin : MonoBehaviour
{
    public UDPReceiverDisplay udpReceiver; // Reference to the UDPReceiverDisplay script
    private CesiumGlobeAnchor globeAnchor; // The CesiumGlobeAnchor component on the Cesium Origin GameObject

    public float initializationDelay = 0.2f; // Delay in seconds before initializing the origin

    void Start()
    {
        // Get the CesiumGlobeAnchor component attached to this GameObject
        globeAnchor = GetComponent<CesiumGlobeAnchor>();

        if (globeAnchor == null)
        {
            Debug.LogError("CesiumGlobeAnchor component is missing on the Cesium Origin GameObject!");
            return;
        }

        if (udpReceiver == null)
        {
            Debug.LogError("UDPReceiverDisplay reference is missing!");
            return;
        }

        // Start the delayed initialization coroutine
        StartCoroutine(InitializeOriginAfterDelay());
    }

    IEnumerator InitializeOriginAfterDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(initializationDelay);

        // Get initial latitude and longitude from UDPReceiverDisplay
        double initialLatitude = udpReceiver.LatitudeValue;
        double initialLongitude = udpReceiver.LongitudeValue;

        // Check for valid values, retry if invalid
        while (initialLatitude == 0 || initialLongitude == 0)
        {
            Debug.LogWarning("Initial latitude or longitude is invalid. Waiting for valid data...");
            yield return null; // Wait for the next frame
            initialLatitude = udpReceiver.LatitudeValue;
            initialLongitude = udpReceiver.LongitudeValue;
        }

        // Initialize the CesiumGlobeAnchor position
        InitializeOrigin(initialLatitude, initialLongitude);
    }

    private void InitializeOrigin(double latitude, double longitude)
    {
        // Set the CesiumGlobeAnchor's position using latitude and longitude
        globeAnchor.longitudeLatitudeHeight = new double3(longitude, latitude, 2250.0);

        // Log the initialized position for debugging
        Debug.Log($"Cesium Origin initialized at Lat: {latitude}, Lon: {longitude}.");
    }
}

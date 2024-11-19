using UnityEngine;
using Unity.Mathematics;
using CesiumForUnity;

public class UpdateCesiumAnchor : MonoBehaviour
{
    public UDPReceiverDisplay udpReceiver; // Reference to the UDPReceiverDisplay script
    private CesiumGlobeAnchor globeAnchor; // The CesiumGlobeAnchor component on this GameObject
    public CesiumGlobeAnchor originAnchor;

    double latitude;
    double longitude;
    double altitude;
    double originHeight;

    void Start()
    {
        // Get the CesiumGlobeAnchor component on this GameObject
        globeAnchor = GetComponent<CesiumGlobeAnchor>();
        originAnchor = GetComponent<CesiumGlobeAnchor>();

        if (globeAnchor == null)
        {
            Debug.LogError("CesiumGlobeAnchor component is missing!");
        }

        if (udpReceiver == null)
        {
            Debug.LogError("UDPReceiverDisplay reference is missing!");
        }

        // Initialize the position
        latitude = udpReceiver.LatitudeValue;
        longitude = udpReceiver.LongitudeValue;
        altitude = udpReceiver.AltitudeValue + originHeight;
    }

    void Update()
    {
        // Check if both components are available
        if (globeAnchor != null && udpReceiver != null)
        {
            // Get the current latitude, longitude, and altitude from UDPReceiverDisplay
            latitude = udpReceiver.LatitudeValue;
            longitude = udpReceiver.LongitudeValue;
            altitude = udpReceiver.AltitudeValue;

            // Update the CesiumGlobeAnchor position
            globeAnchor.longitudeLatitudeHeight = new double3(latitude, longitude, altitude);

            // Optional: Log the updated position for debugging
            Debug.Log($"CesiumGlobeAnchor updated to Lat: {latitude}, Lon: {longitude}, Alt: {altitude} meters");
        }
    }
}

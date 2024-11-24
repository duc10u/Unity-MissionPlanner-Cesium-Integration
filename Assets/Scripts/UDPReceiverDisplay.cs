using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using TMPro;
using System;

public class UDPReceiverDisplay : MonoBehaviour
{
    // Display fields for latitude, longitude, altitude, roll, pitch, yaw, and statustext
    public TMP_Text LatitudeText;
    public TMP_Text LongitudeText;
    public TMP_Text AltitudeText;
    public TMP_Text RollText;
    public TMP_Text PitchText;
    public TMP_Text YawText;

    // Port and IP configuration
    public int port = 14555;
    public string ip = "127.0.0.1";

    // UDP Client settings
    UdpClient listener;
    IPEndPoint groupEP;
    Thread listenThread;
    public bool running;
    private int receiveTimeoutDurationMs = 2000;

    // Variables to hold the MAVLink data to be displayed
    private double latValue;
    private double lonValue;
    private double altValue;
    private float rollValue;
    private float pitchValue;
    private float yawValue;
    private string statusMessage; // Variable to store STATUSTEXT message

    private string latitude;
    private string longitude;
    private string altitude;
    private string roll;
    private string pitch;
    private string yaw;

    // Public properties to access the current telemetry values
    public double LatitudeValue => latValue;
    public double LongitudeValue => lonValue;
    public double AltitudeValue => altValue;
    public float RollValue => rollValue;
    public float PitchValue => pitchValue;
    public float YawValue => yawValue;
    public string StatusMessage => statusMessage;

    void Start()
    {
        SetUpClient();
        StartListening();
    }

    void SetUpClient()
    {
        groupEP = new IPEndPoint(IPAddress.Any, port);
        listener = new UdpClient(port);
    }

    private void OnDisable()
    {
        running = false;
        listener.Close();
    }

    public void StartListening()
    {
        // If the thread is already running, stop it first
        if (listenThread != null) { StopThread(); }

        // Receive on a separate thread
        ThreadStart threadStarter = new ThreadStart(StartReceiver);
        listenThread = new Thread(threadStarter);
        listenThread.Start();
    }

    private void StopThread()
    {
        running = false;
        listenThread.Join();
    }

    // Thread function to receive packets
    void StartReceiver()
    {
        listener.Client.ReceiveTimeout = receiveTimeoutDurationMs;  // Set timeout
        running = true;
        while (running)
        {
            GetPacket();
        }
        listener.Close();
    }

    void GetPacket()
    {
        byte[] receivedBytes = null;

        try
        {
            receivedBytes = listener.Receive(ref groupEP);  // Receive data

            if (receivedBytes != null)
            {
                // Parse the bytes into a MAVLink message
                MAVLink.MAVLinkMessage mavMessage = new MAVLink.MAVLinkMessage(receivedBytes);

                if (mavMessage != null)
                {
                    // Handle GLOBAL_POSITION_INT for GPS data
                    if (mavMessage.data.GetType() == typeof(MAVLink.mavlink_global_position_int_t))
                    {
                        var gpsPacket = (MAVLink.mavlink_global_position_int_t)mavMessage.data;
                        UpdatePositionData(gpsPacket);
                    }
                    // Handle ATTITUDE for roll, pitch, yaw data
                    else if (mavMessage.data.GetType() == typeof(MAVLink.mavlink_attitude_t))
                    {
                        var attitudePacket = (MAVLink.mavlink_attitude_t)mavMessage.data;
                        UpdateAttitudeData(attitudePacket);
                    }
                    // Handle STATUSTEXT for status messages
                    else if (mavMessage.data.GetType() == typeof(MAVLink.mavlink_statustext_t))
                    {
                        var statusPacket = (MAVLink.mavlink_statustext_t)mavMessage.data;
                        UpdateStatusText(statusPacket);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }

        Thread.Sleep(1);
    }

    // Update local variables with position data
    private void UpdatePositionData(MAVLink.mavlink_global_position_int_t position)
    {
        // Latitude and longitude are scaled by 1E7 in MAVLink
        latValue = position.lat / 1e7;
        lonValue = position.lon / 1e7;
        altValue = position.relative_alt / 1000.0; // Convert mm to meters

        // Format strings for UI display
        latitude = $"Latitude: {latValue:F7}";
        longitude = $"Longitude: {lonValue:F7}";
        altitude = $"Altitude: {altValue:F2} m";
    }

    // Update local variables with attitude data
    private void UpdateAttitudeData(MAVLink.mavlink_attitude_t attitude)
    {
        rollValue = attitude.roll;
        pitchValue = attitude.pitch;
        yawValue = attitude.yaw;

        // Format strings for UI display
        roll = $"Roll: {rollValue:F3}";
        pitch = $"Pitch: {pitchValue:F3}";
        yaw = $"Yaw: {yawValue:F3}";
    }

    // Update local variable with STATUSTEXT data
    private void UpdateStatusText(MAVLink.mavlink_statustext_t status)
    {
        // Extract the status message text, ensuring it is null-terminated
        statusMessage = $"Status: {System.Text.Encoding.ASCII.GetString(status.text).TrimEnd('\0')}";
        Debug.Log(statusMessage);
    }

    // Update UI text elements on the main thread
    void Update()
    {
        LatitudeText.text = latitude;
        LongitudeText.text = longitude;
        AltitudeText.text = altitude;

        RollText.text = roll;
        PitchText.text = pitch;
        YawText.text = yaw;
    }
}

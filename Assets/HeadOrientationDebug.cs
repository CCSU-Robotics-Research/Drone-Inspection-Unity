using System;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;

public class HeadOrientationSender : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform headTransform;
    [SerializeField] private TextMeshProUGUI debugText;

    [Header("UDP Settings")]
    [SerializeField] private string destinationIP = "10.15.74.130";
    [SerializeField] private int destinationPort = 5005;
    [SerializeField, Range(1f, 120f)] private float sendRateHz = 30f;

    [Header("Axis Adjustments")]
    [SerializeField] private bool invertRoll = false;
    [SerializeField] private bool invertPitch = false;
    [SerializeField] private bool invertYaw = false;

    private UdpClient udpClient;
    private float sendInterval;
    private float sendTimer;
    private bool udpReady;

    private void Start()
    {
        if (headTransform == null && Camera.main != null)
        {
            headTransform = Camera.main.transform;
        }

        sendInterval = 1f / Mathf.Max(1f, sendRateHz);

        try
        {
            udpClient = new UdpClient();
            udpReady = true;
        }
        catch (Exception e)
        {
            udpReady = false;
            Debug.LogError("Failed to create UDP client: " + e.Message);
        }
    }

    private void Update()
    {
        if (headTransform == null)
        {
            if (Camera.main != null)
            {
                headTransform = Camera.main.transform;
            }
            else
            {
                UpdateDebugText(0f, 0f, 0f, "No Main Camera found.");
                return;
            }
        }

        Vector3 euler = headTransform.rotation.eulerAngles;

        float pitch = NormalizeAngle(euler.x);
        float yaw = NormalizeAngle(euler.y);
        float roll = NormalizeAngle(euler.z);

        if (invertRoll) roll = -roll;
        if (invertPitch) pitch = -pitch;
        if (invertYaw) yaw = -yaw;

        sendTimer += Time.deltaTime;
        string status = udpReady ? "Idle" : "Not Ready";

        if (udpReady && sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            bool sent = SendUdpPacket(roll, pitch, yaw);
            status = sent ? "Sending" : "Send Failed";
        }

        UpdateDebugText(roll, pitch, yaw, status);
    }

    private bool SendUdpPacket(float roll, float pitch, float yaw)
    {
        try
        {
            string payload = $"{roll:F2},{pitch:F2},{yaw:F2}";
            byte[] data = Encoding.UTF8.GetBytes(payload);
            udpClient.Send(data, data.Length, destinationIP, destinationPort);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("UDP send failed: " + e.Message);
            return false;
        }
    }

    private void UpdateDebugText(float roll, float pitch, float yaw, string status)
    {
        if (debugText == null) return;

        debugText.text =
            $"Head Orientation\n" +
            $"Roll:  {roll:F1} deg\n" +
            $"Pitch: {pitch:F1} deg\n" +
            $"Yaw:   {yaw:F1} deg\n\n" +
            $"UDP IP:   {destinationIP}\n" +
            $"UDP Port: {destinationPort}\n" +
            $"Rate:     {sendRateHz:F0} Hz\n" +
            $"Status:   {status}";
    }

    private float NormalizeAngle(float angleDegrees)
    {
        angleDegrees %= 360f;
        if (angleDegrees > 180f)
        {
            angleDegrees -= 360f;
        }
        return angleDegrees;
    }

    private void OnApplicationQuit()
    {
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
        }
    }
}
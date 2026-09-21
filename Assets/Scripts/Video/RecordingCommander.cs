using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

/// <summary>
/// Sends recording commands to the ground station for start/stop.
/// Responsible for toggling recording on/off from the FOV; otherwise
/// this keeps no state record of the recording. The recording
/// indication is overlayed on the video stream with OpenCV.
/// </summary>
public class RecordingCommander : MonoBehaviour
{
    [SerializeField] private string host = "127.0.0.1";
    [SerializeField] private int port = 5011;

    private UdpClient udpClient;

    private void Start()
    {
        try
        {
            udpClient = new UdpClient();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to create UDP client: " + e.Message);
        }
    }

    /// <summary>
    /// Wired to the record button's click event.
    /// </summary>
    public void ToggleRecording()
    {
        Send("record:toggle");
    }

    private void Send(string command)
    {
        if (udpClient == null)
        {
            return;
        }
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(command);
            udpClient.Send(data, data.Length, host, port);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Command send failed: " + e.Message);
        }
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

using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

/// <summary>
/// Receives the ground stations' video stream and displays
/// the frames onto a renderer's texture in HoloLens FOV.
/// 
/// A background thread receives and parses frames. The newest
/// frame is uploaded to the texture. Only the main thread can
/// touch Unity textures.
/// </summary>
public class VideoStreamReceiver : MonoBehaviour
{
    [SerializeField] private string host = "127.0.0.1";
    [SerializeField] private int port = 5010;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private float reconnectDelaySeconds = 2.0f;

    private Thread receiveThread;
    private volatile bool running;

    private readonly object latestLock = new object();
    private byte[] latestJpeg;
    private bool hasNewFrame;

    private Texture2D texture;

    private void Start()
    {
        texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        if (targetRenderer != null)
        {
            targetRenderer.material.mainTexture = texture;
        }
        else
        {
            Debug.LogWarning(
                "VideoStreamReceiver: no Target Renderer assigned");
        }

        running = true;
        receiveThread = new Thread(ReceiveLoop)
        {
            IsBackground = true,
            Name = "video-stream-receiver"
        };
        receiveThread.Start();
    }

    private void Update()
    {
        byte[] jpeg = null;
        lock (latestLock)
        {
            if (hasNewFrame)
            {
                jpeg = latestJpeg;
                hasNewFrame = false;
            }
        }

        if (jpeg != null)
        {
            // Resize texture to frame dimensions
            texture.LoadImage(jpeg);
        }
    }

    private void ReceiveLoop()
    {
        var readBuffer = new byte[65536];


        while (running)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(host, port);
                    Debug.Log($"Video stream connected to {host}:{port}");

                    using (NetworkStream stream = client.GetStream())
                    {
                        var assembler = new FrameAssembler();

                        while (running)
                        {
                            int n = stream.Read(
                                readBuffer, 0, readBuffer.Length);
                            if (n <= 0)
                            {
                                break;
                            }

                            foreach (byte[] payload in
                                assembler.ExtractPayloads(readBuffer, n))
                            {
                                lock (latestLock)
                                {
                                    latestJpeg = payload;
                                    hasNewFrame = true;
                                }
                            }
                        }
                    }
                }
                Debug.Log("Video stream disconnected");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Video stream: {e.Message}; retrying...");
            }

            if (running)
            {
                Thread.Sleep(
                    (int)(reconnectDelaySeconds * 1000.0f));
            }
        }
    }

    private void OnDestroy()
    {
        running = false;
        try
        {
            receiveThread?.Join(500);
        }
        catch (ThreadStateException)
        {
            // If the thread was never started, do nothing...
        }
    }
}

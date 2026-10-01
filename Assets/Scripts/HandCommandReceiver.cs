using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class HandCommandReceiver : MonoBehaviour
{
    public FPSMovement player;
    public HandPointer handPointer;

    private UdpClient client;
    private Thread receiveThread;

    // =========================
    // Movement
    // =========================

    private volatile bool bothHandsOpen = false;

    // =========================
    // Hand Tracking
    // =========================

    private volatile bool isPointing = false;

    private volatile float fingerX = 0.5f;
    private volatile float fingerY = 0.5f;

    void Start()
    {
        client = new UdpClient(5050);

        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        Debug.Log("UDP Receiver started on port 5050");
    }

    // =========================
    // รับข้อมูลจาก Python
    // =========================

    void ReceiveData()
    {
        IPEndPoint endPoint =
            new IPEndPoint(
                IPAddress.Any,
                5050
            );

        try
        {
            while (true)
            {
                byte[] data =
                    client.Receive(
                        ref endPoint
                    );

                string message =
                    Encoding.UTF8
                    .GetString(data)
                    .Trim();

                Debug.Log(
                    "UDP RAW: [" +
                    message +
                    "]"
                );

                // =========================
                // เดินหน้า
                // =========================

                if (message == "FORWARD")
                {
                    bothHandsOpen = true;
                    isPointing = false;

                    Debug.Log(
                        "Received: FORWARD"
                    );
                }

                // =========================
                // หยุด
                // =========================

                else if (message == "STOP")
                {
                    bothHandsOpen = false;
                    isPointing = false;

                    Debug.Log(
                        "Received: STOP"
                    );
                }

                // =========================
                // Tracking นิ้ว
                // POINT,x,y
                // =========================

                else if (message.StartsWith("POINT,"))
                {
                    string[] parts =
                        message.Split(',');

                    if (parts.Length == 3)
                    {
                        if (
                            float.TryParse(
                                parts[1],
                                out float x
                            )
                            &&
                            float.TryParse(
                                parts[2],
                                out float y
                            )
                        )
                        {
                            fingerX = x;
                            fingerY = y;

                            isPointing = true;

                            Debug.Log(
                                "Receiver POINT: " +
                                fingerX +
                                ", " +
                                fingerY
                            );
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // ปิด UDP ตอนออกจากเกม
        }
    }

    // =========================
    // Unity Update
    // =========================

    void Update()
    {
        // =========================
        // ส่งคำสั่งเดิน
        // =========================

        if (player != null)
        {
            player.bothHandsOpen =
                bothHandsOpen;
        }

        // =========================
        // ส่งตำแหน่งให้ HandPointer
        // =========================

        if (handPointer != null)
        {
            handPointer.SetPointing(
                isPointing
            );

            if (isPointing)
            {
                handPointer.SetFingerPosition(
                    fingerX,
                    fingerY
                );
            }
        }

        // =========================
        // ส่งตำแหน่งมือให้ FPSMovement
        // =========================

        if (player != null)
        {
            if (isPointing)
            {
                player.SetHandLook(
                    fingerX,
                    fingerY
                );
            }
            else
            {
                player.StopHandLook();
            }
        }
    }

    // =========================
    // ปิด UDP
    // =========================

    void OnApplicationQuit()
    {
        if (client != null)
        {
            client.Close();
        }

        if (receiveThread != null)
        {
            receiveThread.Abort();
        }
    }
}
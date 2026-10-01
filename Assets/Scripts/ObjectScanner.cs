
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ObjectScanner : MonoBehaviour
{
    public Camera playerCamera;
    public float scanDistance = 20f;

    // Text สำหรับแสดงระยะ
    public TMP_Text scanDistanceText;

    private GameObject scannedObject;
    private Renderer scannedRenderer;
    private Color originalColor;

    void Start()
    {
        // ซ่อนข้อความระยะตอนเริ่มเกม
        if (scanDistanceText != null)
        {
            scanDistanceText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // =========================
        // กด Space เพื่อสแกน
        // =========================

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Scan();
        }

        // =========================
        // อัปเดตระยะ
        // =========================

        if (scannedObject != null &&
            scanDistanceText != null)
        {
            UpdateDistanceText();
        }
    }

    // =====================================
    // Scan
    // =====================================

    void Scan()
    {
        // =========================
        // คืนสี Object เดิม
        // =========================

        if (scannedRenderer != null)
        {
            scannedRenderer.material.color = originalColor;

            scannedRenderer = null;
            scannedObject = null;
        }

        // =========================
        // ซ่อนข้อความเก่า
        // =========================

        if (scanDistanceText != null)
        {
            scanDistanceText.gameObject.SetActive(false);
        }

        // =========================
        // Ray ยิงจาก "กลางหน้าจอ"
        // =========================

        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        RaycastHit hit;

        // =========================
        // ตรวจสอบ Ray
        // =========================

        if (Physics.Raycast(
            ray,
            out hit,
            scanDistance))
        {
            // =========================
            // ตรวจว่าเป็น Object ที่ Scan ได้
            // =========================

            if (hit.collider.CompareTag("Interactable"))
            {
                scannedObject =
                    hit.collider.gameObject;

                scannedRenderer =
                    scannedObject.GetComponent<Renderer>();

                // =========================
                // Highlight Object
                // =========================

                if (scannedRenderer != null)
                {
                    // จำสีเดิม
                    originalColor =
                        scannedRenderer.material.color;

                    // เปลี่ยนเป็นสีเหลือง
                    scannedRenderer.material.color =
                        Color.yellow;
                }

                // =========================
                // คำนวณระยะ
                // =========================

                float distance =
                    Vector3.Distance(
                        transform.position,
                        scannedObject.transform.position
                    );

                Debug.Log(
                    "SCAN พบวัตถุ: " +
                    scannedObject.name
                );

                Debug.Log(
                    "ระยะห่าง: " +
                    distance.ToString("F1") +
                    " m"
                );

                // =========================
                // แสดงข้อความระยะ
                // =========================

                if (scanDistanceText != null)
                {
                    scanDistanceText.gameObject.SetActive(true);

                    scanDistanceText.text =
                        distance.ToString("F1") +
                        " m";

                    UpdateDistanceText();
                }
            }
            else
            {
                Debug.Log(
                    "SCAN ไม่พบวัตถุ Interactable"
                );
            }
        }
        else
        {
            Debug.Log(
                "SCAN ไม่พบวัตถุ"
            );
        }
    }

    // =====================================
    // อัปเดตตำแหน่งข้อความ
    // =====================================

    void UpdateDistanceText()
    {
        if (scannedObject == null ||
            scanDistanceText == null)
        {
            return;
        }

        // =========================
        // ตำแหน่งข้อความใต้ Object
        // =========================

        Vector3 worldPosition =
            scannedObject.transform.position +
            Vector3.down * 1.2f;

        // =========================
        // แปลง 3D → Screen
        // =========================

        Vector3 screenPosition =
            playerCamera.WorldToScreenPoint(
                worldPosition
            );

        // =========================
        // Object อยู่ด้านหน้ากล้อง
        // =========================

        if (screenPosition.z > 0)
        {
            scanDistanceText.transform.position =
                screenPosition;
        }

        // =========================
        // อัปเดตระยะปัจจุบัน
        // =========================

        float distance =
            Vector3.Distance(
                transform.position,
                scannedObject.transform.position
            );

        scanDistanceText.text =
            distance.ToString("F1") +
            " m";
    }
}



using UnityEngine;

public class HandPointer : MonoBehaviour
{
    public Camera playerCamera;
    public float maxDistance = 5f;
    public bool isPointing = false;
    public InfoPopup infoPopup;

    private float fingerX = 0.5f;
    private float fingerY = 0.5f;

    private GameObject lastHitObject;

    void Update()
    {
        // ถ้าไม่ได้ชี้
        if (!isPointing)
        {
            lastHitObject = null;

            if (infoPopup != null)
            {
                infoPopup.Hide();
            }

            return;
        }

        // ตำแหน่งนิ้วบนหน้าจอ
        float screenX = fingerX * Screen.width;
        float screenY = (1f - fingerY) * Screen.height;

        // สร้าง Ray จากตำแหน่งนิ้ว
        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(screenX, screenY, 0)
        );

        // ตรวจวัตถุที่นิ้วชี้ไป
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            Debug.Log(
                "Raycast โดน: " +
                hit.collider.gameObject.name
            );

            if (hit.collider.CompareTag("Interactable"))
            {
                Debug.Log(
                    "กำลังชี้ไปที่: " +
                    hit.collider.gameObject.name
                );

                // ถ้าเปลี่ยนไปชี้วัตถุใหม่
                if (lastHitObject != hit.collider.gameObject)
                {
                    lastHitObject = hit.collider.gameObject;

                    ObjectInfo info =
                        hit.collider.GetComponent<ObjectInfo>();

                    if (info != null && infoPopup != null)
                    {
                        infoPopup.Show(info);
                    }
                }
            }
            else
            {
                lastHitObject = null;

                if (infoPopup != null)
                {
                    infoPopup.Hide();
                }
            }
        }
        else
        {
            // ไม่ได้ชี้โดนอะไร
            lastHitObject = null;

            if (infoPopup != null)
            {
                infoPopup.Hide();
            }
        }
    }

    public void SetFingerPosition(float x, float y)
    {
        fingerX = x;
        fingerY = y;
    }

    public void SetPointing(bool value)
    {
        isPointing = value;
    }
}
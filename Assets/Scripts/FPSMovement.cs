using UnityEngine;
using UnityEngine.InputSystem;

public class FPSMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cameraTransform;

    // =========================
    // Movement
    // =========================
    public float speed = 3f;
    public float gravity = -9.81f;

    public bool bothHandsOpen = false;

    // =========================
    // Mouse Look
    // =========================
    public float mouseSensitivity = 2f;

    private float xRotation = 0f;

    // =========================
    // Hand Look
    // =========================
    public bool handLooking = false;

    public float handLookSensitivity = 120f;

    // ความนุ่มของการหัน
    public float handLookSmooth = 8f;

    private float lastHandX = 0.5f;
    private float lastHandY = 0.5f;

    // ค่าการหมุนที่ Smooth แล้ว
    private float smoothRotationX = 0f;
    private float smoothRotationY = 0f;

    // =========================
    // Gravity
    // =========================
    private Vector3 velocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // =========================
        // Mouse Look
        // =========================

        if (Mouse.current != null && !handLooking)
        {
            Vector2 mouseInput =
                Mouse.current.delta.ReadValue();

            float mouseX =
                mouseInput.x *
                mouseSensitivity *
                Time.deltaTime;

            float mouseY =
                mouseInput.y *
                mouseSensitivity *
                Time.deltaTime;

            // มองขึ้น / ลง
            xRotation -= mouseY;

            xRotation =
                Mathf.Clamp(
                    xRotation,
                    -90f,
                    90f
                );

            cameraTransform.localRotation =
                Quaternion.Euler(
                    xRotation,
                    0f,
                    0f
                );

            // หันซ้าย / ขวา
            transform.Rotate(
                Vector3.up * mouseX
            );
        }

        // =========================
        // Movement
        // =========================

        Vector2 input = Vector2.zero;

        // มือแบ 2 ข้าง = เดินหน้า
        if (bothHandsOpen)
        {
            input.y = 1f;
        }

        // WASD สำหรับทดสอบ
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed)
            {
                input.y = 1f;
            }

            if (Keyboard.current.sKey.isPressed)
            {
                input.y = -1f;
            }

            if (Keyboard.current.aKey.isPressed)
            {
                input.x = -1f;
            }

            if (Keyboard.current.dKey.isPressed)
            {
                input.x = 1f;
            }
        }

        Vector3 move =
            transform.right * input.x +
            transform.forward * input.y;

        controller.Move(
            move.normalized *
            speed *
            Time.deltaTime
        );

        // =========================
        // Gravity
        // =========================

        if (
            controller.isGrounded &&
            velocity.y < 0f
        )
        {
            velocity.y = -2f;
        }

        velocity.y +=
            gravity *
            Time.deltaTime;

        controller.Move(
            velocity *
            Time.deltaTime
        );
    }

    // =========================
    // Hand Look
    // =========================

    public void SetHandLook(
        float handX,
        float handY
    )
    {
        // เริ่มจับตำแหน่งมือ
        if (!handLooking)
        {
            lastHandX = handX;
            lastHandY = handY;

            smoothRotationX = 0f;
            smoothRotationY = 0f;

            handLooking = true;

            return;
        }

        // =========================
        // หาความต่างตำแหน่งมือ
        // =========================

        float deltaX =
            handX - lastHandX;

        float deltaY =
            handY - lastHandY;

        // =========================
        // ป้องกันตำแหน่งกระโดด
        // =========================

        if (
            Mathf.Abs(deltaX) > 0.15f ||
            Mathf.Abs(deltaY) > 0.15f
        )
        {
            lastHandX = handX;
            lastHandY = handY;

            return;
        }

        // =========================
        // คำนวณการหมุน
        // =========================

        float targetRotationX =
            deltaX *
            handLookSensitivity;

        float targetRotationY =
            deltaY *
            handLookSensitivity;

        // =========================
        // Smooth การหมุน
        // =========================

        smoothRotationX =
            Mathf.Lerp(
                smoothRotationX,
                targetRotationX,
                handLookSmooth *
                Time.deltaTime
            );

        smoothRotationY =
            Mathf.Lerp(
                smoothRotationY,
                targetRotationY,
                handLookSmooth *
                Time.deltaTime
            );

        // =========================
        // หันซ้าย / ขวา
        // =========================

        transform.Rotate(
            Vector3.up *
            smoothRotationX
        );

        // =========================
        // มองขึ้น / ลง
        // =========================

        // มือขึ้น = มองขึ้น
        // มือลง = มองลง
        xRotation +=
            smoothRotationY;

        xRotation =
            Mathf.Clamp(
                xRotation,
                -90f,
                90f
            );

        cameraTransform.localRotation =
            Quaternion.Euler(
                xRotation,
                0f,
                0f
            );

        // =========================
        // บันทึกตำแหน่งมือ
        // =========================

        lastHandX = handX;
        lastHandY = handY;
    }

    // =========================
    // หยุด Hand Look
    // =========================

    public void StopHandLook()
    {
        handLooking = false;

        lastHandX = 0.5f;
        lastHandY = 0.5f;

        smoothRotationX = 0f;
        smoothRotationY = 0f;
    }
}
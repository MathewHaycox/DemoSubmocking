
using UnityEngine;

public class FishWander : MonoBehaviour
{
    // ==========================================
    // MOVEMENT
    // ==========================================

    [Header("Movement")]
    [Min(0.001f)]
    public float maxSpeed = 0.08f;

    [Min(0.001f)]
    public float acceleration = 0.08f;

    [Min(1f)]
    public float turnSpeed = 25f;

    [Min(0.01f)]
    public float arrivalDistance = 0.2f;

    [Min(0.01f)]
    public float slowdownDistance = 0.6f;


    // ==========================================
    // WANDERING
    // ==========================================

    [Header("Wandering")]
    [Min(0.1f)]
    public float radius = 1.5f;


    // ==========================================
    // GROUND PROTECTION
    // ==========================================

    [Header("Ground Protection")]
    public Collider fishCollider;

    public LayerMask groundLayer;

    [Min(0f)]
    public float groundClearance = 0.2f;

    [Min(0.1f)]
    public float raycastHeight = 5f;

    [Min(0.1f)]
    public float raycastDistance = 20f;

    [Min(0.01f)]
    public float groundCorrectionSpeed = 2f;


    // ==========================================
    // VERTICAL SWIMMING
    // ==========================================

    [Header("Vertical Swimming")]
    [Min(0f)]
    public float bobAmplitude = 0.08f;

    [Min(0.01f)]
    public float bobFrequency = 1.2f;


    // ==========================================
    // IDLE ROTATION
    // ==========================================

    [Header("Idle Rotation")]
    [Range(0f, 90f)]
    public float idleTurnAngle = 20f;

    [Min(0.1f)]
    public float idleTurnSpeed = 0.8f;

    [Range(0f, 15f)]
    public float idleSway = 3f;


    // ==========================================
    // WAITING
    // ==========================================

    [Header("Waiting")]
    [Min(0f)]
    public float minWaitTime = 1f;

    [Min(0f)]
    public float maxWaitTime = 2f;


    // ==========================================
    // DEBUG
    // ==========================================

    [Header("Debug")]
    public bool showDebugRays = true;

    public bool showDebugLogs = true;


    // ==========================================
    // PRIVATE VARIABLES
    // ==========================================

    private Vector3 home;
    private Vector3 target;
    private Vector3 velocity;

    private float waitTimer;
    private float idleTimer;
    private float idlePhase;
    private float idleBaseYaw;

    private float bobPhase;
    private float nextGroundLogTime;

    private bool waiting;


    // ==========================================
    // START
    // ==========================================

    void Start()
    {
        home = transform.position;

        // Automatically find collider if not assigned.
        if (fishCollider == null)
        {
            fishCollider =
                GetComponentInChildren<Collider>();
        }

        if (fishCollider == null)
        {
            Debug.LogError(
                "[FishWander] No fish collider assigned!",
                this
            );

            enabled = false;
            return;
        }

        // Give each fish a different bobbing rhythm.
        bobPhase = Random.Range(
            0f,
            Mathf.PI * 2f
        );

        // Keep movement root upright.
        transform.rotation = Quaternion.Euler(
            0f,
            transform.eulerAngles.y,
            0f
        );

        PickTarget();

        Debug.Log(
            "[FishWander] Initialized: " + gameObject.name,
            this
        );
    }


    // ==========================================
    // UPDATE
    // ==========================================

    void Update()
    {
        float dt = Time.deltaTime;

        if (dt <= 0f)
            return;

        if (waiting)
        {
            // Gradually stop moving.
            velocity = Vector3.MoveTowards(
                velocity,
                Vector3.zero,
                acceleration * dt
            );

            IdleRotate(dt);

            waitTimer -= dt;

            if (waitTimer <= 0f)
            {
                waiting = false;
                PickTarget();
            }
        }
        else
        {
            MoveFish(dt);
        }

        // Always maintain ground clearance.
        ApplyMovement(dt);
    }


    // ==========================================
    // HORIZONTAL MOVEMENT
    // ==========================================

    void MoveFish(float dt)
    {
        Vector3 direction =
            target - transform.position;

        // Horizontal swimming only.
        direction.y = 0f;

        float distance = direction.magnitude;

        // Destination reached.
        if (distance <= arrivalDistance)
        {
            velocity = Vector3.MoveTowards(
                velocity,
                Vector3.zero,
                acceleration * dt
            );

            if (velocity.magnitude < 0.003f)
            {
                velocity = Vector3.zero;

                waiting = true;

                waitTimer = Random.Range(
                    minWaitTime,
                    Mathf.Max(
                        minWaitTime,
                        maxWaitTime
                    )
                );

                idleTimer = 0f;

                idlePhase = Random.Range(
                    0f,
                    Mathf.PI * 2f
                );

                idleBaseYaw =
                    transform.eulerAngles.y;
            }

            return;
        }

        // Slow down near destination.
        float speedFactor = Mathf.Clamp01(
            distance / slowdownDistance
        );

        Vector3 desiredVelocity =
            direction.normalized *
            maxSpeed *
            speedFactor;

        // Smooth acceleration.
        velocity = Vector3.MoveTowards(
            velocity,
            desiredVelocity,
            acceleration * dt
        );

        // Strict maximum speed.
        velocity = Vector3.ClampMagnitude(
            velocity,
            maxSpeed
        );

        // Rotate horizontally toward movement.
        Vector3 flatVelocity = velocity;

        flatVelocity.y = 0f;

        if (flatVelocity.sqrMagnitude > 0.00001f)
        {
            Quaternion desiredRotation =
                Quaternion.LookRotation(
                    flatVelocity,
                    Vector3.up
                );

            transform.rotation =
                Quaternion.RotateTowards(
                    transform.rotation,
                    desiredRotation,
                    turnSpeed * dt
                );
        }
    }


    // ==========================================
    // IDLE ROTATION
    // ==========================================

    void IdleRotate(float dt)
    {
        idleTimer += dt * idleTurnSpeed;

        float yaw =
            Mathf.Sin(idleTimer + idlePhase) *
            idleTurnAngle;

        float sway =
            Mathf.Sin(
                idleTimer * 0.7f + idlePhase
            ) * idleSway;

        Quaternion desiredRotation =
            Quaternion.Euler(
                0f,
                idleBaseYaw + yaw,
                sway
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                Mathf.Clamp01(dt * 2f)
            );
    }


    // ==========================================
    // GROUND FOLLOWING + VERTICAL SWIMMING
    // ==========================================

    void ApplyMovement(float dt)
    {
        // Calculate horizontal movement.
        Vector3 nextPosition =
            transform.position + velocity * dt;

        // Keep fish within wandering radius.
        Vector3 offset = nextPosition - home;

        offset.y = 0f;

        if (offset.magnitude > radius)
        {
            offset =
                offset.normalized * radius;

            nextPosition.x =
                home.x + offset.x;

            nextPosition.z =
                home.z + offset.z;

            velocity.x = 0f;
            velocity.z = 0f;
        }

        // Raycast from above the fish.
        Vector3 rayOrigin =
            nextPosition +
            Vector3.up * raycastHeight;

        float rayLength =
            raycastHeight + raycastDistance;

        // Debug ray.
        if (showDebugRays)
        {
            Debug.DrawRay(
                rayOrigin,
                Vector3.down * rayLength,
                Color.magenta
            );
        }

        // Detect terrain.
        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            rayLength,
            groundLayer,
            QueryTriggerInteraction.Ignore
        ))
        {
            // Calculate root-to-collider-bottom offset.
            float bottomOffset =
                transform.position.y -
                fishCollider.bounds.min.y;

            // Minimum safe root height.
            float groundHeight =
                hit.point.y +
                groundClearance +
                bottomOffset;

            // Independent vertical bobbing.
            float bob =
                Mathf.Sin(
                    Time.time * bobFrequency +
                    bobPhase
                ) * bobAmplitude;

            // Bob between minimum clearance
            // and twice the bob amplitude above it.
            float desiredHeight =
                groundHeight +
                bobAmplitude +
                bob;

            // Smoothly follow terrain.
            nextPosition.y = Mathf.MoveTowards(
                transform.position.y,
                desiredHeight,
                groundCorrectionSpeed * dt
            );

            // Debug clearance.
            if (showDebugRays)
            {
                Debug.DrawLine(
                    hit.point,
                    hit.point +
                    Vector3.up * groundClearance,
                    Color.green
                );
            }

            // Console diagnostics once per second.
            if (showDebugLogs &&
                Time.time >= nextGroundLogTime)
            {
                Debug.Log(
                    "[FishWander] Ground: " +
                    hit.collider.name +

                    " | Ground Y: " +
                    hit.point.y.ToString("F2") +

                    " | Bottom Offset: " +
                    bottomOffset.ToString("F2") +

                    " | Target Root Y: " +
                    desiredHeight.ToString("F2") +

                    " | Current Root Y: " +
                    transform.position.y.ToString("F2"),

                    this
                );

                nextGroundLogTime =
                    Time.time + 1f;
            }
        }
        else
        {
            // Do not randomly change height
            // when the ground cannot be detected.
            nextPosition.y =
                transform.position.y;

            if (showDebugLogs &&
                Time.time >= nextGroundLogTime)
            {
                Debug.LogWarning(
                    "[FishWander] No ground detected beneath " +
                    gameObject.name,

                    this
                );

                nextGroundLogTime =
                    Time.time + 1f;
            }
        }

        // Apply final position.
        transform.position = nextPosition;
    }


    // ==========================================
    // RANDOM DESTINATION
    // ==========================================

    void PickTarget()
    {
        Vector2 randomPoint =
            Random.insideUnitCircle *
            radius * 0.8f;

        target = new Vector3(
            home.x + randomPoint.x,
            home.y,
            home.z + randomPoint.y
        );
    }


    // ==========================================
    // EDITOR GIZMOS
    // ==========================================

    void OnDrawGizmosSelected()
    {
        Vector3 center =
            Application.isPlaying
            ? home
            : transform.position;

        // Wandering boundary.
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            center,
            radius
        );

        if (Application.isPlaying)
        {
            // Destination.
            Gizmos.color = Color.green;

            Gizmos.DrawSphere(
                target,
                0.08f
            );

            Gizmos.DrawLine(
                transform.position,
                target
            );
        }
    }
}

using System.Collections;
using UnityEngine;

public class FPSController : PortalTraveller
{
    [Header("Move")]
    public float walkSpeed = 3;
    public float runSpeed = 6;
    public float smoothMoveTime = 0.1f;
    public float jumpForce = 8;
    public float gravity = 18;

    [Header("Look")]
    public bool lockCursor;
    public float mouseSensitivity = 10;
    public Vector2 pitchMinMax = new Vector2(-40, 85);
    public float rotationSmoothTime = 0.1f;

    // Components
    CharacterController controller;
    Camera cam;

    // Rotation
    public float yaw;
    public float pitch;
    float smoothYaw;
    float smoothPitch;
    float yawSmoothV;
    float pitchSmoothV;

    // Movement
    float verticalVelocity;
    Vector3 velocity;
    Vector3 smoothV;

    // Jump
    bool jumping;
    float lastGroundedTime;

    // Puzzle control
    bool disabled;

    void Start()
    {
        cam = Camera.main;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        controller = GetComponent<CharacterController>();

        yaw = transform.eulerAngles.y;
        pitch = cam.transform.localEulerAngles.x;
        smoothYaw = yaw;
        smoothPitch = pitch;
    }

    void Update()
    {
        // Cursor debug
        if (Input.GetKeyDown(KeyCode.U))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // disabled = !disabled;
        }

        // ESC: exit puzzle (restore control)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (disabled)
            {
                disabled = false;
            }
        }

        // If disabled, stop all input & movement
        if (disabled)
            return;

        // ======================
        // Movement
        // ======================

        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        Vector3 inputDir = new Vector3(input.x, 0, input.y).normalized;
        Vector3 worldInputDir = transform.TransformDirection(inputDir);

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        Vector3 targetVelocity = worldInputDir * currentSpeed;
        velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref smoothV, smoothMoveTime);

        // Gravity
        verticalVelocity -= gravity * Time.deltaTime;
        velocity = new Vector3(velocity.x, verticalVelocity, velocity.z);

        CollisionFlags flags = controller.Move(velocity * Time.deltaTime);
        if (flags == CollisionFlags.Below)
        {
            jumping = false;
            lastGroundedTime = Time.time;
            verticalVelocity = 0;
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float timeSinceGrounded = Time.time - lastGroundedTime;
            if (controller.isGrounded || (!jumping && timeSinceGrounded < 0.15f))
            {
                jumping = true;
                verticalVelocity = jumpForce;
            }
        }

        // ======================
        // Look
        // ======================

        float mX = Input.GetAxisRaw("Mouse X");
        float mY = Input.GetAxisRaw("Mouse Y");

        // Gross hack to prevent camera snap at start
        float mMag = Mathf.Sqrt(mX * mX + mY * mY);
        if (mMag > 5)
        {
            mX = 0;
            mY = 0;
        }

        yaw += mX * mouseSensitivity;
        pitch -= mY * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        smoothYaw = Mathf.SmoothDampAngle(smoothYaw, yaw, ref yawSmoothV, rotationSmoothTime);
        smoothPitch = Mathf.SmoothDampAngle(smoothPitch, pitch, ref pitchSmoothV, rotationSmoothTime);

        transform.eulerAngles = Vector3.up * smoothYaw;
        cam.transform.localEulerAngles = Vector3.right * smoothPitch;
    }

    // ======================
    // Portal
    // ======================

    public override void Teleport(
        Transform fromPortal,
        Transform toPortal,
        Vector3 pos,
        Quaternion rot
    )
    {
        transform.position = pos;

        Vector3 eulerRot = rot.eulerAngles;
        float delta = Mathf.DeltaAngle(smoothYaw, eulerRot.y);

        yaw += delta;
        smoothYaw += delta;
        transform.eulerAngles = Vector3.up * smoothYaw;

        velocity = toPortal.TransformVector(
            fromPortal.InverseTransformVector(velocity)
        );

        Physics.SyncTransforms();
    }

    // ======================
    // Puzzle Trigger
    // ======================

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Stop_Area Collider")
        {
            StartCoroutine(DisablePlayerIE());
        }
    }

    IEnumerator DisablePlayerIE()
    {
        yield return new WaitForSeconds(1f);
        disabled = true;

        // Start hospital puzzle
        Puzzle_Hos_Handler.instance.RestartSemangReadingTiming();
    }

    // Optional public hook (used by puzzle complete)
    public void EnablePlayer()
    {
        disabled = false;
    }
}

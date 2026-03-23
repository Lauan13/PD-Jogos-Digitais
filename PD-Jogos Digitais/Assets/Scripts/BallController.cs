using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    // Force applied to move the ball (units: meters/second^2 when using ForceMode.Acceleration)
    [SerializeField] float moveForce = 10f;
    // Maximum horizontal speed (m/s)
    [SerializeField] float maxSpeed = 5f;
    // Optional reference transform to make input camera-relative. If null, world axes are used.
    [SerializeField] Transform referenceTransform = null;

    Rigidbody rb;
    InputSystem_Actions controls;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError("BallController requires a Rigidbody on the same GameObject.");

        // Create input actions generated from the Input Actions asset
        controls = new InputSystem_Actions();
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        Vector2 moveInput = controls.Player.Move.ReadValue<Vector2>();

        // Build movement vector. Input: x => right, y => forward
        Vector3 moveDir;
        if (referenceTransform != null)
        {
            // Project reference forward/right onto the XZ plane so vertical tilt doesn't affect movement
            Vector3 forward = referenceTransform.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = referenceTransform.right;
            right.y = 0f;
            right.Normalize();
            moveDir = right * moveInput.x + forward * moveInput.y;
        }
        else
        {
            moveDir = new Vector3(moveInput.x, 0f, moveInput.y);
        }

        // Apply force for movement. Using ForceMode.Acceleration produces consistent acceleration regardless of mass.
        if (moveDir.sqrMagnitude > 0f)
        {
            rb.AddForce(moveDir.normalized * moveForce, ForceMode.Acceleration);
        }

        // Limit horizontal velocity to maxSpeed while preserving vertical velocity (e.g. falling)
        Vector3 horizontalVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (horizontalVel.magnitude > maxSpeed)
        {
            Vector3 limited = horizontalVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limited.x, rb.velocity.y, limited.z);
        }
    }

    // Public setters to allow changing parameters at runtime
    public void SetMoveForce(float value) => moveForce = value;
    public void SetMaxSpeed(float value) => maxSpeed = value;
}


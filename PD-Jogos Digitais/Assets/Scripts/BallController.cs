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
    [SerializeField] Transform referenceTransform;

    Rigidbody _rb;
    // Use an InputActionReference so the action can be assigned from the Input Actions asset in the inspector.
    [SerializeField] InputActionReference moveAction;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
            Debug.LogError("BallController requires a Rigidbody on the same GameObject.");

        if (moveAction == null || moveAction.action == null)
        {
            Debug.LogWarning("BallController: no Move action assigned to `moveAction`. Assign an action from your Input Actions asset in the inspector.");
        }
    }

    void OnEnable()
    {
        // Enable the assigned action (if any)
        moveAction?.action?.Enable();
    }

    void OnDisable()
    {
        moveAction?.action?.Disable();
    }

    void FixedUpdate()
    {
        if (_rb == null) return;

        // Read move input from the assigned action. Fallback to zero if not assigned.
        Vector2 moveInput = Vector2.zero;
        if (moveAction != null && moveAction.action != null)
            moveInput = moveAction.action.ReadValue<Vector2>();

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
            _rb.AddForce(moveDir.normalized * moveForce, ForceMode.Acceleration);
        }

        // Limit horizontal velocity to maxSpeed while preserving vertical velocity (e.g. falling)
        Vector3 linearVel = _rb.linearVelocity;
        Vector3 horizontalVel = new Vector3(linearVel.x, 0f, linearVel.z);
        if (horizontalVel.magnitude > maxSpeed)
        {
            Vector3 limited = horizontalVel.normalized * maxSpeed;
            _rb.linearVelocity = new Vector3(limited.x, _rb.linearVelocity.y, limited.z);
        }
    }

    // Public setters to allow changing parameters at runtime
    public void SetMoveForce(float value) => moveForce = value;
    public void SetMaxSpeed(float value) => maxSpeed = value;
}


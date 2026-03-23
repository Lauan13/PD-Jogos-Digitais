using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador simples para um jogo roll-a-ball usando o novo Input System.
/// Como usar:
/// - Anexe este componente a um GameObject que tenha Rigidbody (por exemplo, a bola).
/// - No Inspector arraste a Action "Move" do seu asset InputSystem_Actions.inputactions para o campo Move Action
///   (use um Input Action Reference apontando para a ação do tipo Value / Vector2).
/// - Ajuste Move Force e Max Speed conforme necessário. Recomenda-se usar Freeze Rotation no Rigidbody
///   se quiser evitar que a bola gire em eixos inesperados.
/// Comportamento:
/// - Lê um Vector2 (x = esquerda/direita, y = frente/trás) e aplica AddForce no Rigidbody em FixedUpdate
///   usando ForceMode.Acceleration; a velocidade no plano XZ é limitada por MaxSpeed.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class RollABallController : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Referência à Action 'Move' (Vector2) do Input System. Arraste a ação do asset aqui.")]
    [SerializeField] private InputActionReference moveAction = null;

    [Header("Movement")]
    [Tooltip("Força aplicada como aceleração para movimentar a bola.")]
    [SerializeField] private float moveForce = 10f;

    [Tooltip("Velocidade máxima no plano XZ (em unidades por segundo).")]
    [SerializeField] private float maxSpeed = 6f;

    private Rigidbody rb;
    private Vector2 moveInput = Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("RollABallController requer um Rigidbody.");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (moveAction == null || moveAction.action == null)
        {
            Debug.LogWarning("Move Action não atribuída ou inválida. Desabilitando controlador.");
            enabled = false;
            return;
        }

        // Subscribes
        moveAction.action.performed += OnMovePerformed;
        moveAction.action.canceled += OnMoveCanceled;

        // Ensure action is enabled
        moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null && moveAction.action != null)
        {
            // Unsubscribe
            moveAction.action.performed -= OnMovePerformed;
            moveAction.action.canceled -= OnMoveCanceled;
            moveAction.action.Disable();
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        // Quando a ação é cancelada, zera a entrada
        moveInput = Vector2.zero;
    }

    private void FixedUpdate()
    {
        // Converter Vector2 (x, y) para movimento no plano XZ
        Vector3 force = new Vector3(moveInput.x, 0f, moveInput.y) * moveForce;

        // Aplica força como aceleração para comportamento físico mais natural
        rb.AddForce(force, ForceMode.Acceleration);

        // Limita a velocidade apenas no plano XZ, preservando a componente Y (gravidade)
        Vector3 vel = rb.linearVelocity;
        Vector3 planar = new Vector3(vel.x, 0f, vel.z);
        Vector3 clampedPlanar = Vector3.ClampMagnitude(planar, maxSpeed);
        rb.linearVelocity = new Vector3(clampedPlanar.x, vel.y, clampedPlanar.z);
    }

    // Opcional: expõe método público para ajustar a sensibilidade em runtime
    public void SetMoveForce(float force)
    {
        moveForce = Mathf.Max(0f, force);
    }
}


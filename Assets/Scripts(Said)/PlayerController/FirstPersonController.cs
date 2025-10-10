using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;
    private bool isCrouching = false;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool lockCursor = true;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference runAction;
    [SerializeField] private InputActionReference crouchAction;

    // --- NUEVO: Sistema de Vida ---
    [Header("Health System")]
    [Tooltip("Activa para que el jugador pueda recibir daño y morir.")]
    [SerializeField] private bool healthSystemEnabled = true;
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    // Referencia al GameManager para notificar la muerte
    [SerializeField] private GameManager gameManager;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private bool isGrounded;
    private float xRotation = 0f;
    private Vector2 currentMovementInput;
    private bool isRunning;
    private bool isInputEnabled = true;

    // Propiedad pública para saber si está corriendo
    public bool IsRunning => isRunning;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider == null)
        {
            Debug.LogError("No se encontró un CapsuleCollider en el jugador.");
        }
        if (gameManager == null)
        {
            // Intenta encontrar el GameManager en la escena si no está asignado
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("No se encontró el GameManager en la escena. Es necesario para manejar la muerte del jugador.");
            }
        }
        LockCursor();
    }

    void Start()
    {
        // Inicializa la vida y la corrutina de daño
        if (healthSystemEnabled)
        {
            currentHealth = maxHealth;
            StartCoroutine(CheckRunningAndApplyDamage());
        }
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
        runAction.action.Enable();
        crouchAction.action.Enable();

        moveAction.action.performed += OnMovement;
        moveAction.action.canceled += OnMovement;
        jumpAction.action.performed += OnJump;
        runAction.action.performed += OnRun;
        runAction.action.canceled += OnRun;
        crouchAction.action.performed += OnCrouch;
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
        runAction.action.Disable();
        crouchAction.action.Disable();

        moveAction.action.performed -= OnMovement;
        moveAction.action.canceled -= OnMovement;
        jumpAction.action.performed -= OnJump;
        runAction.action.performed -= OnRun;
        runAction.action.canceled -= OnRun;
        crouchAction.action.performed -= OnCrouch;
    }

    void Update()
    {
        if (isInputEnabled)
        {
            HandleMouseLook();
        }
    }

    void FixedUpdate()
    {
        if (isInputEnabled)
        {
            HandleMovement();
        }
        CheckGrounded();
    }

    // --- Lógica de vida movida aquí ---
    private IEnumerator CheckRunningAndApplyDamage()
    {
        // Se ejecuta mientras el sistema esté activo y el jugador tenga vida
        while (healthSystemEnabled && currentHealth > 0)
        {
            yield return new WaitForSeconds(1.5f); // Intervalo de chequeo

            if (IsRunning)
            {
                // 25% de probabilidad de recibir daño
                if (Random.value < 0.50f)
                {
                    TakeDamage(1);
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (!healthSystemEnabled) return;

        currentHealth -= damage;
        Debug.Log($"El jugador recibió {damage} de daño. Vida restante: {currentHealth}");

        // Aquí podrías invocar un evento para actualizar la UI de vida si la tuvieras

        if (currentHealth <= 0)
        {
            Debug.Log("El jugador se ha quedado sin vida.");
            if (gameManager != null)
            {
                gameManager.HandlePlayerDeath(); // Notifica al GameManager
            }
        }
    }


    private void LockCursor()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void OnMovement(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && !isCrouching)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.performed && !isCrouching;
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        isCrouching = !isCrouching;
        if (capsuleCollider != null)
        {
            capsuleCollider.height = isCrouching ? crouchHeight : standingHeight;
            if (isCrouching)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }
    }

    private void HandleMouseLook()
    {
        Vector2 lookDelta = lookAction.action.ReadValue<Vector2>();
        float mouseX = lookDelta.x * mouseSensitivity;
        float mouseY = lookDelta.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        if (isCrouching) return;

        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 direction = (transform.forward * currentMovementInput.y + transform.right * currentMovementInput.x).normalized;
        Vector3 targetVelocity = new Vector3(direction.x * currentSpeed, 0, direction.z * currentSpeed);
        Vector3 velocityChange = (targetVelocity - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z));
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    public void SetInputEnabled(bool enabled)
    {
        isInputEnabled = enabled;

        if (!enabled)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            currentMovementInput = Vector2.zero;

            moveAction.action.Disable();
            lookAction.action.Disable();
            jumpAction.action.Disable();
            runAction.action.Disable();
            crouchAction.action.Disable();
        }
        else
        {
            LockCursor();
            moveAction.action.Enable();
            lookAction.action.Enable();
            jumpAction.action.Enable();
            runAction.action.Enable();
            crouchAction.action.Enable();
        }
    }
}
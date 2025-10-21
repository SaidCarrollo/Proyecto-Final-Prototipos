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

    [Header("Health System")]
    [Tooltip("Activa para que el jugador pueda recibir daño y morir.")]
    [SerializeField] private bool healthSystemEnabled = true;
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    [SerializeField] private GameManager gameManager;

    // --- NUEVAS REFERENCIAS A MANAGERS ---
    [Header("System References")]
    [Tooltip("Referencia al UIManager para mostrar mensajes en pantalla.")]
    [SerializeField] private UIManager uiManager;
    [Tooltip("Referencia al BadgeManager para desbloquear logros/errores.")]
    [SerializeField] private BadgeManager badgeManager;
    [Tooltip("Referencia al VignetteController para efectos visuales de daño.")]
    [SerializeField] private VignetteController vignetteController;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private bool isGrounded;
    private float xRotation = 0f;
    private Vector2 currentMovementInput;
    private bool isRunning;
    private bool isInputEnabled = true;
    [Header("Injury Cojo")]
    [SerializeField] private float injuredSpeedMultiplier = 0.6f;
    private bool isInjured = false;
    private bool canRun = true;
    private float baseWalkSpeed, baseRunSpeed;
    public bool IsRunning => isRunning;
    public bool IsCrouching => isCrouching;
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
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("No se encontró el GameManager en la escena. Es necesario para manejar la muerte del jugador.");
            }
        }

        //// --- BÚSQUEDA AUTOMÁTICA DE REFERENCIAS (OPCIONAL PERO RECOMENDADO) ---
        //if (uiManager == null)
        //{
        //    uiManager = FindObjectOfType<UIManager>();
        //}
        //if (vignetteController == null)
        //{
        //    vignetteController = FindObjectOfType<VignetteController>();
        //}
        //// BadgeManager es un ScriptableObject, usualmente se asigna manualmente.

        LockCursor();
    }

    void Start()
    {
        if (healthSystemEnabled)
        {
            currentHealth = maxHealth;
            StartCoroutine(CheckRunningAndApplyDamage());
        }
        baseWalkSpeed = walkSpeed;
        baseRunSpeed = runSpeed;
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

    private IEnumerator CheckRunningAndApplyDamage()
    {
        while (healthSystemEnabled && currentHealth > 0)
        {
            yield return new WaitForSeconds(1.5f);

            if (IsRunning)
            {
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

        // --- ¡AQUÍ ESTÁ LA NUEVA LÓGICA! ---

        // 1. Mostrar mensaje "aunch" en la UI
        if (uiManager != null)
        {
            uiManager.OnMessageEventRaised("aunch");
        }
        else
        {
            Debug.LogWarning("Referencia a UIManager no asignada en FirstPersonController.");
        }

        // 2. Activar viñeta roja
        if (vignetteController != null)
        {
            // Parámetros: Color, Intensidad (0 a 1), Duración del efecto antes de desvanecerse
            vignetteController.TriggerVignette(Color.red, 0.4f, 0.5f);
        }
        else
        {
            Debug.LogWarning("Referencia a VignetteController no asignada en FirstPersonController.");
        }

        // 3. Desbloquear un badge de tipo "Incorrecto"
        if (badgeManager != null)
        {
            // IMPORTANTE: Debes crear un badge con este ID en tu ScriptableObject BadgeManager
            badgeManager.UnlockBadge("auch");
        }
        else
        {
            Debug.LogWarning("Referencia a BadgeManager no asignada en FirstPersonController.");
        }


        if (currentHealth <= 0)
        {
            Debug.Log("El jugador se ha quedado sin vida.");
            if (gameManager != null)
            {
                gameManager.HandlePlayerDeath();
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
        isRunning = context.performed && !isCrouching && canRun;
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
    public void ApplyPermanentInjury(string uiMessageOverride, float slowMultiplier)
    {
        if (isInjured) return;

        isInjured = true;
        canRun = false;

        // Reduce caminar y equipara el correr al caminar (correr ya no acelera)
        injuredSpeedMultiplier = Mathf.Clamp(slowMultiplier, 0.1f, 1f);
        walkSpeed = Mathf.Max(0.1f, baseWalkSpeed * injuredSpeedMultiplier);
        runSpeed = walkSpeed;

        // Cortar carrera si estaba corriendo
        isRunning = false;

        // Frenar un poco el XZ para que se note
        rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y, rb.linearVelocity.z * 0.5f);

        // Mensaje claro en UI
        if (!string.IsNullOrEmpty(uiMessageOverride) && uiManager != null)
        {
            uiManager.OnMessageEventRaised(uiMessageOverride); // Muestra en pantalla. :contentReference[oaicite:6]{index=6}
        }
    }

}
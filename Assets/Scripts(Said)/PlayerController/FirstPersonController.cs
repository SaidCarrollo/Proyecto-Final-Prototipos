using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.EventSystems;

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
    [Tooltip("Sensibilidad para mouse / pointer (PC)")]
    [SerializeField] private float mouseSensitivity = 100f;
    [Tooltip("Sensibilidad para el segundo joystick (móvil)")]
    [SerializeField] private float stickSensitivity = 180f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool lockCursor = true;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;       // PC / mouse / pointer
    [SerializeField] private InputActionReference lookStickAction;  // NUEVO: joystick derecho móvil
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference runAction;
    [SerializeField] private InputActionReference crouchAction;

    [Header("Health System")]
    [SerializeField] private bool healthSystemEnabled = true;
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    [SerializeField] private GameManager gameManager;

    [Header("System References")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private BadgeManager badgeManager;
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

    private bool hasEverRun = false;
    private bool windowInjuryOccurred = false;

    public bool IsRunning => isRunning;
    public bool IsCrouching => isCrouching;
    public bool HasEverRun => hasEverRun;
    public bool WindowInjuryOccurred => windowInjuryOccurred;

    // ====== entrada externa (si más adelante la quieres usar) ======
    public void AddLookInput(Vector2 lookDelta)
    {
        if (!isInputEnabled) return;
        // este método espera el delta YA escalado
        ApplyLook(lookDelta);
    }

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
            Debug.LogError("No se encontró el GameManager en la escena. Es necesario para manejar la muerte del jugador.");
        }

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
        if (lookStickAction != null) lookStickAction.action.Enable();
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
        moveAction.action.performed -= OnMovement;
        moveAction.action.canceled -= OnMovement;
        jumpAction.action.performed -= OnJump;
        runAction.action.performed -= OnRun;
        runAction.action.canceled -= OnRun;
        crouchAction.action.performed -= OnCrouch;

        moveAction.action.Disable();
        lookAction.action.Disable();
        if (lookStickAction != null) lookStickAction.action.Disable();
        jumpAction.action.Disable();
        runAction.action.Disable();
        crouchAction.action.Disable();
    }

    void Update()
    {
        //if (!isInputEnabled) return;

        //// 1) Look de PC / mouse / pointer
        //Vector2 finalLook = Vector2.zero;
        //if (lookAction != null)
        //{
        //    // pointer/mouse suele venir en "pixels por frame", por eso usamos Time.deltaTime
        //    Vector2 lookDelta = lookAction.action.ReadValue<Vector2>();
        //    finalLook += lookDelta * mouseSensitivity * Time.deltaTime;
        //}

        //// 2) Look de segundo joystick (móvil)
        //if (lookStickAction != null)
        //{
        //    // el stick viene en -1..1, así que lo escalamos a grados/segundo
        //    Vector2 stickDelta = lookStickAction.action.ReadValue<Vector2>();
        //    finalLook += stickDelta * stickSensitivity * Time.deltaTime;
        //}

        //// 3) Aplicar si hay algo
        //if (finalLook.sqrMagnitude > 0.0001f)
        //{
        //    ApplyLook(finalLook);
        //}
        if (!isInputEnabled) return;

        // si hay una UI debajo del dedo/click, no gires
        if (IsPointerOverUI())
            return;

        Vector2 lookDelta = lookAction.action.ReadValue<Vector2>();
        if (lookDelta.sqrMagnitude > 0.0001f)
        {
            ApplyLook(lookDelta * mouseSensitivity * Time.deltaTime);
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

        if (uiManager != null)
            uiManager.OnMessageEventRaised("aunch");

        if (vignetteController != null)
            vignetteController.TriggerVignette(Color.red, 0.4f, 0.5f);

        if (badgeManager != null)
            badgeManager.UnlockBadge("auch");

        if (currentHealth <= 0)
        {
            if (gameManager != null)
            {
                gameManager.HandlePlayerDeath();
            }
        }
    }

    private void LockCursor()
    {
        // solo bloquee si hay mouse (en móvil no)
        if (lockCursor && Mouse.current != null)
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
        if (isRunning) hasEverRun = true;
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

    // ========= lógica real de mirar =========
    private void ApplyLook(Vector2 deltaDegrees)
    {
        // deltaDegrees.X = rotación en yaw
        // deltaDegrees.Y = rotación en pitch (invertimos)
        xRotation -= deltaDegrees.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * deltaDegrees.x);
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
            if (lookStickAction != null) lookStickAction.action.Disable();
            jumpAction.action.Disable();
            runAction.action.Disable();
            crouchAction.action.Disable();
        }
        else
        {
            LockCursor();
            moveAction.action.Enable();
            lookAction.action.Enable();
            if (lookStickAction != null) lookStickAction.action.Enable();
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

        injuredSpeedMultiplier = Mathf.Clamp(slowMultiplier, 0.1f, 1f);
        walkSpeed = Mathf.Max(0.1f, baseWalkSpeed * injuredSpeedMultiplier);
        runSpeed = walkSpeed;

        isRunning = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y, rb.linearVelocity.z * 0.5f);

        if (!string.IsNullOrEmpty(uiMessageOverride) && uiManager != null)
        {
            uiManager.OnMessageEventRaised(uiMessageOverride);
        }
        var tilt = GetComponentInChildren<CameraInjuryTilt>();
        if (tilt != null) tilt.EnableInjuryTilt(true);
    }

    public void MarkWindowInjury()
    {
        windowInjuryOccurred = true;
    }
    private bool IsPointerOverUI()
    {
        // PC / mouse
        if (Mouse.current != null && Mouse.current.press.isPressed)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return true;
        }

        // Mobile / touch (revisar cada dedo)
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (!touch.press.isPressed) continue;
                int id = touch.touchId.ReadValue();
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(id))
                    return true;
            }
        }

        return false;
    }
}


using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool lockCursor = true;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference runAction;
    [SerializeField] private Joystick dynamicJoystick;
    [SerializeField] private TouchLookInput touchLookInput;

    private Rigidbody rb;
    private bool isGrounded;
    private float xRotation = 0f;
    private Vector2 currentMovementInput;
    private bool isRunning;
    private bool movementEnabled = true;
    private bool isInputEnabled = true;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        LockCursor();
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
        runAction.action.Enable();

        moveAction.action.performed += OnMovement;
        moveAction.action.canceled += OnMovement;
        jumpAction.action.performed += OnJump;
        runAction.action.performed += OnRun;
        runAction.action.canceled += OnRun;
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
        runAction.action.Disable();

        moveAction.action.performed -= OnMovement;
        moveAction.action.canceled -= OnMovement;
        jumpAction.action.performed -= OnJump;
        runAction.action.performed -= OnRun;
        runAction.action.canceled -= OnRun;
    }

    void Update()
    {

        if (isInputEnabled)
        {
            HandleMouseLook();
        }
       // HandleMouseLook();
    }

    void FixedUpdate()
    {
        if (isInputEnabled)
        {
            HandleMovement();
        }
        CheckGrounded();
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
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.performed;
    }

    private void HandleMouseLook()
    {
        Vector2 lookDelta;

#if UNITY_ANDROID
        lookDelta = touchLookInput != null ? touchLookInput.LookDelta : Vector2.zero;
        float sensitivity = mouseSensitivity * Time.deltaTime * 20f; 
#else
    lookDelta = lookAction.action.ReadValue<Vector2>();
    float sensitivity = mouseSensitivity * Time.deltaTime;
#endif

        float mouseX = lookDelta.x * sensitivity;
        float mouseY = lookDelta.y * sensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }


    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        if (!enabled)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            currentMovementInput = Vector2.zero;
        }
    }
    private void HandleMovement()
    {
        Vector2 inputVector = currentMovementInput;

#if UNITY_ANDROID
        if (dynamicJoystick != null)
        {
            inputVector = dynamicJoystick.Direction;
        }
#endif

        Vector3 direction = (transform.forward * inputVector.y + transform.right * inputVector.x).normalized;

        float speed = isRunning ? runSpeed : walkSpeed;

        rb.linearVelocity = new Vector3(
            direction.x * speed,
            rb.linearVelocity.y,
            direction.z * speed
        );
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance,
            groundLayer
        );
    }
    public void SetInputEnabled(bool enabled)
    {
        isInputEnabled = enabled;

        if (!enabled)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            currentMovementInput = Vector2.zero;
        }
    }
}
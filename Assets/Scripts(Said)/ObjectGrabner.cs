using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectGrabber : MonoBehaviour
{
    public event Action<GameObject> OnObjectGrabbed;
    public event Action<GameObject> OnObjectReleased;

    [Header("Grab Settings")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("El punto base donde descansan los objetos. Representará el 25% del Slider.")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float grabRange = 5f;
    [SerializeField] private LayerMask interactableLayer = ~0;

    [Header("Suavizado (Smoothness)")]
    [Tooltip("Tiempo en segundos que tarda el objeto en seguir la cámara. 0.05 a 0.1 es ideal.")]
    [SerializeField] private float followSmoothTime = 0.05f;
    [Tooltip("Velocidad a la que se acerca/aleja cuando usas el slider.")]
    [SerializeField] private float zoomSpeed = 10f;

    [Header("Límites del Slider (Offsets)")]
    [SerializeField] private float minHoldOffset = -0.5f;
    [SerializeField] private float maxHoldOffset = 1.5f;

    [Header("Input Actions (opcional)")]
    [SerializeField] private InputActionReference adjustHoldAction;

    [Header("SFX (opcional)")]
    [SerializeField] private AudioSource grabAudio;
    [SerializeField] private AudioSource releaseAudio;

    private GameObject heldObject;
    private Rigidbody heldObjectRb;

    // Variables para el suavizado
    private float targetHoldOffset = 0f;
    private float currentHoldOffset = 0f;
    private Vector3 velocitySmoothDamp;

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        if (adjustHoldAction != null)
        {
            adjustHoldAction.action.Enable();
            adjustHoldAction.action.performed += OnAdjustHoldPerformed;
        }
    }

    private void OnDisable()
    {
        if (adjustHoldAction != null)
        {
            adjustHoldAction.action.performed -= OnAdjustHoldPerformed;
            adjustHoldAction.action.Disable();
        }
    }

    // --- CAMBIO CLAVE 1: Pasamos de FixedUpdate a LateUpdate ---
    // LateUpdate ocurre justo después de que la cámara principal se ha movido, garantizando cero temblores.
    private void LateUpdate()
    {
        MoveHeldObjectSmoothly();
    }

    public bool IsHoldingObject() => heldObject != null;

    public void TryGrab()
    {
        if (cameraTransform == null) return;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, grabRange, interactableLayer, QueryTriggerInteraction.Ignore))
        {
            var go = hit.collider.attachedRigidbody ? hit.collider.attachedRigidbody.gameObject : hit.collider.gameObject;

            heldObject = go;
            heldObjectRb = go.GetComponent<Rigidbody>();

            targetHoldOffset = 0f;
            currentHoldOffset = 0f;
            velocitySmoothDamp = Vector3.zero;

            if (heldObjectRb != null)
            {
                heldObjectRb.useGravity = false;
                heldObjectRb.freezeRotation = true;

                // --- CAMBIO CLAVE 2: Desactivamos la física mientras lo sostenemos ---
                heldObjectRb.isKinematic = true;
                heldObjectRb.linearVelocity = Vector3.zero;
            }

            grabAudio?.Play();
            OnObjectGrabbed?.Invoke(heldObject);
        }
    }

    public void Release()
    {
        if (heldObjectRb != null)
        {
            heldObjectRb.useGravity = true;
            heldObjectRb.freezeRotation = false;

            // --- CAMBIO CLAVE 3: Devolvemos la física al soltar ---
            heldObjectRb.isKinematic = false;
        }

        var released = heldObject;
        heldObject = null;
        heldObjectRb = null;

        releaseAudio?.Play();
        OnObjectReleased?.Invoke(released);
    }

    public void ToggleGrabRelease()
    {
        if (IsHoldingObject()) Release();
        else TryGrab();
    }

    private void OnAdjustHoldPerformed(InputAction.CallbackContext ctx)
    {
        float delta = ctx.ReadValue<float>();
        targetHoldOffset = Mathf.Clamp(targetHoldOffset + delta, minHoldOffset, maxHoldOffset);
    }

    public void NudgeHoldDistance(float delta)
    {
        targetHoldOffset = Mathf.Clamp(targetHoldOffset + delta, minHoldOffset, maxHoldOffset);
    }

    private void MoveHeldObjectSmoothly()
    {
        if (heldObject == null || cameraTransform == null) return;

        // 1. Suavizar el acercamiento/alejamiento (Zoom)
        // Usamos Time.deltaTime porque estamos en LateUpdate
        currentHoldOffset = Mathf.Lerp(currentHoldOffset, targetHoldOffset, Time.deltaTime * zoomSpeed);

        // 2. Calcular la posición objetivo ideal
        Vector3 forwardHorizontal = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
        Vector3 targetPos;

        if (holdPoint != null)
        {
            targetPos = holdPoint.position + (forwardHorizontal * currentHoldOffset);
        }
        else
        {
            targetPos = cameraTransform.position + forwardHorizontal * (1.5f + currentHoldOffset);
        }

        // 3. Ajuste de velocidad para NavMesh / Carreras
        float dist = Vector3.Distance(heldObject.transform.position, targetPos);
        float currentSmoothTime = dist > 1.0f ? 0.02f : followSmoothTime;

        // --- CAMBIO CLAVE 4: Movemos el Transform directamente, sin pelear con el Rigidbody ---
        heldObject.transform.position = Vector3.SmoothDamp(
            heldObject.transform.position,
            targetPos,
            ref velocitySmoothDamp,
            currentSmoothTime,
            Mathf.Infinity,
            Time.deltaTime
        );
    }

    // --- API PÚBLICA PARA EL SLIDER ---
    public float GetMinHoldDistance() => minHoldOffset;
    public float GetMaxHoldDistance() => maxHoldOffset;
    public float GetCurrentHoldDistance() => targetHoldOffset;

    public void SetHoldDistance(float nuevaDistancia)
    {
        targetHoldOffset = Mathf.Clamp(nuevaDistancia, minHoldOffset, maxHoldOffset);
    }
}
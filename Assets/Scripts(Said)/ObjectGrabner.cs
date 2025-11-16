using System;
using UnityEngine;
using UnityEngine.InputSystem; // sólo para adjustHoldAction (si quieres)

public class ObjectGrabber : MonoBehaviour
{
    // ---- Eventos iguales que antes ----
    public event Action<GameObject> OnObjectGrabbed;
    public event Action<GameObject> OnObjectReleased;

    [Header("Grab Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float grabRange = 5f;
    [SerializeField] private float grabSpeed = 15f;
    [SerializeField] private float verticalOffset = 0.1f;
    [SerializeField] private LayerMask interactableLayer = ~0;
    [SerializeField] private float minHoldDistance = 0.5f;
    [SerializeField] private float maxHoldDistance = 1.8f;
    [SerializeField] private float startHoldDistance = 1.5f;

    [Header("Input Actions (opcional)")]
    [Tooltip("Eje/slider/pinch que entrega +/− para acercar/alejar el objeto")]
    [SerializeField] private InputActionReference adjustHoldAction;

    [Header("SFX (opcional)")]
    [SerializeField] private AudioSource grabAudio;
    [SerializeField] private AudioSource releaseAudio;

    // ---- Estado interno ----
    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private float currentHoldDistance;

    private void Awake()
    {
        if (cameraTransform == null)
        {
            var cam = Camera.main;
            if (cam != null) cameraTransform = cam.transform;
        }
        currentHoldDistance = Mathf.Clamp(startHoldDistance, minHoldDistance, maxHoldDistance);
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

    private void FixedUpdate()
    {
        MoveHeldWithPhysics();
    }

    // ========= Métodos llamados desde fuera =========

    public bool IsHoldingObject() => heldObject != null;

    /// <summary>Intentar agarrar el objeto que esté delante de la cámara.</summary>
    public void TryGrab()
    {
        if (cameraTransform == null) return;

        if (Physics.Raycast(cameraTransform.position,
                            cameraTransform.forward,
                            out var hit,
                            grabRange,
                            interactableLayer,
                            QueryTriggerInteraction.Ignore))
        {
            var go = hit.collider.attachedRigidbody
                     ? hit.collider.attachedRigidbody.gameObject
                     : hit.collider.gameObject;

            heldObject = go;
            heldObjectRb = go.GetComponent<Rigidbody>();

            if (heldObjectRb != null)
            {
                heldObjectRb.useGravity = false;
                heldObjectRb.freezeRotation = true;
                heldObjectRb.linearVelocity = Vector3.zero;
            }

            grabAudio?.Play();
            OnObjectGrabbed?.Invoke(heldObject);
        }
    }

    /// <summary>Soltar el objeto actualmente agarrado.</summary>
    public void Release()
    {
        if (heldObjectRb != null)
        {
            heldObjectRb.useGravity = true;
            heldObjectRb.freezeRotation = false;
        }

        var released = heldObject;
        heldObject = null;
        heldObjectRb = null;

        releaseAudio?.Play();
        OnObjectReleased?.Invoke(released);
    }

    // Por si quieres usarlo en otros contextos
    public void ToggleGrabRelease()
    {
        if (IsHoldingObject()) Release();
        else TryGrab();
    }

    // ========= Input opcional para acercar/alejar =========
    private void OnAdjustHoldPerformed(InputAction.CallbackContext ctx)
    {
        float delta = ctx.ReadValue<float>();
        currentHoldDistance = Mathf.Clamp(currentHoldDistance + delta, minHoldDistance, maxHoldDistance);
    }

    // API pública para botones + / –
    public void NudgeHoldDistance(float delta)
    {
        currentHoldDistance = Mathf.Clamp(currentHoldDistance + delta, minHoldDistance, maxHoldDistance);
    }

    // ========= Seguimiento del objeto =========
    private void MoveHeldWithPhysics()
    {
        if (heldObjectRb == null || cameraTransform == null) return;

        Vector3 target = cameraTransform.position
                       + cameraTransform.forward * currentHoldDistance
                       + cameraTransform.up * verticalOffset;

        Vector3 dir = (target - heldObject.transform.position);
        heldObjectRb.linearVelocity = dir * grabSpeed;
    }
}

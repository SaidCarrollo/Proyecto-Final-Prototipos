using System;
using UnityEngine;

public class ObjectGrabber : MonoBehaviour
{
    public event Action<GameObject> OnObjectGrabbed;
    public event Action<GameObject> OnObjectReleased;

    [Header("Grab Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float grabRange = 5f;
    [SerializeField] private float grabSpeed = 15f;
    [SerializeField] private float verticalOffset = 0.1f;
    [SerializeField] private LayerMask interactableLayer;
    [Header("Physics Grab (Fuerzas)")]
    [SerializeField] private float grabPosGain = 100f;  // Fuerza del muelle para seguir la c�mara
    [SerializeField] private float grabVelGain = 20f;   // Amortiguaci�n (evita que vibre)
    [SerializeField] private float grabMaxAccel = 150f; // L�mite de aceleraci�n
    [SerializeField] private float grabMaxSpeed = 4f;   // L�mite de velocidad
    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private float currentHoldDistance = 1.5f;
    public AudioSource Grabaudio;

    // NUEVO: expone el objeto actualmente agarrado
    public GameObject HeldObject => heldObject;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (heldObject == null) TryGrabObject();
            else ReleaseObject();
        }

        if (heldObject != null && Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float minDistance = 0.5f;
            float maxDistance = 1.5f;
            currentHoldDistance = Mathf.Clamp(
                currentHoldDistance - Input.GetAxis("Mouse ScrollWheel") * 2f,
                minDistance, maxDistance);
        }
    }

    void FixedUpdate()
    {
        if (heldObject != null)
        {
            // �S�, siempre queremos moverlo!
            // Esto crear� la "lucha de fuerzas" contra el AnchorFollower.
            MoveObjectWithPhysics();
        }
    }
    private void TryGrabObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, grabRange, interactableLayer))
        {
            heldObject = hit.collider.gameObject;

            // Si estaba anclado, forzar desanclaje antes de agarrar (t� ya lo ten�as)
            AnchorFollower follower = heldObject.GetComponentInParent<AnchorFollower>();
            if (follower != null && follower.IsActive) follower.ForceDetach(); // :contentReference[oaicite:1]{index=1}

            heldObjectRb = heldObject.GetComponent<Rigidbody>();
            if (heldObjectRb != null)
            {
                heldObjectRb.useGravity = false;
                heldObjectRb.freezeRotation = true;
            }

            if (Grabaudio != null) Grabaudio.Play();
            OnObjectGrabbed?.Invoke(heldObject);
        }
    }

    private void ReleaseObject()
    {
        if (heldObject != null)
        {
            // --- INICIO DE LA MODIFICACI�N ---
            // Antes de soltar, comprobar si est� anclado y forzar el desanclaje
            AnchorFollower follower = heldObject.GetComponentInParent<AnchorFollower>();
            if (follower != null && follower.IsActive)
            {
                // Usamos ForceDetach() (que vi en tu script original) para 
                // asegurar que se suelte inmediatamente.
                follower.ForceDetach();
            }
            // --- FIN DE LA MODIFICACI�N ---

            if (heldObjectRb != null)
            {
                heldObjectRb.useGravity = true;
                heldObjectRb.freezeRotation = false;
            }

            OnObjectReleased?.Invoke(heldObject);
            heldObject = null;
        }
    }

    public bool IsHoldingObject() => heldObject != null;

    private void MoveObjectWithPhysics()
    {
        if (heldObjectRb == null) return;

        // 1. Calcular la posici�n objetivo (sin cambios)
        Vector3 targetPosition = cameraTransform.position
                               + cameraTransform.forward * currentHoldDistance
                               + cameraTransform.up * verticalOffset;

        // 2. Calcular la fuerza de muelle (Spring) (sin cambios)
        Vector3 toTarget = targetPosition - heldObject.transform.position;
        Vector3 spring = toTarget * grabPosGain;

        // --- INICIO DE LA MODIFICACI�N ---
        // 3. Calcular el amortiguador (Damper)

        // Comprobar si un ancla ya est� aplicando su propio amortiguador
        AnchorFollower follower = heldObject.GetComponentInParent<AnchorFollower>();

        // El ancla est� amortiguando si est� activa Y est� en modo Dynamic
        bool anchorIsDamping = (follower != null &&
                              follower.IsActive &&
                              follower.followMode == AnchorFollower.FollowMode.Dynamic);

        Vector3 damper = Vector3.zero;
        if (!anchorIsDamping)
        {
            // Si el ancla NO est� activa, aplicamos nuestro propio amortiguador
            damper = -heldObjectRb.linearVelocity * grabVelGain;
        }
        // Si el ancla S� est� activa, 'damper' se queda en Vector3.zero.
        // Dejamos que el AnchorFollower se encargue de TODO el amortiguado.
        // --- FIN DE LA MODIFICACI�N ---

        // 4. Sumar fuerzas (ahora solo suma el muelle si el ancla est� activa)
        Vector3 accel = spring + damper;

        // 5. Limitar aceleraci�n (sin cambios)
        if (accel.sqrMagnitude > grabMaxAccel * grabMaxAccel)
            accel = accel.normalized * grabMaxAccel;

        // 6. Aplicar la fuerza (sin cambios)
        heldObjectRb.AddForce(accel, ForceMode.Acceleration);

        // 7. Limitar velocidad m�xima (sin cambios)
        if (heldObjectRb.linearVelocity.sqrMagnitude > grabMaxSpeed * grabMaxSpeed)
            heldObjectRb.linearVelocity = heldObjectRb.linearVelocity.normalized * grabMaxSpeed;
    }
}
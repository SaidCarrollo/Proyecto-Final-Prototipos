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
    [SerializeField] private float verticalOffset = 0.5f;
    [SerializeField] private LayerMask interactableLayer;

    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private float currentHoldDistance = 2f; // Distancia inicial


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (heldObject == null)
            {
                TryGrabObject();
            }
            else
            {
                ReleaseObject();
            }
        }

        // Ajustar distancia con la rueda del mouse
        if (heldObject != null && Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            currentHoldDistance = Mathf.Clamp(currentHoldDistance - Input.GetAxis("Mouse ScrollWheel") * 2f, 0.5f, 5f);
        }
    }

    void FixedUpdate()
    {
        if (heldObject != null)
        {
            MoveObjectWithPhysics();
        }
    }

    private void TryGrabObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, grabRange, interactableLayer))
        {
            heldObject = hit.collider.gameObject;
            heldObjectRb = heldObject.GetComponent<Rigidbody>();
            heldObjectRb.useGravity = false;

            OutlineObject outline = heldObject.GetComponent<OutlineObject>();
            if (outline != null)
            {
                outline.EnableOutline();
            }

            OnObjectGrabbed?.Invoke(heldObject);
        }
    }

    private void ReleaseObject()
    {
        if (heldObject != null)
        {
            heldObjectRb.useGravity = true;

            // 🔥 Invocar evento
            OnObjectReleased?.Invoke(heldObject);

            heldObject = null;
        }
    }

    public bool IsHoldingObject() => heldObject != null;
    private void MoveObjectWithPhysics()
    {
        // Calcula la posición objetivo (delante de la cámara + offset vertical)
        Vector3 targetPosition = cameraTransform.position +
                               cameraTransform.forward * currentHoldDistance +
                               cameraTransform.up * verticalOffset;

        // Mueve el objeto con física
        Vector3 moveDirection = (targetPosition - heldObject.transform.position);
        heldObjectRb.linearVelocity = moveDirection * grabSpeed;

        // Rotación suave hacia la cámara (opcional)
        Quaternion targetRotation = Quaternion.LookRotation(heldObject.transform.position - cameraTransform.position);
        heldObjectRb.rotation = Quaternion.Slerp(heldObjectRb.rotation, targetRotation, Time.deltaTime * 5f);
    }
}
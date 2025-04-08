using UnityEngine;

public class ObjectGrabber : MonoBehaviour
{
    [Header("Grab Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float grabRange = 5f;
    [SerializeField] private float grabSpeed = 15f; // Más rápido para mejor respuesta
    [SerializeField] private float verticalOffset = 0.5f; // Altura respecto a la cámara
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private LaserPointer laserPointer;

    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private float currentHoldDistance; // Distancia dinámica

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
        if (heldObject != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            currentHoldDistance = Mathf.Clamp(currentHoldDistance - scroll * 2f, 0.5f, 3f); // Rango de distancia
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
            heldObject.GetComponent<OutlineObject>().EnableOutline(); // Activa outline
        }
    }

    private void ReleaseObject()
    {
        heldObject.GetComponent<OutlineObject>().DisableOutline(); // Desactiva outline
        heldObjectRb.useGravity = true;
        heldObject = null;
    }
    public bool IsHoldingObject()
    {
        return heldObject != null;
    }
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
        heldObjectRb.angularVelocity = Vector3.zero; // Elimina rotación no deseada
        heldObjectRb.rotation = Quaternion.Slerp(heldObjectRb.rotation, targetRotation, Time.deltaTime * 5f);
    }

}
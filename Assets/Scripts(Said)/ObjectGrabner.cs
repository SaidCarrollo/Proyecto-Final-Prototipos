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

    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    private float currentHoldDistance = 1.5f;
    public AudioSource Grabaudio;

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

        if (heldObject != null && Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float minDistance = 0.5f;
            float maxDistance = 1.5f;
            currentHoldDistance = Mathf.Clamp(currentHoldDistance - Input.GetAxis("Mouse ScrollWheel") * 2f, minDistance, maxDistance);
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
            if (heldObjectRb != null)
            {
                heldObjectRb.useGravity = false;
                heldObjectRb.freezeRotation = true;
            }

            if (Grabaudio != null)
            {
                Grabaudio.Play();
            }

            OnObjectGrabbed?.Invoke(heldObject);
        }
    }


    private void ReleaseObject()
    {
        if (heldObject != null)
        {
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

        Vector3 targetPosition = cameraTransform.position +
                               cameraTransform.forward * currentHoldDistance +
                               cameraTransform.up * verticalOffset;

        Vector3 moveDirection = (targetPosition - heldObject.transform.position);
        heldObjectRb.linearVelocity = moveDirection * grabSpeed;

    }
}
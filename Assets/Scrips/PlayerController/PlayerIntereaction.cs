
using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactionRange = 3f;

    [SerializeField] private LayerMask interactionLayer;

    [Header("UI")]
    [SerializeField] private InteractionPromptUI interactionPromptUI;

    [Header("Input")]
    [SerializeField] private InputActionReference interactActionReference;

    private Interactable currentInteractable;
    private GameObject lastLookedAtObject = null;
    public ObjectGrabber objectGrabber;
    void Awake()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("PlayerInteraction: Camera Transform not assigned!", this);
            enabled = false;
        }
        if (interactionPromptUI == null)
        {
            Debug.LogError("PlayerInteraction: Interaction Prompt UI not assigned!", this);
        }
    }

    void OnEnable()
    {
        if (interactActionReference != null)
        {
            interactActionReference.action.Enable();
            interactActionReference.action.performed += OnInteractInput;
        }
    }

    void OnDisable()
    {
        if (interactActionReference != null)
        {
            interactActionReference.action.performed -= OnInteractInput;
            interactActionReference.action.Disable();
        }
    }

    void Update()
    {
        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        RaycastHit hit;
        bool foundInteractable = false;
        bool isGrabbable = false;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactionRange, interactionLayer))
        {
            GameObject hitObject = hit.collider.gameObject;
            Interactable interactableComponent = hitObject.GetComponent<Interactable>();
            GrabbableObject grabbableComponent = hitObject.GetComponent<GrabbableObject>();

            if (interactableComponent != null || grabbableComponent != null)
            {
                currentInteractable = interactableComponent;
                foundInteractable = true;
                isGrabbable = grabbableComponent != null;

                if (hitObject != lastLookedAtObject)
                {
                    if (interactionPromptUI != null)
                    {
                        interactionPromptUI.ShowPrompt(isGrabbable);
                    }
                    lastLookedAtObject = hitObject;
                }
            }
        }

        if (!foundInteractable && lastLookedAtObject != null)
        {
            ClearCurrentInteractable();
        }
    }

    private void OnInteractInput(InputAction.CallbackContext context)
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();

        }
    }

    private void ClearCurrentInteractable()
    {
        if (interactionPromptUI != null)
        {
            interactionPromptUI.HidePrompt();
        }
        currentInteractable = null;
        lastLookedAtObject = null;
    }
    public void InteractFromButton()
    {

        if (objectGrabber != null && lastLookedAtObject != null && lastLookedAtObject.GetComponent<GrabbableObject>() != null)
        {
            objectGrabber.TryGrabOrReleaseFromButton();
        }
        else if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }
}

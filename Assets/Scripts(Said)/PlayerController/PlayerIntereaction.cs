using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Objetos Normales (Corta Distancia)")]
    [SerializeField] private float normalInteractionRange = 3f;
    [Tooltip("Capas para objetos agarrables o interactuables normales (Ej: 'Grab', 'Interactable')")]
    [SerializeField] private LayerMask normalLayers;

    [Header("Nodos de Movimiento (Larga Distancia)")]
    [SerializeField] private float waypointRange = 20f;
    [Tooltip("Capa EXCLUSIVA para los botones flotantes (Ej: 'Waypoint')")]
    [SerializeField] private LayerMask waypointLayer;

    [Header("Configuración General")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool showDebugLogs = true;

    [Header("UI")]
    [SerializeField] private InteractionPromptUI interactionPromptUI;
    [SerializeField] private string textoInteractuar = "Interactuar";
    [SerializeField] private string textoSoltar = "Soltar";
    [SerializeField] private string textoViajar = "Ir aquí"; // NUEVO texto para waypoints

    [Header("Input")]
    [SerializeField] private InputActionReference interactActionReference;

    [Header("Grab System")]
    [SerializeField] private ObjectGrabber objectGrabber;

    // --- Variables de Estado ---
    private Interactable currentInteractableLogic;
    private ObjectHighlighter currentHighlighter;
    private GameObject lastHitObject = null;

    void Awake()
    {
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
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
        HandleInteractionDetection();
    }

    private void HandleInteractionDetection()
    {
        // 1. PRIORIDAD: Si tenemos algo agarrado, mostramos "Soltar" y no buscamos más
        if (objectGrabber != null && objectGrabber.IsHoldingObject())
        {
            if (currentHighlighter != null)
            {
                currentHighlighter.SetFocus(false);
                currentHighlighter = null;
                lastHitObject = null;
            }

            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetPromptText(textoSoltar);
                interactionPromptUI.ShowPrompt();
            }
            return;
        }

        // --- 2. SISTEMA DE DOBLE RAYCAST ---
        RaycastHit hit;
        bool hitSomethingValid = false;
        GameObject hitObj = null;
        string textoUI_Sugerido = textoInteractuar;

        // Intento A: Buscar Objetos Normales (Rayo Corto)
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, normalInteractionRange, normalLayers))
        {
            hitObj = hit.collider.gameObject;
            hitSomethingValid = true;
            textoUI_Sugerido = textoInteractuar;

            if (showDebugLogs) Debug.DrawRay(cameraTransform.position, cameraTransform.forward * normalInteractionRange, Color.green);
        }
        // Intento B: Si no encontró nada normal, buscar Waypoints (Rayo Largo)
        else if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, waypointRange, waypointLayer))
        {
            hitObj = hit.collider.gameObject;
            hitSomethingValid = true;
            textoUI_Sugerido = textoViajar; // Cambiamos el texto porque es un nodo de viaje

            if (showDebugLogs) Debug.DrawRay(cameraTransform.position, cameraTransform.forward * waypointRange, Color.cyan);
        }
        else
        {
            // Nada golpeado
            if (showDebugLogs) Debug.DrawRay(cameraTransform.position, cameraTransform.forward * waypointRange, Color.red);
        }

        // --- 3. PROCESAR EL OBJETO GOLPEADO ---
        if (hitSomethingValid)
        {
            if (hitObj != lastHitObject)
            {
                ClearCurrentFocus();
                lastHitObject = hitObj;

                // Buscar Efectos Visuales (Brillo)
                currentHighlighter = hitObj.GetComponent<ObjectHighlighter>();
                if (currentHighlighter == null) currentHighlighter = hitObj.GetComponentInChildren<ObjectHighlighter>();
                if (currentHighlighter == null) currentHighlighter = hitObj.GetComponentInParent<ObjectHighlighter>();

                if (currentHighlighter != null) currentHighlighter.SetFocus(true);

                // Buscar Lógica (Funciona igual para Interactable normal y para AssistedNode3D porque hereda)
                currentInteractableLogic = hitObj.GetComponent<Interactable>();
                if (currentInteractableLogic == null) currentInteractableLogic = hitObj.GetComponentInChildren<Interactable>();
                if (currentInteractableLogic == null) currentInteractableLogic = hitObj.GetComponentInParent<Interactable>();

                // Mostrar UI
                if (interactionPromptUI != null)
                {
                    // Usa el texto adecuado dependiendo de si fue el rayo corto o largo
                    interactionPromptUI.SetPromptText(textoUI_Sugerido);
                    interactionPromptUI.ShowPrompt();
                }
            }
        }
        else if (lastHitObject != null)
        {
            // Perdimos el foco
            ClearCurrentFocus();
        }
    }

    private void ClearCurrentFocus()
    {
        if (interactionPromptUI != null) interactionPromptUI.HidePrompt();

        if (currentHighlighter != null)
        {
            currentHighlighter.SetFocus(false);
        }

        currentHighlighter = null;
        currentInteractableLogic = null;
        lastHitObject = null;
    }

    private void OnInteractInput(InputAction.CallbackContext context)
    {
        if (objectGrabber != null && objectGrabber.IsHoldingObject())
        {
            objectGrabber.Release();
            return;
        }

        if (currentInteractableLogic != null)
        {
            currentInteractableLogic.Interact();
            return;
        }

        if (objectGrabber != null)
        {
            objectGrabber.TryGrab();
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneController : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("El panel de la UI que representa el tel�fono.")]
    [SerializeField] private GameObject phonePanel;

    [Header("Events")]
    [Tooltip("Evento que se dispara para mostrar un mensaje en la UI.")]
    [SerializeField] private GameEventstring messageEvent;

    [Header("Vignette Event")]
    [Tooltip("Evento para activar la vi�eta.")]
    [SerializeField] private VignetteEvent vignetteEvent;

    [Header("Input Actions")]
    [Tooltip("Referencia a la acci�n para mostrar/ocultar el tel�fono.")]
    [SerializeField] private InputActionReference togglePhoneAction;

    [Tooltip("Referencia a la acci�n para interactuar (llamar).")]
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private BadgeManager badgeManager;
    private void OnEnable()
    {
        if (togglePhoneAction != null)
        {
            togglePhoneAction.action.Enable();
            togglePhoneAction.action.performed += OnTogglePhone;
        }
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteract;
        }
    }

    private void OnDisable()
    {
        if (togglePhoneAction != null)
        {
            togglePhoneAction.action.performed -= OnTogglePhone;
            togglePhoneAction.action.Disable();
        }
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
    }

    private void OnTogglePhone(InputAction.CallbackContext context)
    {
        Debug.Log("OnTogglePhone FUE LLAMADO! La tecla 'Q' funciona.");

        if (phonePanel != null)
        {
            Debug.Log($"El estado actual del panel es: {phonePanel.activeSelf}. Se cambiar� a: {!phonePanel.activeSelf}");
            phonePanel.SetActive(!phonePanel.activeSelf);
        }
        else
        {
            Debug.LogError("ERROR: La referencia 'phonePanel' no est� asignada en el Inspector!");
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (phonePanel != null && phonePanel.activeInHierarchy)
        {
            Debug.Log("Llamando al 116 desde el panel...");

            if (messageEvent != null)
            {
                messageEvent.Raise("Llamada de emergencia realizada.");
            }

            if (badgeManager != null)
            {
                badgeManager.UnlockBadge("LlamadaDeEmergencia");
            }
            if (vignetteEvent != null)
            {
                vignetteEvent.Raise(Color.green, 0.4f, 2f); // Color verde, intensidad 0.4, duraci�n 2 segundos
            }

            phonePanel.SetActive(false);
        }
    }
}
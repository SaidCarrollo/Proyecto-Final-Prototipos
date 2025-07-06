
using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneController : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("El panel de la UI que representa el teléfono.")]
    [SerializeField] private GameObject phonePanel;

    [Header("Events")]
    [Tooltip("Evento que se dispara para mostrar un mensaje en la UI.")]
    [SerializeField] private GameEventstring messageEvent;

    [Header("Vignette Event")]
    [Tooltip("Evento para activar la viñeta.")]
    [SerializeField] private VignetteEvent vignetteEvent;

    [Header("Input Actions")]
    [Tooltip("Referencia a la acción para mostrar/ocultar el teléfono.")]
    [SerializeField] private InputActionReference togglePhoneAction;
    [SerializeField] private FirstPersonController playerController;
    [Tooltip("Referencia a la acción para interactuar (llamar).")]
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private BadgeManager badgeManager;
    [SerializeField] private GameManager gameManager;

    [SerializeField] private string goodCallBadgeID = "LlamadaDeEmergenciaCorrecta";
    [SerializeField, TextArea(2, 5)] private string goodCallMessage = "¡Bien hecho! Los bomberos están en camino.";

    [SerializeField] private string badCallBadgeID = "LlamadaDeEmergenciaInnecesaria";
    [SerializeField, TextArea(2, 5)] private string badCallMessage = "El fuego está controlado, no era necesario llamar.";

    [SerializeField] private string distractedBadgeID = "DistraccionConTelefono";
    [SerializeField, TextArea(2, 5)] private string distractedMessage = "Debería enfocarme mientras el fuego está controlado.";

    [SerializeField] private string hesitatedBadgeID = "DudandoEnLlamar";
    [SerializeField, TextArea(2, 5)] private string hesitatedMessage = "¿Por qué lo guardo? ¡Debo llamar a los bomberos ahora!";

    private bool phoneWasOpenedAndNotUsed = false;
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
        if (phonePanel == null)
        {
            Debug.LogError("ERROR: La referencia 'phonePanel' no está asignada en el Inspector!");
            return;
        }

        bool isPhoneActive = phonePanel.activeSelf;
        phonePanel.SetActive(!isPhoneActive);

        if (playerController != null)
        {
            playerController.SetMovementEnabled(isPhoneActive); 
        }

        if (isPhoneActive) 
        {
            if (phoneWasOpenedAndNotUsed)
            {
                if (gameManager.IsFireUncontrolled)
                {
                    vignetteEvent.Raise(Color.red, 0.5f, 3f);
                    badgeManager.UnlockBadge(hesitatedBadgeID);
                    messageEvent.Raise(hesitatedMessage);
                }
                else
                {
                    vignetteEvent.Raise(Color.red, 0.5f, 3f);
                    badgeManager.UnlockBadge(distractedBadgeID);
                    messageEvent.Raise(distractedMessage);
                }
            }
            phoneWasOpenedAndNotUsed = false;
        }
        else 
        {
            phoneWasOpenedAndNotUsed = true;
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (phonePanel == null || !phonePanel.activeInHierarchy)
        {
            return;
        }
        if (playerController != null)
        {
            playerController.SetMovementEnabled(true);
        }
        phoneWasOpenedAndNotUsed = false;

        if (gameManager == null)
        {
            Debug.LogError("GameManager no está asignado en PhoneController.");
            return;
        }

        Debug.Log("Intentando realizar llamada de emergencia...");

        if (gameManager.IsFireUncontrolled)
        {
            Debug.Log("Llamada correcta. El fuego está fuera de control.");

            if (messageEvent != null && !string.IsNullOrEmpty(goodCallMessage))
            {
                messageEvent.Raise(goodCallMessage);
            }

            if (badgeManager != null)
            {
                badgeManager.UnlockBadge(goodCallBadgeID);
            }

            if (vignetteEvent != null)
            {
                vignetteEvent.Raise(Color.green, 0.4f, 2f);
            }
        }
        else
        {
            Debug.Log("Llamada innecesaria. El fuego todavía era controlable.");

            if (messageEvent != null && !string.IsNullOrEmpty(badCallMessage))
            {
                messageEvent.Raise(badCallMessage);
            }

            if (badgeManager != null)
            {
                badgeManager.UnlockBadge(badCallBadgeID);
            }

            if (vignetteEvent != null)
            {
                vignetteEvent.Raise(Color.red, 0.5f, 3f);
            }
        }

        phonePanel.SetActive(false);
    }
}
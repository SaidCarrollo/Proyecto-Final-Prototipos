
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
    [SerializeField] private FirstPersonController playerController;
    [Tooltip("Referencia a la acci�n para interactuar (llamar).")]
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private BadgeManager badgeManager;
    [SerializeField] private GameManager gameManager;

    [SerializeField] private string goodCallBadgeID = "LlamadaDeEmergenciaCorrecta";
    [SerializeField, TextArea(2, 5)] private string goodCallMessage = "�Bien hecho! Los bomberos est�n en camino.";

    [SerializeField] private string badCallBadgeID = "LlamadaDeEmergenciaInnecesaria";
    [SerializeField, TextArea(2, 5)] private string badCallMessage = "El fuego est� controlado, no era necesario llamar.";

    [SerializeField] private string distractedBadgeID = "DistraccionConTelefono";
    [SerializeField, TextArea(2, 5)] private string distractedMessage = "Deber�a enfocarme mientras el fuego est� controlado.";

    [SerializeField] private string hesitatedBadgeID = "DudandoEnLlamar";
    [SerializeField, TextArea(2, 5)] private string hesitatedMessage = "�Por qu� lo guardo? �Debo llamar a los bomberos ahora!";

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
            Debug.LogError("ERROR: La referencia 'phonePanel' no est� asignada en el Inspector!");
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
            Debug.LogError("GameManager no est� asignado en PhoneController.");
            return;
        }

        Debug.Log("Intentando realizar llamada de emergencia...");

        if (gameManager.IsFireUncontrolled)
        {
            Debug.Log("Llamada correcta. El fuego est� fuera de control.");

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
            Debug.Log("Llamada innecesaria. El fuego todav�a era controlable.");

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
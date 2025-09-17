using UnityEngine;
using UnityEngine.Events;

public class WindowInteractable : MonoBehaviour
{
    [Header("System References")]
    [Tooltip("Referencia al BadgeManager para dar logros.")]
    [SerializeField] private BadgeManager badgeManager;
    [Tooltip("Referencia al sistema de viñetas para efectos visuales.")]
    [SerializeField] private VignetteEvent vignetteEvent;
    [Tooltip("Referencia al sistema de mensajes para la UI.")]
    [SerializeField] private GameEventstring messageEvent;
    [SerializeField] private Animator animator;
    [Header("Re-Interaction Settings")]
    [Tooltip("ID del badge que se otorga al interactuar más de una vez.")]
    [SerializeField] private string reInteractBadgeID = "abrirventanadespuesdecerrarla";
    [Tooltip("Mensaje que se muestra en la UI al interactuar más de una vez.")]
    [SerializeField, TextArea(2, 5)] private string reInteractMessage = "No debería estar jugando con la ventana. Necesito concentrarme.";
    private bool windowIsOpen = false;
    [Header("First Interaction")]
    [Tooltip("Evento que se dispara LA PRIMERA VEZ que se interactúa (ej. abrir/cerrar la ventana).")]
    public UnityEvent OnFirstInteract;

    private int interactionCount = 0;
    private bool badOutcomeTriggered = false;

    public void Interact()
    {
        if (badOutcomeTriggered)
        {
            Debug.Log($"'{gameObject.name}' ya ha sido interactuado repetidamente. No se hace nada más.");
            return;
        }

        interactionCount++;
        if (animator != null)
        {
            windowIsOpen = !windowIsOpen;
            animator.SetBool("isOpen", windowIsOpen);
        }

        if (interactionCount == 1)
        {
            Debug.Log($"Primera interacción con '{gameObject.name}'.");
            OnFirstInteract?.Invoke();
        }
        else 
        {
            Debug.Log($"Interacción repetida con '{gameObject.name}'. Disparando evento negativo.");

            if (badgeManager != null)
            {
                badgeManager.UnlockBadge(reInteractBadgeID);
            }

            if (vignetteEvent != null)
            {
                vignetteEvent.Raise(Color.red, 0.5f, 3f);
            }

            if (messageEvent != null && !string.IsNullOrEmpty(reInteractMessage))
            {
                messageEvent.Raise(reInteractMessage);
            }

            badOutcomeTriggered = true;

            Interactable baseInteractable = GetComponent<Interactable>();
            if (baseInteractable != null)
            {
                baseInteractable.DisableInteraction();
            }
        }
    }
}
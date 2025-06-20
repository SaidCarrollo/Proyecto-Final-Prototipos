using UnityEngine;

public class OvenInteractable : MonoBehaviour
{
    [Header("Game State")]
    [Tooltip("Referencia al GameManager para comprobar el estado del juego.")]
    [SerializeField] private GameManager gameManager;

    [Header("Managers & Events")]
    [SerializeField] private BadgeManager badgeManager;
    [SerializeField] private VignetteEvent vignetteEvent;

    [Header("Default Interaction (Antes del descontrol)")]
    [Tooltip("ID del badge que se da por prevenir a tiempo.")]
    [SerializeField] private string defaultBadgeID = "PrevencionHornilla";

    [Header("Late Interaction (Despu�s del descontrol)")]
    [Tooltip("ID del badge que se da por actuar demasiado tarde.")]
    [SerializeField] private string lateBadgeID = "HornillaTarde";

    [Header("Events on Late Interaction")]
    [Tooltip("Evento que se dispara si se interact�a tarde (causa la muerte).")]
    [SerializeField] private GameEvent onPlayerDeathEvent;

    [Tooltip("Mensaje a mostrar si la interacci�n es correcta (a tiempo).")]
    [SerializeField, TextArea(2, 5)] private string successMessage = "�Acci�n preventiva correcta!";
    [Tooltip("Mensaje a mostrar si la interacci�n es incorrecta (tarde).")]
    [SerializeField, TextArea(2, 5)] private string failureMessage = "�Demasiado tarde! El fuego ya es incontrolable.";
    [SerializeField] private GameEventstring messageEvent;
    private bool hasBeenUsed = false;

    public void Interact()
    {
        if (hasBeenUsed)
        {
            Debug.Log("El horno ya ha sido usado, no se puede interactuar de nuevo.");
            return;
        }

        if (gameManager == null || badgeManager == null || vignetteEvent == null)
        {
            Debug.LogError("Faltan referencias en el OvenInteractable. Asigna GameManager, BadgeManager y VignetteEvent.");
            return;
        }

        if (gameManager.IsFireUncontrolled)
        {
            badgeManager.UnlockBadge(lateBadgeID);
            vignetteEvent.Raise(Color.red, 0.5f, 3f);

            if (messageEvent != null && !string.IsNullOrEmpty(failureMessage))
            {
                messageEvent.Raise(failureMessage);
            }
        }
        else
        {
            badgeManager.UnlockBadge(defaultBadgeID);
            vignetteEvent.Raise(Color.green, 0.4f, 2f);

            if (messageEvent != null && !string.IsNullOrEmpty(successMessage))
            {
                messageEvent.Raise(successMessage);
            }
        }

        hasBeenUsed = true;
    }
}
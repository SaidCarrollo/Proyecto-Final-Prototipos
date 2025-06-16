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
    [SerializeField] private Color defaultVignetteColor = Color.green;

    [Header("Late Interaction (Después del descontrol)")]
    [Tooltip("ID del badge que se da por actuar demasiado tarde.")]
    [SerializeField] private string lateBadgeID = "HornillaTarde";
    [SerializeField] private Color lateVignetteColor = Color.red;

    [Header("Events on Late Interaction")]
    [Tooltip("Evento que se dispara si se interactúa tarde (causa la muerte).")]
    [SerializeField] private GameEvent onPlayerDeathEvent;

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
            Debug.Log("Interacción tardía con el horno. El fuego ya está descontrolado.");
            badgeManager.UnlockBadge(lateBadgeID);
            vignetteEvent.Raise(lateVignetteColor, 0.5f, 3f);

            onPlayerDeathEvent?.Raise();
        }
        else
        {
            Debug.Log("Interacción a tiempo con el horno. Acción preventiva correcta.");
            badgeManager.UnlockBadge(defaultBadgeID);
            vignetteEvent.Raise(defaultVignetteColor, 0.4f, 2f);
        }

        hasBeenUsed = true; 
    }
}
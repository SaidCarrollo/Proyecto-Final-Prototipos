using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WindowHazardZone : MonoBehaviour
{
    [Header("Refs (opcional auto-find)")]
    [SerializeField] private UIManager uiManager;          // Para mostrar mensaje y overlay
    [SerializeField] private BadgeManager badgeManager;    // Para desbloquear badge
    [SerializeField] private string hazardBadgeId = "Cerca de la ventana"; // Badge incorrecto del vidrio

    [Header("Efectos")]
    [Tooltip("Da�o que recibe el jugador al herirse.")]
    [SerializeField] private int damageOnHit = 1;

    [Tooltip("Multiplicador de velocidad al quedar herido. 0.6 = 60% de la velocidad de caminar.")]
    [SerializeField] private float injuredWalkMultiplier = 0.6f;

    [Tooltip("Si es true, s�lo se aplica una vez por partida.")]
    [SerializeField] private bool oneShot = true;
    [Header("Checklist opcional")]
    [SerializeField] private ObjectiveChecklistUI objectiveChecklistUI;
    private bool _triggered;

    private void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered && oneShot) return;

        var fpc = other.GetComponentInParent<FirstPersonController>();
        if (fpc != null)
        {
            // Marca espec�ficamente que fue por la ventana
            fpc.MarkWindowInjury();

            // Aplica da�o al sistema de vida + feedback de da�o general
            fpc.TakeDamage(damageOnHit);

            // Aplica la lesi�n permanente: m�s lento y sin correr
            fpc.ApplyPermanentInjury("Me he hecho da�o, ya no puedo correr", injuredWalkMultiplier);
        }
        else
        {
            return;
        }

        // Desbloquear badge espec�fico del peligro ventana
        if (badgeManager != null && !string.IsNullOrEmpty(hazardBadgeId))
        {
            badgeManager.UnlockBadge(hazardBadgeId);
        }

        // Mensaje en pantalla (refuerzo). Esto ya lo hace ApplyPermanentInjury internamente,
        // pero lo dejamos por si quieres sobreescribir el texto aqu�.
        if (uiManager != null)
        {
            uiManager.OnMessageEventRaised("Me he hecho da�o, ya no puedo correr");

            uiManager.ShowWindowInjuryOverlay();
        }

        _triggered = true;
        if (objectiveChecklistUI != null)
            objectiveChecklistUI.ForceFailPendingsAndGoToSecondPhase();
    }
}


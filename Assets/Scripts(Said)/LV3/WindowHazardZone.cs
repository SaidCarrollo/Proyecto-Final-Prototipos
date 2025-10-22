using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WindowHazardZone : MonoBehaviour
{
    [Header("Refs (opcional auto-find)")]
    [SerializeField] private UIManager uiManager;          // Para mostrar el mensaje
    [SerializeField] private BadgeManager badgeManager;    // Para desbloquear badge
    [SerializeField] private string hazardBadgeId = "Cerca de la ventana"; // Crea este ID en tu BadgeManager

    [Header("Efectos")]
    [Tooltip("Daño que recibe el jugador al herirse.")]
    [SerializeField] private int damageOnHit = 1;

    [Tooltip("Multiplicador de velocidad al quedar herido. 0.6 = 60% de la velocidad de caminar.")]
    [SerializeField] private float injuredWalkMultiplier = 0.6f;

    [Tooltip("Si es true, sólo se aplica una vez por partida.")]
    [SerializeField] private bool oneShot = true;

    private bool _triggered;

    private void Reset()
    {
        // Asegura que sea trigger
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (_triggered && oneShot) return;

        var fpc = other.GetComponentInParent<FirstPersonController>();
        if (fpc != null)
        {
            fpc.MarkWindowInjury(); // <-- registra que la herida provino de la ventana
            fpc.TakeDamage(1);      // tu daño actual (mantén tu feedback/viñeta/badge "auch")
        }
        if (fpc == null) return;

        // 1) Daño (esto ya maneja vignette/vida y podría disparar un badge genérico si lo configuras así)
        fpc.TakeDamage(damageOnHit);  // Usa el sistema de vida que ya tienes. :contentReference[oaicite:0]{index=0}

        // 2) Aplicar lesión permanente (más lento y sin correr) + mensaje claro
        fpc.ApplyPermanentInjury("Me he hecho daño, ya no puedo correr", injuredWalkMultiplier);

        // 3) Desbloquear badge específico de este hazard (incorrecto)
        if (badgeManager != null && !string.IsNullOrEmpty(hazardBadgeId))
        {
            badgeManager.UnlockBadge(hazardBadgeId); // Usa tu SO de badges. :contentReference[oaicite:1]{index=1}
        }

        // 4) (Opcional) Reforzar mensaje en UI (sobre-escribe cualquier texto anterior)
        if (uiManager != null)
        {
            uiManager.OnMessageEventRaised("Me he hecho daño, ya no puedo correr"); // Muestra mensaje en pantalla. :contentReference[oaicite:2]{index=2}
        }

        _triggered = true;
    }
}


using UnityEngine;

public class GasLeakScenarioManager : MonoBehaviour
{
    [Header("Referencias de la Escena")]
    [Tooltip("Arrastra aquí el objeto que tiene el script HazardTimer.")]
    [SerializeField] private HazardTimer hazardTimer;

    [Tooltip("Arrastra aquí el objeto GameManager de la escena.")]
    [SerializeField] private GameManager gameManager;

    [Tooltip("Arrastra aquí tu asset de BadgeManager.")]
    [SerializeField] private BadgeManager badgeManager; 

    [Header("Configuración de Badges")]
    [Tooltip("Escribe el ID EXACTO del badge principal que se debe dar al sobrevivir.")]
    [SerializeField] private string principalSuccessBadgeID = "GasVentilado";

    [Header("Configuración del Segundo Temporizador")]
    [Tooltip("Duración en segundos del segundo contador (tanto el mortal como el de supervivencia).")]
    [SerializeField] private float secondTimerDuration = 60f;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private bool gasValveHasBeenClosed = false;   
    [SerializeField] private bool windowHasBeenOpened = false;
//    [SerializeField] private DangerIndicatorUI dangerIndicator;
    private void TryDefuseHazardIfReady()
    {
        if (gasValveHasBeenClosed && windowHasBeenOpened)
        {
            // Idempotente: si ya fue defusado, HazardTimer lo ignora
            hazardTimer.DefuseHazard();
            Debug.Log("ESCENARIO: Ambas acciones listas. ¡Peligro DESACTIVADO!");
        }
    }
    public void MarkGasValveAsClosed()
    {
        if (gasValveHasBeenClosed) return;
        gasValveHasBeenClosed = true;
        Debug.Log("ESCENARIO: La válvula de gas ha sido cerrada.");
        TryDefuseHazardIfReady();
    }

    public void OnWindowOpened()
    {
        if (windowHasBeenOpened) return;
        windowHasBeenOpened = true;
        Debug.Log("ESCENARIO: Ventana abierta.");
        TryDefuseHazardIfReady();
    }

    public void TriggerBadConsequence()
    {
        Debug.Log("ESCENARIO: El primer timer terminó mal. ¡Iniciando contador MORTAL!");
        gameManager.IniciarContadorMortal();
        uiManager?.UpdateObjectiveTextAndFadeLater("Evacua la zona.", 20.5f, 0.6f);
    }

    public void TriggerGoodConsequence()
    {
        Debug.Log("ESCENARIO: El primer timer terminó bien. ¡Iniciando contador de SUPERVIVENCIA!");

        if (badgeManager != null && !string.IsNullOrEmpty(principalSuccessBadgeID))
        {
            badgeManager.UnlockBadge(principalSuccessBadgeID);
            Debug.Log($"BADGE PRINCIPAL DESBLOQUEADO: {principalSuccessBadgeID}");
        }
        else
        {
            Debug.LogWarning("No se ha asignado el BadgeManager o el ID del badge principal en GasLeakScenarioManager.");
        }

        gameManager.IniciarContadorSupervivencia(secondTimerDuration);
        uiManager?.UpdateObjectiveTextAndFadeLater("¡Detuviste la fuga! Ve qué más puedes hacer y evacua el hogar.", 20.5f, 0.6f);

    }
}
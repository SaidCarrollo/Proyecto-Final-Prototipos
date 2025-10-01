// GasLeakScenarioManager.cs (versi�n corregida)

using UnityEngine;

public class GasLeakScenarioManager : MonoBehaviour
{
    [Header("Referencias de la Escena")]
    [Tooltip("Arrastra aqu� el objeto que tiene el script HazardTimer.")]
    [SerializeField] private HazardTimer hazardTimer;

    [Tooltip("Arrastra aqu� el objeto GameManager de la escena.")]
    [SerializeField] private GameManager gameManager;

    [Tooltip("Arrastra aqu� tu asset de BadgeManager.")]
    [SerializeField] private BadgeManager badgeManager; 

    [Header("Configuraci�n de Badges")]
    [Tooltip("Escribe el ID EXACTO del badge principal que se debe dar al sobrevivir.")]
    [SerializeField] private string principalSuccessBadgeID = "GasVentilado";

    [Header("Configuraci�n del Segundo Temporizador")]
    [Tooltip("Duraci�n en segundos del segundo contador (tanto el mortal como el de supervivencia).")]
    [SerializeField] private float secondTimerDuration = 60f;

    private bool gasValveHasBeenClosed = false;

    public void MarkGasValveAsClosed()
    {
        if (gasValveHasBeenClosed) return;
        gasValveHasBeenClosed = true;
        Debug.Log("ESCENARIO: La v�lvula de gas ha sido cerrada.");
    }

    public void OnWindowOpened()
    {
        if (gasValveHasBeenClosed)
        {
            Debug.Log("ESCENARIO: Ventana abierta tras cerrar v�lvula. Desactivando peligro.");
            hazardTimer.DefuseHazard();
        }
        else
        {
            Debug.Log("ESCENARIO: Ventana abierta ANTES de cerrar v�lvula. No hay efecto.");
        }
    }

    public void TriggerBadConsequence()
    {
        Debug.Log("ESCENARIO: El primer timer termin� mal. �Iniciando contador MORTAL!");
        gameManager.IniciarContadorMortal();
    }

    public void TriggerGoodConsequence()
    {
        Debug.Log("ESCENARIO: El primer timer termin� bien. �Iniciando contador de SUPERVIVENCIA!");

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
    }
}
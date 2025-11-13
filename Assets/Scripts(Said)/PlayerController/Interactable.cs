using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Tooltip("La acción a realizar cuando se interactúa con este objeto.")]
    public UnityEvent OnInteract;

    /// <summary>
    /// Bloque global para TODOS los interactables (útil para los 10s de exploración).
    /// </summary>
    public static bool GlobalInteractionEnabled { get; private set; } = true;

    /// <summary>
    /// Bloqueo local por objeto (por si quieres desactivar solo uno).
    /// </summary>
    [SerializeField]
    private bool localInteractionEnabled = true;

    /// <summary>
    /// Estado combinado (global + local).
    /// PlayerInteraction usa esta propiedad para saber si mostrar el prompt.
    /// </summary>
    public bool IsInteractionEnabled => GlobalInteractionEnabled && localInteractionEnabled;

    /// <summary>
    /// Activa / desactiva TODAS las interacciones del juego.
    /// </summary>
    public static void SetGlobalInteractionEnabled(bool enabled)
    {
        GlobalInteractionEnabled = enabled;
        Debug.Log($"[Interactable] Global interactions {(enabled ? "ENABLED" : "DISABLED")}");
    }

    public void Interact()
    {
        // Si el global o el local está bloqueado, no pasa nada
        if (!IsInteractionEnabled)
            return;

        OnInteract?.Invoke();
        Debug.Log($"Interacted with {gameObject.name}");
    }

    // Desactiva solo ESTE interactable (no afecta al resto)
    public void DisableInteraction()
    {
        localInteractionEnabled = false;
        Debug.Log($"Interactions disabled for {gameObject.name}");
    }

    public void EnableInteraction()
    {
        localInteractionEnabled = true;
        Debug.Log($"Interactions enabled for {gameObject.name}");
    }
}

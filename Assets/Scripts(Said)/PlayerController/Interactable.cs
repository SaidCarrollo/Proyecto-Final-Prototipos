using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Tooltip("La acción a realizar cuando se interactúa con este objeto.")]
    public UnityEvent OnInteract;

    [Tooltip("Si marcas esta casilla, el objeto dejará de ser interactuable tras el primer uso.")]
    public bool esDeUnSoloUso = false;

    public bool IsInteractionEnabled { get; private set; } = true;

    // Usamos el script correcto
    private ObjectHighlighter highlighter;

    protected virtual void Awake()
    {
        // Buscamos el script visual (puede estar en el mismo objeto o en un hijo)
        highlighter = GetComponent<ObjectHighlighter>();
        if (highlighter == null) highlighter = GetComponentInChildren<ObjectHighlighter>();
    }

    public virtual void Interact()
    {
        if (!IsInteractionEnabled) return;

        OnInteract?.Invoke();
        Debug.Log($"Interacted with {gameObject.name}");

        // Si es de un solo uso, lo deshabilitamos tras interactuar
        if (esDeUnSoloUso)
        {
            DisableInteraction();
        }
    }

    public void DisableInteraction()
    {
        IsInteractionEnabled = false;

        // 1. Apagar el brillo por completo y desactivar el script de respiración
        if (highlighter != null)
        {
            highlighter.ApagarPorCompleto();
            highlighter.enabled = false;
        }

        // 2. Cambiar la capa a "Default" (Layer 0) para que el Raycast lo ignore.
        gameObject.layer = 0;

        // (Opcional) Si el collider que toca el Raycast está en los hijos, 
        // cambiamos también la capa de todos los hijos.
        foreach (Transform child in transform)
        {
            child.gameObject.layer = 0;
        }

        Debug.Log($"Interactions disabled for {gameObject.name}. Layer changed to Default.");
    }
}
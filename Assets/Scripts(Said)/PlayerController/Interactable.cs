using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Tooltip("La acción a realizar cuando se interactúa con este objeto.")]
    public UnityEvent OnInteract;

    public bool IsInteractionEnabled { get; private set; } = true;

    public void Interact()
    {
        if (!IsInteractionEnabled) return;

        OnInteract?.Invoke();
        Debug.Log($"Interacted with {gameObject.name}");
    }

    public void DisableInteraction()
    {
        IsInteractionEnabled = false;
        Debug.Log($"Interactions disabled for {gameObject.name}");
    }
}
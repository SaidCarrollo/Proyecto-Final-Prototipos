
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Tooltip("La acción a realizar cuando se interactúa con este objeto.")]
    public UnityEvent OnInteract;

    public void Interact()
    {
        OnInteract?.Invoke();
        Debug.Log($"Interacted with {gameObject.name}");
    }
}
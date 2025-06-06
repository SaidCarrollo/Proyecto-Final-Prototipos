
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Tooltip("La acci�n a realizar cuando se interact�a con este objeto.")]
    public UnityEvent OnInteract;

    public void Interact()
    {
        OnInteract?.Invoke();
        Debug.Log($"Interacted with {gameObject.name}");
    }
}
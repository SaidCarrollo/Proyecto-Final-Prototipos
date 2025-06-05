
using UnityEngine;
using UnityEngine.Events; 

public class Interactable : MonoBehaviour
{
    [Tooltip("The action(s) to perform when this object is interacted with.")]
    public UnityEvent OnInteract; 

    public void Interact()
    {
        OnInteract?.Invoke(); 
        Debug.Log($"Interacted with {gameObject.name}");
    }

}
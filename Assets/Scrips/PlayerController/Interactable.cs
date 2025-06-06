
using UnityEngine;
using UnityEngine.Events; 

public class Interactable : MonoBehaviour
{
    [SerializeField] private FloatEvent ScoreUpdate;
    [Tooltip("The action(s) to perform when this object is interacted with.")]
    public UnityEvent OnInteract; 

    public void Interact()
    {
        ScoreUpdate.Raise(4);
        OnInteract?.Invoke(); 
        Debug.Log($"Interacted with {gameObject.name}");
    }

}
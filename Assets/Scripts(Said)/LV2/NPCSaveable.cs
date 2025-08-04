using UnityEngine;
using UnityEngine.Events;

public class NPCSaveable : MonoBehaviour
{
    [Tooltip("Manager que lleva la cuenta de todos los NPCs.")]
    [HideInInspector] public NPCSaveableManager manager;

    [Tooltip("Evento que se dispara cuando este NPC es salvado. Útil para sonidos o animaciones.")]
    public UnityEvent OnSaved;

    public bool IsSaved { get; private set; } = false;

    public void SaveNPC()
    {
        if (IsSaved) return;

        IsSaved = true;
        OnSaved?.Invoke(); 
        Debug.Log($"<color=cyan>NPC salvado:</color> {gameObject.name}");

        if (manager != null)
        {
            manager.CheckForAllSaved();
        }
    }
}
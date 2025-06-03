// GameEventListener.cs
using UnityEngine;
using UnityEngine.Events; // Necesario para UnityEvent

public class GameEventListener : MonoBehaviour
{
    [Tooltip("Evento al que suscribirse.")]
    public GameEvent Event;

    [Tooltip("Respuesta a invocar cuando el evento se dispara.")]
    public UnityEvent Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised()
    {
        Response.Invoke();
    }
}
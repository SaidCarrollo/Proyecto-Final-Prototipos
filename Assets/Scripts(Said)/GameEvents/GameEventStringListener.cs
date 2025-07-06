
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class StringUnityEvent : UnityEvent<string> { }

public class GameEventStringListener : MonoBehaviour
{
    [Tooltip("El evento de string al que suscribirse.")]
    public GameEventstring Event;

    [Tooltip("La respuesta a invocar cuando el evento se dispara. Recibirá el string del evento.")]
    public StringUnityEvent Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(string value)
    {
        Response.Invoke(value);
    }
}
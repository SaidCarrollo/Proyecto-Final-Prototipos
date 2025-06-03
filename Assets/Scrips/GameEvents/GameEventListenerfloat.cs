// FloatEventListener.cs
using UnityEngine;
using UnityEngine.Events; // Necesario para UnityEvent<T>

// Evento de Unity que puede tomar un float como argumento
[System.Serializable]
public class GameEventFloat : UnityEvent<float> { }

public class GameEventListenerfloat : MonoBehaviour
{
    [Tooltip("Evento al que suscribirse.")]
    public FloatEvent Event;

    [Tooltip("Respuesta a invocar cuando el evento se dispara.")]
    public GameEventFloat Response;

    private void OnEnable()
    {
        if (Event != null) Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        if (Event != null) Event.UnregisterListener(this);
    }

    public void OnEventRaised(float value)
    {
        Response.Invoke(value);
    }
}
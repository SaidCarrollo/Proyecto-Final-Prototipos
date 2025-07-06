
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class VignetteUnityEvent : UnityEvent<Color, float, float> { }

public class VignetteEventListener : MonoBehaviour
{
    [Tooltip("El evento de viñeta al que suscribirse.")]
    public VignetteEvent Event;

    [Tooltip("La respuesta a invocar cuando el evento se dispara.")]
    public VignetteUnityEvent Response;

    private void OnEnable()
    {
        Event.RegisterListener(this);
    }

    private void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised(Color color, float intensity, float duration)
    {
        Response.Invoke(color, intensity, duration);
    }
}
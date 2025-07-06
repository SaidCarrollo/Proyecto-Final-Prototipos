
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game Events/Vignette Event")]
public class VignetteEvent : ScriptableObject
{
    private readonly List<VignetteEventListener> listeners = new List<VignetteEventListener>();

    public void Raise(Color color, float intensity, float duration)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventRaised(color, intensity, duration);
        }
    }

    public void RegisterListener(VignetteEventListener listener)
    {
        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }

    public void UnregisterListener(VignetteEventListener listener)
    {
        if (listeners.Contains(listener))
            listeners.Remove(listener);
    }
}
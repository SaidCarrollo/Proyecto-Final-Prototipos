// FloatEvent.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game Event/Float Event")]
public class FloatEvent : ScriptableObject
{
    private List<GameEventListenerfloat> listeners = new List<GameEventListenerfloat>();

    public void Raise(float value)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventRaised(value);
        }
    }

    public void RegisterListener(GameEventListenerfloat listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }

    public void UnregisterListener(GameEventListenerfloat listener)
    {
        if (listeners.Contains(listener))
        {
            listeners.Remove(listener);
        }
    }
}
// StringEvent.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Events/String Event")]
public class GameEventstring : ScriptableObject
{
    private readonly List<GameEventStringListener> listeners = new List<GameEventStringListener>();

    public void Raise(string value)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventRaised(value);
        }
    }

    public void RegisterListener(GameEventStringListener listener)
    {
        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }

    public void UnregisterListener(GameEventStringListener listener)
    {
        if (listeners.Contains(listener))
            listeners.Remove(listener);
    }
}
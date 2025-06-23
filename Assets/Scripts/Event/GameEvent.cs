using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "Event/GameEvent")]
public class GameEvent : ScriptableObject
{
    private readonly HashSet<GameEventListener> listeners = new HashSet<GameEventListener>();

    public void OnSubscribe(GameEventListener listener) {
        listeners.Add(listener);
    }

    public void OnUnsubscribe(GameEventListener listener) {
        listeners.Remove(listener);
    }

    public void BroadCast() {
        foreach (GameEventListener listener in listeners) {
            listener.OnEventRaised();
        }
    }
}

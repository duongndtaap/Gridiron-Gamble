using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [SerializeField] GameEvent gameEvent;
    [SerializeField] UnityEvent reponse;

    private void OnEnable() {
        gameEvent.OnSubscribe(this);
    }

    private void OnDisable() {
        gameEvent.OnUnsubscribe(this);
    }

    public void OnEventRaised() {
        reponse?.Invoke();
    }
}

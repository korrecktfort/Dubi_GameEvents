using Dubi.GameEvents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameEventValue
{
    [SerializeField] bool muted = false;
    [SerializeField] GameEvent gameEvent = null;

    public GameEvent GameEvent { get => gameEvent;}

    public void Instantiate()
    {
        this.gameEvent = ScriptableObject.Instantiate(this.gameEvent);
    }

    public void OnAwake(GameObject gameObject)
    {
        if (this.muted) return;
        this.gameEvent?.OnAwake(gameObject);
    }

    public void OnEnabled(GameObject gameObject)
    {
        if(this.muted) return;
        this.gameEvent?.OnEnabled(gameObject);
    }

    public void OnDisabled(GameObject gameObject)
    {
        if(this.muted) return;
        this.gameEvent?.OnDisabled(gameObject);
    }

    public bool CallEvent()
    {
        if (this.muted) return true;

        if(this.gameEvent == null)        
        {
#if UNITY_EDITOR
            Debug.LogWarning("Empty game event.");
#endif
            return false;
        }    

        return this.gameEvent.CallEvent();
    }
}

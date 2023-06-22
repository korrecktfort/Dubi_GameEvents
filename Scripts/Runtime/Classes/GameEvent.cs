using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Dubi.GameEvents
{
    [DenyAsSubAsset, CreateAssetMenu(menuName ="Dubi/Game Events/Game Event")]
    public class GameEvent : ScriptableObject
    {     
        public bool LockValuesOnSubAsset => this.lockValuesOnSubAsset;
        [SerializeField] bool lockValuesOnSubAsset = false;

        public float Delay => this.delay;
        [SerializeField] float delay = 0.0f;

        public virtual void OnAwake(GameObject gameObject) { }        

        public virtual void OnEnabled(GameObject gameObject) { }

        public virtual void OnDisabled(GameObject gameObject) { }

        public virtual bool CallEvent() { return true; }
    }
}

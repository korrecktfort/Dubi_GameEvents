using Dubi.GameEvents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dubi.GameEvents
{
    [CreateAssetMenu(menuName ="Dubi/Game Events/Debug Message")]
    public class DebugMessage : GameEvent
    {
        [SerializeField, ShowOnSubAsset] string debugMessage = "";

        public override bool CallEvent()
        {
            Debug.Log(this.debugMessage);
            return true;
        }
    }
}

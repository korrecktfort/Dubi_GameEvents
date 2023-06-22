using UnityEngine;

namespace Dubi.GameEvents
{
    [RequireObjectComponent(typeof(ObjectEventsComponent))]
    public class CallObjectEvent : GameEvent
    {
        [SerializeField, HideInInspector] int selectedIndex = -1;
        [SerializeField, HideInInspector] ObjectEventsComponent objectEvents = null;

        public override void OnAwake(GameObject gameObject)
        {
            this.objectEvents = gameObject.GetComponent<ObjectEventsComponent>();
        }

        public override bool CallEvent()
        {
            this.objectEvents?.ObjectEvents?.Call(this.selectedIndex);
            return true;
        }
    }
}

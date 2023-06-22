using UnityEngine;

namespace Dubi.GameEvents
{
    [System.Serializable]
    public class ObjectEventsComponent : MonoBehaviour
    {
        public ObjectEvents ObjectEvents { get => objectEvents; }

        [SerializeField] ObjectEvents objectEvents = new ObjectEvents();
    }
}

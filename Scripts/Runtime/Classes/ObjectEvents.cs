using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Dubi.GameEvents
{
    [System.Serializable]
    public class ObjectEvents
    {
        public string[] EventNames 
        { 
            get 
            { 
                List<string> list = new List<string>();

                foreach(ObjectEvent objectEvent in objectEvents)
                    list.Add(objectEvent.Name);

                return list.ToArray();
            }         
        }

        [SerializeField] ObjectEvent[] objectEvents = new ObjectEvent[0];

        public void Call(int index)
        {
            if (index >= 0 && index < objectEvents.Length)
                objectEvents[index].Events.Invoke();
        }

        public void Call(string name)
        {
            foreach(ObjectEvent objectEvent in objectEvents)
                if(objectEvent.Name == name)
                {
                    objectEvent.Events.Invoke();
                    return;
                }
        }
    }

    [System.Serializable]
    public class ObjectEvent
    {
        [SerializeField] string name = "New Object Event";
        [SerializeField] UnityEvent events;

        public UnityEvent Events { get => events;}
        public string Name { get => name;}
    }
}

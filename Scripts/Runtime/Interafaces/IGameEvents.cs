using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Dubi.GameEvents
{
    /// <summary>
    /// Use this interface if you intend to call game events for proper
    /// functionality
    /// </summary>
    public interface IGameEvents
    {
        public GameEvent[] GameEvents
        {
            get
            {
                List<GameEvent> list = new List<GameEvent>();
                foreach (GameEventValue gameEventValue in this.GameEventValues)
                    list.Add(gameEventValue.GameEvent);

                return list.ToArray();
            }
        }

        public GameEventValue[] GameEventValues { get;}

        public bool CallEvents();
    }
}
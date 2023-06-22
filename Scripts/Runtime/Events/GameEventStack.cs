using Dubi.GameEvents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dubi.GameEvents
{
    [CreateAssetMenu(menuName ="Dubi/Game Events/Game Events Stack"), DenyAsSubAsset]
    public class GameEventStack : GameEvent, IGameEvents
    {
        public GameEventValue[] GameEventValues { get => gameEventValues;}       

        [SerializeField, ShowOnSubAsset] GameEventValue[] gameEventValues = new GameEventValue[0];

        public override void OnAwake(GameObject gameObject)
        {
            foreach (GameEventValue gameEventValue in gameEventValues)
                gameEventValue?.Instantiate();


            foreach (GameEventValue gameEventValue in gameEventValues)
                gameEventValue.OnAwake(gameObject);
        }

        public override void OnEnabled(GameObject gameObject)
        {
            foreach (GameEventValue gameEventValue in this.gameEventValues)
                gameEventValue.OnEnabled(gameObject);
        }

        public override void OnDisabled(GameObject gameObject)
        {
            foreach (GameEventValue gameEventValue in this.gameEventValues)
                gameEventValue.OnDisabled(gameObject);
        }

        public bool CallEvents()
        {
            foreach (GameEventValue gameEventValue in this.gameEventValues)
            {
                if (gameEventValue == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Empty game event value: " + this.name + "(Scriptable Object)");
#endif
                    continue;
                }

                if (!gameEventValue.CallEvent())
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Game event returned false " + this.name + "(Scriptable Object)");
#endif
                    return false;
                }
            }

            return true;
        }
    }
}

using UnityEngine;

namespace Dubi.GameEvents
{
    /// <summary>
    /// Use this component to call game events.
    /// </summary>
    [System.Serializable]
    public class GameEventsComponent : MonoBehaviour, IGameEvents
    {      
        public GameEventValue[] GameEventValues { get => gameEventValues; }

        [SerializeField] GameEventValue[] gameEventValues = new GameEventValue[0];

        GameObject ownGameObject = null;        

        private void Awake()
        {
            this.ownGameObject = this.gameObject;

            foreach (GameEventValue gameEventValue in gameEventValues)
                gameEventValue?.Instantiate();


            foreach (GameEventValue gameEventValue in gameEventValues)
                gameEventValue.OnAwake(this.ownGameObject);
        }

        private void OnEnable()
        {
            foreach (GameEventValue gameEventValue in this.gameEventValues)
                gameEventValue.OnEnabled(this.ownGameObject);
        }

        private void OnDisable()
        {
            foreach (GameEventValue gameEventValue in this.gameEventValues)
                gameEventValue.OnDisabled(this.ownGameObject);
        }

        public bool CallEvents()
        {
            foreach (GameEventValue gameEventValue in this.gameEventValues)
            {
                if (gameEventValue == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Empty game event value: " + this.ownGameObject);
#endif
                    continue;
                }

                if(!gameEventValue.CallEvent())
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Game event returned false " + this.ownGameObject);
#endif
                    return false;
                }
            }

            return true;
        }       
    }
}

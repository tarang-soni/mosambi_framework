using UnityEngine;
using Mosambi.Events;

namespace Mosambi.Core
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Core Dependencies")]
        [SerializeField] private GameStateChannelSO gameStateChannel;
        // [SerializeField] private LevelManager levelManager;
        // [SerializeField] private AdManager adManager;

        private void Start()
        {
            // 1. Initialize any core systems here via manual Dependency Injection
            // levelManager.Initialize();
            // adManager.Initialize();

            // 2. Kick off the game flow
            // The UIManager is listening to this channel, so it will automatically 
            // show the Lobby screen the moment this event is raised.
            Debug.Log("[GameBootstrapper] Systems Initialized. Entering Lobby State.");
            gameStateChannel.RaiseEvent(GameState.Lobby);
        }

        public void OnPlayerCrossedFinishLine()
        {
            gameStateChannel.RaiseEvent(GameState.LevelComplete);
        }
    }
}
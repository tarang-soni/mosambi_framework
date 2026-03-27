using UnityEngine;
using VContainer.Unity;
using Mosambi.Events;

namespace Mosambi.Core.DI
{
    // IStartable tells VContainer: "Run this immediately after building the DI container"
    public class GameFlowStarter : IStartable
    {
        private readonly GameStateChannelSO gameStateChannel;

        // VContainer automatically reads this constructor and hands it the Channel
        public GameFlowStarter(GameStateChannelSO gameStateChannel)
        {
            this.gameStateChannel = gameStateChannel;
        }

        public void Start()
        {
            Debug.Log("[GameFlow] All dependencies injected. Kicking off the Lobby state.");
            gameStateChannel.RaiseEvent(GameState.Lobby);
        }
    }
}
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Mosambi.UI;

namespace Mosambi.Lobby
{
    public class LobbyLifetimeScope : LifetimeScope
    {
        [Header("Lobby Dependencies")]
        [SerializeField] private UIManager lobbyUIManager;

        protected override void Configure(IContainerBuilder builder)
        {
            // 1. Safety Check
            if (lobbyUIManager == null)
            {
                Debug.LogError("[LobbyLifetimeScope] UIManager is missing in the Inspector!");
                return;
            }

            builder.RegisterComponent(lobbyUIManager).As<IUIManager>();
        }
    }
}
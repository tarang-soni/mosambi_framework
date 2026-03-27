using UnityEngine;
using VContainer;
using VContainer.Unity;
using Mosambi.Core.Loading;

namespace Mosambi.Core.DI
{
    // IStartable is a VContainer interface. 
    // It guarantees Start() will ONLY run after every single manager is fully loaded and injected.
    public class Bootstrapper : IStartable
    {
        private readonly ISceneLoader _sceneLoader;

        [Inject]
        public Bootstrapper(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public void Start()
        {
            Debug.Log("[Bootstrapper] Core Scope Initialized. Firing Scene Transition...");
            
            // This name must exactly match the name of your Lobby scene in the Build Settings
            _sceneLoader.LoadSceneAsync("1_Lobby"); 
        }
    }
}
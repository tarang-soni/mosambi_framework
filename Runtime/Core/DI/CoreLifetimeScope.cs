using Mosambi.Core.Loading;
using Mosambi.Core.Pooling;
using Mosambi.Core.UI;
using Mosambi.Events;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VContainer.Unity.Extensions;

namespace Mosambi.Core.DI
{
    public class CoreLifetimeScope : PersistentLifetimeScope<CoreLifetimeScope>
    {
        [Header("--- ARCHITECTURE NOTE ---")]
        [Tooltip("This scene does not use a GameManager. It is driven by the pure C# class: Bootstrapper.cs")]
        [SerializeField] private string entryPoint = "Bootstrapper.cs";

        [Header("Global Managers")]

        [SerializeField] private PoolManager poolManager;
        [SerializeField] private LoadingScreenView loadingScreenView;
        [Header("Global Assets")]
        [SerializeField] private GameStateChannelSO gameStateChannel;
        [SerializeField] private MosambiSecuritySettings securitySettings;

        protected override void Configure(IContainerBuilder builder)
        {
            // 1. Safety Check to prevent cryptic null errors
            if (securitySettings == null || poolManager == null || gameStateChannel == null)
            {
                Debug.LogError("[CoreLifetimeScope] Missing core references in the Inspector!");
                return;
            }

            // 2. Register Pure C# Services
            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();

            // 3. Register Global MonoBehaviours (Attached to the [CORE] object)
            builder.Register<ISaveManager<GameSaveData>>(c =>
            new SecureFileSaveManager<GameSaveData>(securitySettings),
            Lifetime.Singleton);
            builder.RegisterComponent(poolManager).As<IPoolManager>();
            builder.RegisterComponent(loadingScreenView);
            // 4. Register Global ScriptableObjects
            builder.RegisterInstance(gameStateChannel);

            // 5. Register the Engine Starter
            // VContainer builds this class in the background and calls its Start() method
            builder.RegisterEntryPoint<Bootstrapper>();
        }
    }
}
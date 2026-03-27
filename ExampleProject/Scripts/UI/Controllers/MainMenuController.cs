using Mosambi.Core;
using Mosambi.Core.Loading;
using Mosambi.Events;
using Mosambi.UI;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI.Controllers
{
    public class MainMenuController : MonoBehaviour, IRequireUIManager
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;

        private GameStateChannelSO _gameStateChannel;
        private ISceneLoader _sceneLoader;
        private IUIManager uiManager;

        // Injected automatically by the UIManager on startup
        public void InjectManager(IUIManager manager)
        {
            uiManager = manager;
        }

        // VContainer automatically calls this and hands you the dependencies
        [Inject]
        public void Construct(GameStateChannelSO gameStateChannel, ISceneLoader sceneLoader)
        {
            _gameStateChannel = gameStateChannel;
            _sceneLoader = sceneLoader;

            // SAFE ZONE: We know 100% that dependencies are loaded right now.
            // Wake up the Lobby UI immediately.
            Debug.Log("[MainMenuController] Dependencies injected. Waking up Lobby State.");
            _gameStateChannel.RaiseEvent(GameState.Lobby);
        }

        private void OnEnable()
        {
            // This is perfectly safe here because playButton is an Inspector reference, 
            // not an injected dependency. It is always ready before OnEnable.
            if (playButton != null) playButton.onClick.AddListener(OnPlayButtonClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnDisable()
        {
            if (playButton != null) playButton.onClick.RemoveListener(OnPlayButtonClicked);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }

        public async void OnPlayButtonClicked()
        {
            Debug.Log("[MainMenu] Play clicked. Loading Stage...");

            // Prevent double-clicking while it loads
            playButton.interactable = false;

            await _sceneLoader.LoadSceneAsync("2_Stage");
        }

        private void OnSettingsClicked()
        {
            if (uiManager != null)
            {
                uiManager.ShowScreen<Views.SettingsScreen>();
            }
        }
    }
}
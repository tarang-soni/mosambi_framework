using Mosambi.Core;
using Mosambi.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Mosambi.UI
{
    [Serializable]
    public struct UIStateMapping
    {
        [Tooltip("The game state that triggers this screen.")]
        public GameState State;
        [Tooltip("The specific screen to show. Drag the script from the scene hierarchy here.")]
        public UIView ScreenInstance;
    }

    public class UIManager : MonoBehaviour, IUIManager
    {
        [Header("Event Channels")]
        private GameStateChannelSO gameStateChannel;

        [Header("State to Screen Bindings")]
        [SerializeField] private List<UIStateMapping> stateBindings = new List<UIStateMapping>();

        private Dictionary<Type, UIView> viewDictionary = new Dictionary<Type, UIView>();

        // Navigation state
        private List<UIView> screenHistory = new List<UIView>();
        private Stack<UIView> popupStack = new Stack<UIView>();

        private IObjectResolver container;
        [Inject]
        public void Construct(IObjectResolver container,GameStateChannelSO gameStateChannel )
        {
            this.gameStateChannel = gameStateChannel;
            this.container = container;
        }
        private void Awake()
        {
            RegisterAllViews();
        }
        private void Start()
        {
            container.InjectGameObject(this.gameObject);
        }
        private void OnEnable()
        {
            if (gameStateChannel != null)
                gameStateChannel.OnStateChanged += HandleGameStateChange;
        }

        private void OnDisable()
        {
            if (gameStateChannel != null)
                gameStateChannel.OnStateChanged -= HandleGameStateChange;
        }

        private void RegisterAllViews()
        {
            // 1. Register the Views
            UIView[] views = GetComponentsInChildren<UIView>(true);
            foreach (var view in views)
            {
                view.Hide();
                viewDictionary[view.GetType()] = view;
            }

            // 2. Inject the Manager into the Controllers
            IRequireUIManager[] controllers = GetComponentsInChildren<IRequireUIManager>(true);
            foreach (var controller in controllers)
            {
                controller.InjectManager(this);
            }
        }

        public T GetView<T>() where T : UIView
        {
            if (viewDictionary.TryGetValue(typeof(T), out UIView view))
                return (T)view;

            Debug.LogError($"[UIManager] View {typeof(T).Name} not found.");
            return null;
        }

        // Generic version for explicit manual calls
        public void ShowScreen<T>() where T : UIView
        {
            ShowScreen(typeof(T));
        }

        // Non-generic version for dynamic data-driven calls
        private void ShowScreen(Type viewType)
        {
            if (!viewDictionary.TryGetValue(viewType, out UIView targetView))
            {
                Debug.LogError($"[UIManager] Cannot show screen. Type {viewType.Name} not found in registered views.");
                return;
            }

            // Anti-Cyclic Check
            int historyIndex = screenHistory.FindIndex(v => v.GetType() == viewType);

            if (historyIndex != -1)
            {
                // Screen exists in history. Pop everything above it.
                for (int i = screenHistory.Count - 1; i > historyIndex; i--)
                {
                    screenHistory[i].Hide();
                    screenHistory.RemoveAt(i);
                }
            }
            else
            {
                // New screen. Hide current top screen (if any) and add to history.
                if (screenHistory.Count > 0)
                {
                    screenHistory[^1].Hide();
                }
                screenHistory.Add(targetView);
            }

            targetView.Show();
        }

        public void ShowPopup<T>() where T : UIView
        {
            UIView popup = GetView<T>();
            if (popup != null)
            {
                popupStack.Push(popup);
                popup.Show();
            }
        }

        public void GoBack()
        {
            if (popupStack.Count > 0)
            {
                UIView popup = popupStack.Pop();
                popup.Hide();
                return;
            }

            if (screenHistory.Count > 1)
            {
                UIView currentScreen = screenHistory[^1];
                currentScreen.Hide();
                screenHistory.RemoveAt(screenHistory.Count - 1);

                UIView previousScreen = screenHistory[^1];
                previousScreen.Show();
            }
            else
            {
                Debug.LogWarning("[UIManager] Nowhere left to go back to.");
            }
        }

        // 100% Decoupled State Handling
        private void HandleGameStateChange(GameState newState)
        {
            foreach (var binding in stateBindings)
            {
                if (binding.State == newState && binding.ScreenInstance != null)
                {
                    ShowScreen(binding.ScreenInstance.GetType());
                    return; // Found the match, execute and exit
                }
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Mosambi.UI;
using Mosambi.Core;

namespace Game.UI.Controllers
{
    [RequireComponent(typeof(Views.SettingsScreen))]
    public class SettingsController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button soundToggleButton;
        [SerializeField] private Button hapticsToggleButton;

        private IUIManager uiManager;
        private ISaveManager<GameSaveData> saveManager;
        private Views.SettingsScreen view;

        // --- VCONTAINER MAGIC HAPPENS HERE ---
        [Inject]
        public void Construct(IUIManager uiManager, ISaveManager<GameSaveData> saveManager)
        {
            this.uiManager = uiManager;
            this.saveManager = saveManager;
        }

        private void Awake()
        {
            view = GetComponent<Views.SettingsScreen>();
        }

        private void Start()
        {
            // We know saveManager is completely safe to use here because 
            // VContainer guarantees it was injected before Start().
            //view.SetSoundVisualState(saveManager.Data.isSoundOn);
            //view.SetHapticsVisualState(saveManager.Data.isHapticsOn);
        }

        private void OnEnable()
        {
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
            if (soundToggleButton != null) soundToggleButton.onClick.AddListener(OnSoundToggled);
            if (hapticsToggleButton != null) hapticsToggleButton.onClick.AddListener(OnHapticsToggled);
        }

        private void OnDisable()
        {
            if (backButton != null) backButton.onClick.RemoveListener(OnBackClicked);
            if (soundToggleButton != null) soundToggleButton.onClick.RemoveListener(OnSoundToggled);
            if (hapticsToggleButton != null) hapticsToggleButton.onClick.RemoveListener(OnHapticsToggled);
        }

        private void OnSoundToggled()
        {
            saveManager.Data.isSoundOn = !saveManager.Data.isSoundOn;
            saveManager.Save();
            //view.SetSoundVisualState(saveManager.Data.isSoundOn);
        }

        private void OnHapticsToggled()
        {
            saveManager.Data.isHapticsOn = !saveManager.Data.isHapticsOn;
            saveManager.Save();
            //view.SetHapticsVisualState(saveManager.Data.isHapticsOn);
        }

        private void OnBackClicked()
        {
            uiManager.GoBack();
        }
    }
}
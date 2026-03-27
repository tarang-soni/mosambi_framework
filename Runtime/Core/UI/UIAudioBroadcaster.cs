using UnityEngine;
using UnityEngine.UI;
using Mosambi.Events;
using Mosambi.Core.Audio;

namespace Mosambi.UI.Audio
{
    // This forces Unity to guarantee a Button component exists on this object
    [RequireComponent(typeof(Button))]
    public class UIAudioBroadcaster : MonoBehaviour
    {
        [Header("Broadcasting")]
        [SerializeField] private AudioEventChannelSO uiChannel;
        [SerializeField] private AudioEventSO clickSound;

        private Button _button;

        private void Awake()
        {
            // Automatically grab the button on this GameObject
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            // Automatically listen for the click
            _button.onClick.AddListener(PlaySound);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(PlaySound);
        }

        private void PlaySound()
        {
            if (uiChannel != null && clickSound != null)
            {
                uiChannel.RaiseEvent(clickSound);
            }
        }
    }
}
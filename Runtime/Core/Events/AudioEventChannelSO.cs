using System;
using UnityEngine;
using Mosambi.Core.Audio; // We need this to pass the audio data payload

namespace Mosambi.Events
{
    [CreateAssetMenu(fileName = "NewAudioEventChannel", menuName = "Mosambi/Events/Audio Event Channel")]
    public class AudioEventChannelSO : ScriptableObject
    {
        // The receiver (AudioManager) subscribes to this
        public event Action<AudioEventSO> OnAudioRequested;

        // Your gameplay scripts call this
        public void RaiseEvent(AudioEventSO audioData)
        {
            if (audioData != null)
            {
                OnAudioRequested?.Invoke(audioData);
            }
            else
            {
                // Crucial fallback so a missing sound doesn't crash the game silently
                Debug.LogWarning("[AudioEventChannel] A script attempted to broadcast a null AudioEventSO.");
            }
        }
    }
}
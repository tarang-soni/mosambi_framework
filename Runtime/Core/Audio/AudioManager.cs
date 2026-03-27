using System.Collections.Generic;
using UnityEngine;
using Mosambi.Events;

namespace Mosambi.Core.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Listening Channels (The Receivers)")]
        [SerializeField] private AudioEventChannelSO sfxChannel;
        [SerializeField] private AudioEventChannelSO uiChannel;
        [SerializeField] private AudioEventChannelSO musicChannel;

        [Header("Optimization")]
        [Tooltip("How many SFX can play simultaneously before the pool has to expand?")]
        [SerializeField] private int initialSfxPoolSize = 10;

        private AudioSource _musicSource;
        private AudioSource _uiSource;
        private List<AudioSource> _sfxPool = new List<AudioSource>();

        private void Awake()
        {
            // 1. Setup dedicated sources for single-track audio
            _musicSource = gameObject.AddComponent<AudioSource>();
            _uiSource = gameObject.AddComponent<AudioSource>();

            // 2. Pre-warm the Object Pool for overlapping sounds (explosions, gem breaks)
            for (int i = 0; i < initialSfxPoolSize; i++)
            {
                CreateNewSfxSource();
            }
        }

        private AudioSource CreateNewSfxSource()
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            _sfxPool.Add(newSource);
            return newSource;
        }

        private void OnEnable()
        {
            if (sfxChannel != null) sfxChannel.OnAudioRequested += PlaySFX;
            if (uiChannel != null) uiChannel.OnAudioRequested += PlayUI;
            if (musicChannel != null) musicChannel.OnAudioRequested += PlayMusic;
        }

        private void OnDisable()
        {
            if (sfxChannel != null) sfxChannel.OnAudioRequested -= PlaySFX;
            if (uiChannel != null) uiChannel.OnAudioRequested -= PlayUI;
            if (musicChannel != null) musicChannel.OnAudioRequested -= PlayMusic;
        }

        private void PlaySFX(AudioEventSO audioData)
        {
            if (audioData == null) return;

            // Find an idle source in the pool
            AudioSource availableSource = null;
            foreach (var source in _sfxPool)
            {
                if (!source.isPlaying)
                {
                    availableSource = source;
                    break;
                }
            }

            // Expand pool dynamically if a massive chain reaction happens
            if (availableSource == null)
            {
                availableSource = CreateNewSfxSource();
                Debug.LogWarning("[AudioManager] SFX Pool expanded to handle high volume of simultaneous sounds.");
            }

            // Feed the blank source to the ScriptableObject to configure and play
            audioData.ApplyAndPlay(availableSource);
        }

        private void PlayUI(AudioEventSO audioData)
        {
            if (audioData != null) audioData.ApplyAndPlay(_uiSource);
        }

        private void PlayMusic(AudioEventSO audioData)
        {
            if (audioData != null) audioData.ApplyAndPlay(_musicSource);
        }
    }
}
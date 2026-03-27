using UnityEngine;
using UnityEngine.Audio;

namespace Mosambi.Core.Audio
{
    [CreateAssetMenu(fileName = "NewAudioEvent", menuName = "Mosambi/Audio/Audio Event")]
    public class AudioEventSO : ScriptableObject
    {
        [Header("The Sound")]
        public AudioClip[] clips;
        
        [Header("Routing")]
        public AudioMixerGroup mixerGroup;

        [Header("Configuration")]
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 2f)] public float minPitch = 0.95f;
        [Range(0f, 2f)] public float maxPitch = 1.05f;
        public bool loop = false;

        [Header("3D Spatial Settings (For Level Objects)")]
        [Range(0f, 1f)] public float spatialBlend = 0f; // 0 is 2D (UI), 1 is 3D (World)
        public float minDistance = 1f;
        public float maxDistance = 50f;

        // The SO configures the raw AudioSource just like UOP's ApplyTo() method
        public void ApplyAndPlay(AudioSource source, Vector3 position = default)
        {
            if (clips.Length == 0) return;

            source.clip = clips[Random.Range(0, clips.Length)];
            source.outputAudioMixerGroup = mixerGroup;
            source.volume = volume;
            source.pitch = Random.Range(minPitch, maxPitch);
            source.loop = loop;

            // 3D Settings
            source.spatialBlend = spatialBlend;
            if (spatialBlend > 0)
            {
                source.transform.position = position;
                source.minDistance = minDistance;
                source.maxDistance = maxDistance;
                source.rolloffMode = AudioRolloffMode.Logarithmic;
            }

            source.Play();
        }
    }
}
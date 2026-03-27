namespace Mosambi.Core.Audio
{
    public interface IAudioManager
    {
        void PlaySFX(AudioEventSO audioEvent);
        void PlayUI(AudioEventSO audioEvent);
        void PlayMusic(AudioEventSO audioEvent);
        void StopMusic();
    }
}
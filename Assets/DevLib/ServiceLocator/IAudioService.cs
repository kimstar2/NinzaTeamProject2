using DevLib.SoundSystem.Runtime;

namespace DevLib.ServiceLocator
{
    public interface IAudioService
    {
        void Play(SoundClipSO clipData, int channel = 0);
        void StopSfx(int channel);

        void StopBgm();
        void PlayBgm(SoundClipSO clipData, float fadeIn);
        void FadeOutBgm(float duration);
    }
}

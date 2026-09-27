using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using UnityEngine;

namespace Members.KJY._01.Scripts.UI
{
    public sealed class SceneBgm : MonoBehaviour
    {
        [SerializeField] private SoundClipSO bgm;
        private IAudioService _audioService;
        private bool _playing;

        private void OnEnable() => SceneTransition.BeforeSceneLoad += StopBgm;

        private void Start()
        {
            if (bgm == null || bgm.clip == null || bgm.audioType != DevLib.SoundSystem.Runtime.AudioType.Music)
            {
                Debug.LogError("Assign a Music SoundClipSO with an audio clip.", this);
                return;
            }

            _audioService = ServiceLocator.Get<IAudioService>();
            _audioService.Play(bgm);
            _playing = true;
        }

        private void OnDisable()
        {
            SceneTransition.BeforeSceneLoad -= StopBgm;
            StopBgm();
        }

        private void StopBgm()
        {
            if (!_playing) return;
            _playing = false;
            _audioService.StopBgm();
        }
    }
}

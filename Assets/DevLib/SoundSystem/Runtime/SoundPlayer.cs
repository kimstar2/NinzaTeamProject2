using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace DevLib.SoundSystem.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup uiGroup;
        
        private AudioSource _audioSource;
        private Coroutine _stopRoutine;

        public event Action<SoundPlayer> OnSoundFinished;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void PlaySound(SoundClipSO clipData)
        {
            ForceStopSound();
            if (clipData.audioType == AudioType.Sfx)
            {
                _audioSource.outputAudioMixerGroup = sfxGroup;
            }else if (clipData.audioType == AudioType.Music)
            {
                _audioSource.outputAudioMixerGroup = musicGroup;
            }
            else if (clipData.audioType == AudioType.UI)
            {
                _audioSource.outputAudioMixerGroup = uiGroup;
            }
            
            _audioSource.volume = clipData.volume;
            _audioSource.pitch = clipData.pitch;

            if (clipData.randomizePitch)
            {
                _audioSource.pitch += Random.Range(-clipData.randomPitchModifier, clipData.randomPitchModifier);
            }

            _audioSource.clip = clipData.clip;
            _audioSource.loop = clipData.isLoop;
            
            float startTime = clipData.startTime;
            float endTime = clipData.endTime;
            
            _audioSource.timeSamples = Mathf.RoundToInt(startTime * clipData.clip.frequency);
            _audioSource.Play();

            if (!clipData.isLoop)
            {
                float duration = (endTime - startTime) / Mathf.Abs(_audioSource.pitch);
                _stopRoutine = StartCoroutine(DisableSoundTimer(duration + 0.2f));
            }
        }

        private IEnumerator DisableSoundTimer(float time)
        {
            yield return new WaitForSeconds(time);
            _stopRoutine = null;
            if (_audioSource == null) yield break;
            _audioSource.Stop();
            OnSoundFinished?.Invoke(this);
        }

        public AudioClip CurrentClip => _audioSource != null ? _audioSource.clip : null;
        public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;

        private Coroutine _fadeRoutine;

        // unscaled 시간 기준
        public void FadeTo(float target, float duration)
        {
            if (_audioSource == null) return;
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            if (duration <= 0f || !isActiveAndEnabled)
            {
                _audioSource.volume = target;
                _fadeRoutine = null;
                return;
            }
            _fadeRoutine = StartCoroutine(Fade(target, duration));
        }

        public void SetVolume(float volume)
        {
            if (_fadeRoutine != null) { StopCoroutine(_fadeRoutine); _fadeRoutine = null; }
            if (_audioSource != null) _audioSource.volume = volume;
        }

        private IEnumerator Fade(float target, float duration)
        {
            float start = _audioSource.volume;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                _audioSource.volume = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }
            _audioSource.volume = target;
            _fadeRoutine = null;
        }

        private void OnDisable() => ForceStopSound();
        
        public void ForceStopSound()
        {
            if (_stopRoutine != null)
            {
                StopCoroutine(_stopRoutine);
                _stopRoutine = null;
            }

            if (_audioSource != null)
                _audioSource.Stop();
        }
    }
}

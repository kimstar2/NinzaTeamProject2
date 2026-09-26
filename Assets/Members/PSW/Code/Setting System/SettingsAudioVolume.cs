using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Members.PSW.Code.SettingSystem
{
    [DisallowMultipleComponent]
    public sealed class SettingsAudioVolume : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string volumeParameter = "MasterVolume";
        [SerializeField] private string preferenceKey = "audio.master";
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TMP_Text valueLabel;

        private const float MinimumDecibels = -80f;

        private void OnEnable()
        {
            if (volumeSlider == null || valueLabel == null)
            {
                Debug.LogError("Assign the audio volume slider and value label.", this);
                enabled = false;
                return;
            }

            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = SettingsStore.MaximumVolume;
            volumeSlider.wholeNumbers = true;
            UpdateView(SettingsStore.LoadVolume(preferenceKey));
            volumeSlider.onValueChanged.AddListener(ApplyVolume);
        }

        public void RestoreSavedVolume()
        {
            // Called by the active persistence owner, even while this tab is hidden.
            float value = SettingsStore.LoadVolume(preferenceKey);
            UpdateView(value);
            ApplyMixerVolume(value);
        }

        private void OnDisable()
        {
            if (volumeSlider != null)
                volumeSlider.onValueChanged.RemoveListener(ApplyVolume);
        }

        private void ApplyVolume(float value)
        {
            float volume = Mathf.Clamp(value, 0f, SettingsStore.MaximumVolume);
            UpdateView(volume);
            if (ApplyMixerVolume(volume))
                SettingsStore.StoreVolume(preferenceKey, volume);
        }

        private void UpdateView(float value)
        {
            volumeSlider.SetValueWithoutNotify(value);
            valueLabel.SetText("{0}%", value);
        }

        private bool ApplyMixerVolume(float volume)
        {
            float decibels = volume <= 0f
                ? MinimumDecibels
                : Mathf.Max(MinimumDecibels, 20f * Mathf.Log10(volume / SettingsStore.DefaultVolume));

            if (audioMixer == null || !audioMixer.SetFloat(volumeParameter, decibels))
            {
                Debug.LogError($"Assign an AudioMixer with an exposed volume parameter named '{volumeParameter}'.", this);
                volumeSlider.interactable = false;
                enabled = false;
                return false;
            }

            return true;
        }
    }
}

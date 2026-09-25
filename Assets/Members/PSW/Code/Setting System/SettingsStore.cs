using UnityEngine;

namespace Members.PSW.Code.SettingSystem
{
    // Keep storage details here so the UI does not depend on PlayerPrefs.
    public static class SettingsStore
    {
        public const float DefaultVolume = 100f;
        public const float MaximumVolume = 150f;

        private const string KeyPrefix = "PSW.Settings.v1.";
        private const string DisplayModeKey = KeyPrefix + "displayMode";

        public static float LoadVolume(string channelKey)
        {
            float value = PlayerPrefs.GetFloat(KeyPrefix + channelKey, DefaultVolume);
            return float.IsNaN(value) || float.IsInfinity(value)
                ? DefaultVolume
                : Mathf.Clamp(value, 0f, MaximumVolume);
        }

        public static void StoreVolume(string channelKey, float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return;

            PlayerPrefs.SetFloat(KeyPrefix + channelKey, Mathf.Clamp(value, 0f, MaximumVolume));
        }

        public static int LoadDisplayMode(int fallback)
        {
            int value = PlayerPrefs.GetInt(DisplayModeKey, fallback);
            return value >= 0 && value <= 2 ? value : fallback;
        }

        public static void StoreDisplayMode(int value)
        {
            if (value >= 0 && value <= 2)
                PlayerPrefs.SetInt(DisplayModeKey, value);
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }
    }
}

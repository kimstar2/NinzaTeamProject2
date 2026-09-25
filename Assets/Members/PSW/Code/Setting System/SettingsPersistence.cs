using UnityEngine;

namespace Members.PSW.Code.SettingSystem
{
    // Restore after MMSoundManager's default-order Start loads its own settings.
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class SettingsPersistence : MonoBehaviour
    {
        [SerializeField] private SettingsWindow settingsWindow;
        [SerializeField] private SettingsAudioVolume[] audioVolumes;
        [SerializeField] private SettingsDisplayMode displayMode;

        private void Start()
        {
            foreach (SettingsAudioVolume volume in audioVolumes)
                volume.RestoreSavedVolume();

            displayMode.RestoreSavedMode();
        }

        private void OnEnable()
        {
            settingsWindow.Closing += SettingsStore.Save;
        }

        private void OnDisable()
        {
            settingsWindow.Closing -= SettingsStore.Save;
            SettingsStore.Save();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                SettingsStore.Save();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
                SettingsStore.Save();
        }

        private void OnApplicationQuit()
        {
            SettingsStore.Save();
        }
    }
}

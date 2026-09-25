using TMPro;
using UnityEngine;

namespace Members.PSW.Code.SettingSystem
{
    [DisallowMultipleComponent]
    public sealed class SettingsDisplayMode : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown modeDropdown;

        private void OnEnable()
        {
            if (modeDropdown == null)
            {
                Debug.LogError("Assign the display mode dropdown.", this);
                enabled = false;
                return;
            }

            modeDropdown.SetValueWithoutNotify(SettingsStore.LoadDisplayMode(CurrentModeIndex()));
            modeDropdown.onValueChanged.AddListener(ApplyMode);
        }

        private static int CurrentModeIndex()
        {
            return Screen.fullScreenMode switch
            {
                FullScreenMode.Windowed => 0,
                FullScreenMode.ExclusiveFullScreen => 2,
                _ => 1
            };
        }

        private void OnDisable()
        {
            if (modeDropdown != null)
                modeDropdown.onValueChanged.RemoveListener(ApplyMode);
        }

        private void ApplyMode(int index)
        {
            SetScreenMode(index);
            SettingsStore.StoreDisplayMode(index);
            SettingsStore.Save();
        }

        public void RestoreSavedMode()
        {
            int index = SettingsStore.LoadDisplayMode(CurrentModeIndex());
            modeDropdown.SetValueWithoutNotify(index);
            SetScreenMode(index);
        }

        private static void SetScreenMode(int index)
        {
            switch (index)
            {
                case 0:
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    break;
                case 1:
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    break;
                case 2:
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                    break;
            }
        }
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Members.PSW.Code.SettingSystem
{
    [DisallowMultipleComponent]
    public sealed class SettingsDisplayMode : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown modeDropdown;
        [SerializeField] private TMP_Dropdown resolutionDropdown;

        private readonly List<Vector2Int> _resolutions = new();

        private void OnEnable()
        {
            if (modeDropdown == null || resolutionDropdown == null)
            {
                Debug.LogError("Assign the display mode and resolution dropdowns.", this);
                enabled = false;
                return;
            }

            RefreshOptions();
            modeDropdown.onValueChanged.AddListener(ApplyMode);
            resolutionDropdown.onValueChanged.AddListener(ApplyResolution);
        }

        private void RefreshOptions()
        {
            _resolutions.Clear();
            foreach (Resolution resolution in Screen.resolutions)
            {
                Vector2Int size = new Vector2Int(resolution.width, resolution.height);
                if (size.x > 0 && size.y > 0 && !_resolutions.Contains(size))
                    _resolutions.Add(size);
            }

            // Keep custom window sizes and the Editor Game view selectable too.
            Vector2Int current = new Vector2Int(Mathf.Max(1, Screen.width), Mathf.Max(1, Screen.height));
            if (!_resolutions.Contains(current))
                _resolutions.Add(current);

            _resolutions.Sort((a, b) => a.x != b.x ? b.x.CompareTo(a.x) : b.y.CompareTo(a.y));
            var options = new List<string>(_resolutions.Count);
            foreach (Vector2Int size in _resolutions)
                options.Add($"{size.x} x {size.y}");

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
            Vector2Int saved = SettingsStore.LoadResolution(current);
            int index = _resolutions.IndexOf(saved);
            // A different monitor may no longer support the saved resolution.
            resolutionDropdown.SetValueWithoutNotify(index >= 0 ? index : _resolutions.IndexOf(current));
            modeDropdown.SetValueWithoutNotify(SettingsStore.LoadDisplayMode(CurrentModeIndex()));
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
            if (resolutionDropdown != null)
                resolutionDropdown.onValueChanged.RemoveListener(ApplyResolution);
        }

        private void ApplyMode(int index)
        {
            if (index < 0 || index > 2)
                return;

            SettingsStore.StoreDisplayMode(index);
            ApplySelectedSettings();
            SettingsStore.Save();
        }

        private void ApplyResolution(int index)
        {
            if (index < 0 || index >= _resolutions.Count)
                return;

            SettingsStore.StoreResolution(_resolutions[index]);
            ApplySelectedSettings();
            SettingsStore.Save();
        }

        public void RestoreSavedSettings()
        {
            // Refreshing options must not trigger saves or intermediate screen changes.
            modeDropdown.onValueChanged.RemoveListener(ApplyMode);
            resolutionDropdown.onValueChanged.RemoveListener(ApplyResolution);
            RefreshOptions();
            ApplySelectedSettings();
            if (isActiveAndEnabled)
            {
                modeDropdown.onValueChanged.AddListener(ApplyMode);
                resolutionDropdown.onValueChanged.AddListener(ApplyResolution);
            }
        }

        private void ApplySelectedSettings()
        {
            Vector2Int size = _resolutions[resolutionDropdown.value];
            FullScreenMode mode = modeDropdown.value switch
            {
                0 => FullScreenMode.Windowed,
                2 => FullScreenMode.ExclusiveFullScreen,
                _ => FullScreenMode.FullScreenWindow
            };
            // Apply both together; fullScreenMode changes take effect at frame end.
            Screen.SetResolution(size.x, size.y, mode);
        }
    }
}

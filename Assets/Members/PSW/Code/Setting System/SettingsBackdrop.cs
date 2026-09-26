using UnityEngine;
using UnityEngine.UI;

namespace Members.PSW.Code.SettingSystem
{
    [DisallowMultipleComponent]
    public sealed class SettingsBackdrop : MonoBehaviour
    {
        [SerializeField] private SettingsWindow settingsWindow;
        [SerializeField] private Image background;

        private Color _visibleColor;

        private void Awake()
        {
            _visibleColor = background.color;
            Hide();
        }

        private void OnEnable()
        {
            settingsWindow.Arrived += Show;
            settingsWindow.Closing += Hide;
            settingsWindow.Closed += Hide;
        }

        private void OnDisable()
        {
            settingsWindow.Arrived -= Show;
            settingsWindow.Closing -= Hide;
            settingsWindow.Closed -= Hide;
            Hide();
        }

        private void Show()
        {
            background.color = _visibleColor;
        }

        private void Hide()
        {
            // Keep the transparent Image raycastable during either transition.
            background.color = new Color(_visibleColor.r, _visibleColor.g, _visibleColor.b, 0f);
        }
    }
}

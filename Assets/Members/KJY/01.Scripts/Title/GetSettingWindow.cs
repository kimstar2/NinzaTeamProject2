using Members.PSW.Code.SettingSystem;
using UnityEngine;

namespace Members.KJY._01.Scripts.Title
{
    public class GetSettingWindow : MonoBehaviour
    {
        private SettingsWindow _settingsWindow;
        public bool IsOpen => _settingsWindow != null && _settingsWindow.IsOpen;

        private void Start()
        {
            _settingsWindow = FindAnyObjectByType<SettingsWindow>();
        }

        public void Open()
        {
            if (_settingsWindow != null) _settingsWindow.Open();
        }
        
        public void Close()
        {
            if (_settingsWindow != null) _settingsWindow.Close();
        }
    }
}

using System;
using Members.PSW.Code.SettingSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Members.KJY._01.Scripts.Title
{
    public class GetSettingWindow : MonoBehaviour
    {
        private SettingsWindow _settingsWindow; 
        private void Start()
        {
            _settingsWindow = FindAnyObjectByType<SettingsWindow>();
        }

        public void Open()
        {
            _settingsWindow.Open();
        }
        
        public void Close()
        {
            _settingsWindow.Close();
        }
    }
}
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace DeveloperTools
{
    // TMP reads IME through BaseInput. Bridge only this tool's input module to the new Input System.
    public sealed class DeveloperImeInput : BaseInput
    {
        private Keyboard _keyboard;
        private string _composition = "";
        private IMECompositionMode _mode = IMECompositionMode.Auto;
        private Vector2 _cursor;
        private bool _previousIme;

        public override string compositionString => _composition;
        public override bool touchSupported => false;
        public override IMECompositionMode imeCompositionMode
        {
            get => _mode;
            set
            {
                _mode = value;
                _keyboard?.SetIMEEnabled(value == IMECompositionMode.On ||
                    (value == IMECompositionMode.Auto && _previousIme));
            }
        }
        public override Vector2 compositionCursorPos
        {
            get => _cursor;
            set { _cursor = value; _keyboard?.SetIMECursorPosition(value); }
        }

        protected override void OnEnable() { base.OnEnable(); BindKeyboard(); }
        private void Update() { if (_keyboard != Keyboard.current) BindKeyboard(); }
        private void BindKeyboard()
        {
            ReleaseKeyboard();
            _keyboard = Keyboard.current;
            if (_keyboard == null) return;
            _previousIme = _keyboard.imeSelected.isPressed;
            _keyboard.onIMECompositionChange += OnComposition;
            if (_mode == IMECompositionMode.On) _keyboard.SetIMEEnabled(true);
        }
        private void OnComposition(IMECompositionString value) => _composition = value.ToString();
        private void ReleaseKeyboard()
        {
            if (_keyboard != null)
            {
                _keyboard.onIMECompositionChange -= OnComposition;
                _keyboard.SetIMEEnabled(_previousIme);
            }
            _keyboard = null;
            _composition = "";
        }
        protected override void OnDisable() { ReleaseKeyboard(); base.OnDisable(); }
    }
}

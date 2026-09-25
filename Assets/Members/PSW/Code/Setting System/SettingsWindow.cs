using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Members.PSW.Code.SettingSystem
{
    [DisallowMultipleComponent]
    public sealed class SettingsWindow : MonoBehaviour
    {
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private RectTransform windowPanel;
        [SerializeField] private CanvasGroup windowGroup;
        [SerializeField, Min(0.01f)] private float openDuration = 0.4f;
        [SerializeField, Range(0.01f, 1f)] private float startScale = 0.15f;

        public event Action Arrived;
        public event Action Closing;
        public event Action Closed;

        private Controls _controls;
        private Tween _transitionTween;
        private Vector3 _openScale;
        private bool _isClosing;

        private void Awake()
        {
            _controls = new Controls();
            _openScale = windowPanel.localScale;
        }

        private void OnEnable()
        {
            _controls.UI.Cancel.performed += HandleToggle;
            _controls.UI.Cancel.Enable();
        }

        private void OnDisable()
        {
            _controls.UI.Cancel.performed -= HandleToggle;
            _controls.UI.Cancel.Disable();
            _transitionTween?.Kill();
            CompleteClose();
        }

        private void OnDestroy()
        {
            _controls.Dispose();
        }

        public void Open()
        {
            if (!isActiveAndEnabled || (windowRoot.activeSelf && !_isClosing))
                return;

            _transitionTween?.Kill();
            _isClosing = false;
            if (!windowRoot.activeSelf)
            {
                windowPanel.localScale = _openScale * startScale;
                windowGroup.alpha = 0f;
            }
            windowRoot.SetActive(true);

            _transitionTween = DOTween.Sequence()
                .Append(windowPanel.DOScale(_openScale, openDuration).SetEase(Ease.OutCubic))
                .Join(windowGroup.DOFade(1f, openDuration).SetEase(Ease.OutCubic))
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _transitionTween = null;
                    Arrived?.Invoke();
                });
        }

        public void Close()
        {
            if (!windowRoot.activeSelf || _isClosing)
                return;

            _transitionTween?.Kill();
            _isClosing = true;

            _transitionTween = DOTween.Sequence()
                .Append(windowPanel.DOScale(_openScale * startScale, openDuration).SetEase(Ease.InCubic))
                .Join(windowGroup.DOFade(0f, openDuration).SetEase(Ease.InCubic))
                .SetUpdate(true)
                .OnComplete(CompleteClose);

            Closing?.Invoke();
        }

        private void CompleteClose()
        {
            _transitionTween = null;
            _isClosing = false;
            bool wasOpen = windowRoot.activeSelf;
            windowRoot.SetActive(false);
            windowPanel.localScale = _openScale;
            windowGroup.alpha = 1f;
            if (wasOpen)
                Closed?.Invoke();
        }

        private void HandleToggle(InputAction.CallbackContext context)
        {
            if (windowRoot.activeSelf && !_isClosing)
                Close();
            else
                Open();
        }
    }
}

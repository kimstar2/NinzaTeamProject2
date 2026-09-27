using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Members.KJY._01.Scripts.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class UIButtonSound : MonoBehaviour
    {
        [SerializeField] private SoundClipSO clickSound;
        private Button _button;

        private void Awake() => _button = GetComponent<Button>();

        private void OnEnable() => _button.onClick.AddListener(PlayClick);

        private void OnDisable() => _button.onClick.RemoveListener(PlayClick);

        private void PlayClick()
        {
            ServiceLocator.Get<IAudioService>().Play(clickSound);
        }
    }
}

using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using UnityEngine;
using UnityEngine.Events;

namespace Members.KJY._01.Scripts.Title
{
    public class TitleMenu : MonoBehaviour
    {
        [SerializeField] private SoundClipSO btnClick;
        
        public UnityEvent onMenuOpen;
        public void Set()
        {
            ServiceLocator.Get<IAudioService>().Play(btnClick);
            onMenuOpen?.Invoke();
        }
    }
}
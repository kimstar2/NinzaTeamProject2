using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using UnityEngine;

namespace Members.KJY._01.Scripts.UI
{
    // 페이드 아웃은 SceneTransition 담당
    public sealed class SceneBgm : MonoBehaviour
    {
        [SerializeField] private SoundClipSO bgm;

        private void Start()
        {
            if (bgm == null || bgm.clip == null || bgm.audioType != DevLib.SoundSystem.Runtime.AudioType.Music)
            {
                Debug.LogError("Assign a Music SoundClipSO with an audio clip.", this);
                return;
            }

            ServiceLocator.Get<IAudioService>().PlayBgm(bgm, SceneTransition.BgmFadeIn);
        }
    }
}

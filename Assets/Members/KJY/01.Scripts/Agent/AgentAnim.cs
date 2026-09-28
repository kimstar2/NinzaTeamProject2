using System;
using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using DevLib.SoundSystem.Runtime;
using Members.KJY._01.Scripts.Util;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent
{
    public class AgentAnim : MonoModule , IAnimatable , IAnimatorTrigger
    {
        public Animator Animator {get; private set;}

        private void Awake()
        {
            Animator = GetComponent<Animator>();
        }
        
        public void RenderClip(int hash)
        {
            Animator.Play(hash,0,0);
        }

        public void RenderClipIfNotPlaying(int hash)
        {
            if (hash == NoneHash.Value) return;
            if (Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != hash)
                RenderClip(hash);
        }

        public void SetController(AnimatorOverrideController controller)
        {
            Animator.runtimeAnimatorController = controller;
            Animator.Rebind();
            Animator.Update(0f);
        }

        public event Action OnAnimFinished;
        public event Action OnAttack;

        public void AnimFinishedEnd()
        {
            OnAnimFinished?.Invoke();
        }

        public void AnimOnAttack()
        {
            OnAttack?.Invoke();
        }

        // 클립의 각 휘두르기 프레임에서 호출한다. 피해 판정 이벤트와 분리해 연속 검격도 재생한다.
        public void PlayAnimationSound(SoundClipSO sound)
        {
            if (sound == null || sound.clip == null) return;
            ServiceLocator.Get<IAudioService>().Play(sound);
        }
    }
}

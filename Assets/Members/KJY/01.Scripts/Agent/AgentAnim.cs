using System;
using DevLib.ModuleSystem;
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

        public event Action OnAnimFinished;
        public void AnimFinishedEnd()
        {
            Debug.Log("anim finished");
            OnAnimFinished?.Invoke();
        }
    }
}
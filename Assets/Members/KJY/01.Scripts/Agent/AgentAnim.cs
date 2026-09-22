using System;
using DevLib.ModuleSystem;
using Members.KJY._01.Scripts.Util;
using UnityEditor.Animations;
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
            Debug.Log("Onset");
            Animator.runtimeAnimatorController = controller;
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
    }
}
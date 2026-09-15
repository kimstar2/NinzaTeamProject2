using System;
using DevLib.ModuleSystem;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent
{
    public class AgentAnim : MonoModule , IAnimatable
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
            if (Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != hash)
                RenderClip(hash);
        }

    }
}
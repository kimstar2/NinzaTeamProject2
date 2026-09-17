using UnityEditor.Animations;
using UnityEngine;

namespace Members.KJY._01.Scripts.Agent
{
    public interface IAnimatable
    {
        Animator Animator { get; }
        void RenderClip(int hash);
        void RenderClipIfNotPlaying(int hash);
        void SetController(AnimatorController controller);
    }
}
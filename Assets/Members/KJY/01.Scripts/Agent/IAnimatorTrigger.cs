using System;

namespace Members.KJY._01.Scripts.Agent
{
    public interface IAnimatorTrigger
    {
        event Action OnAnimFinished;
        event Action OnAttack;
        void AnimFinishedEnd();
        void AnimOnAttack();
    }
}